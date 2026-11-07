// Copyright (c) Code Impressions, LLC. All Rights Reserved.
//  
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Delivery;
using Transmitly.Util;

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	/// <param name="mailgunOptions">Mailgun options.</param>
	public sealed class EmailDeliveryStatusReportAdaptor(MailgunOptions mailgunOptions) : IChannelProviderDeliveryReportRequestAdaptor
	{
		private readonly MailgunOptions _mailgunOptions = Guard.AgainstNull(mailgunOptions);

		/// <exception cref="DeliveryReportRequestValidationException">The request is a Mailgun webhook but its signature is invalid.</exception>
		public Task<IReadOnlyCollection<DeliveryReport>?> AdaptAsync(IRequestAdaptorContext adaptorContext)
		{
			if (!ShouldAdapt(adaptorContext))
				return Task.FromResult<IReadOnlyCollection<DeliveryReport>?>(null);

			var payload = ParsePayload(adaptorContext);
			if (payload == null)
				return Task.FromResult<IReadOnlyCollection<DeliveryReport>?>(null);

			ValidateSignature(payload);

			var detail = payload.DeliveryMessage ?? payload.DeliveryReason ?? payload.DeliveryDescription;
			var report = new DeliveryReport(
					DeliveryReport.Event.StatusChanged(),
					Id.Channel.Email(),
					MailgunConstant.Id,
					adaptorContext.PipelineIntent,
					adaptorContext.PipelineId,
					payload.MessageId ?? adaptorContext.ResourceId,
					ConvertStatus(payload.Event, payload.DeliveryCode, detail),
					null,
					null,
					null
				).ApplyExtendedProperties(payload);

			return Task.FromResult<IReadOnlyCollection<DeliveryReport>?>(new List<DeliveryReport> { report }.AsReadOnly());
		}

		private static bool ShouldAdapt(IRequestAdaptorContext adaptorContext)
		{
			if (adaptorContext == null)
				return false;

			var channelId = GetContextValue(adaptorContext, DeliveryUtil.ChannelIdKey);
			var providerId = GetContextValue(adaptorContext, DeliveryUtil.ChannelProviderIdKey);

			return
				(channelId?.Equals(Id.Channel.Email(), StringComparison.InvariantCultureIgnoreCase) ?? false) &&
				(providerId?.StartsWith(MailgunConstant.Id, StringComparison.InvariantCultureIgnoreCase) ?? false);
		}

		// https://documentation.mailgun.com/docs/mailgun/user-manual/webhooks/securing-webhooks
		// hex(HMAC-SHA256(signingKey, timestamp + token)) == signature
		private void ValidateSignature(MailgunEmailWebhookEvent payload)
		{
			if (!_mailgunOptions.ValidateDeliveryReportSignature)
				return;
			if (string.IsNullOrWhiteSpace(_mailgunOptions.WebhookSigningKey))
				throw new DeliveryReportRequestValidationException("Mailgun WebhookSigningKey is required to validate webhooks.");

			using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_mailgunOptions.WebhookSigningKey));
			var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload.SignatureTimestamp + payload.SignatureToken));
#if NET9_0_OR_GREATER
			var expected = Convert.ToHexStringLower(hash);
#else
			var expected = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
#endif
			if (payload.SignatureTimestamp == null || payload.SignatureToken == null || !FixedTimeEquals(expected, payload.SignatureValue?.ToLowerInvariant()))
				throw new DeliveryReportRequestValidationException("Mailgun webhook signature is invalid.");
		}

		// CryptographicOperations.FixedTimeEquals isn't available on netstandard2.0/net4x.
		private static bool FixedTimeEquals(string expected, string? actual)
		{
			if (actual == null || actual.Length != expected.Length)
				return false;
			var diff = 0;
			for (var i = 0; i < expected.Length; i++)
				diff |= expected[i] ^ actual[i];
			return diff == 0;
		}

		private static MailgunEmailWebhookEvent? ParsePayload(IRequestAdaptorContext adaptorContext)
		{
			if (!string.IsNullOrWhiteSpace(adaptorContext.Content))
			{
				var payload = MailgunEmailWebhookEvent.TryParseJson(adaptorContext.Content!);
				if (payload != null)
					return payload;
			}

			return MailgunEmailWebhookEvent.TryParseForm(adaptorContext);
		}

		private static string? GetContextValue(IRequestAdaptorContext adaptorContext, string key)
		{
			return adaptorContext.GetQueryValue(key)
				?? adaptorContext.GetFormValue(key)
				?? adaptorContext.GetHeaderValue(key);
		}

		private static CommunicationsStatus ConvertStatus(string? eventName, int? deliveryCode, string? detail)
		{
			if (string.IsNullOrWhiteSpace(eventName))
				return CommunicationsStatus.ClientError(MailgunConstant.Id, "Unknown");

			var normalized = eventName?.Trim() ?? "unknown";
			var lower = normalized.ToLowerInvariant();
			var code = NormalizeSubCode(deliveryCode);

			return lower switch
			{
				"accepted" or "queued" or "sending" or "stored" or "delayed" =>
					CommunicationsStatus.Info(MailgunConstant.Id, normalized, code, detail),

				"delivered" or "opened" or "clicked" =>
					CommunicationsStatus.Success(MailgunConstant.Id, normalized, code, detail),

				"unsubscribed" or "complained" or "spam" =>
					CommunicationsStatus.ClientError(MailgunConstant.Id, normalized, code, detail),

				"bounced" or "failed" or "rejected" or "dropped" =>
					CommunicationsStatus.ServerError(MailgunConstant.Id, normalized, code, detail),

				_ => CommunicationsStatus.ClientError(MailgunConstant.Id, "Unknown")
			};
		}

		private static int NormalizeSubCode(int? code)
		{
			if (!code.HasValue)
				return 0;

			if (code.Value < 0 || code.Value > 999)
				return 0;

			return code.Value;
		}

	}
}
