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
using System.Threading.Tasks;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Delivery;

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	public sealed class EmailDeliveryStatusReportAdaptor : IChannelProviderDeliveryReportRequestAdaptor
	{
		public Task<IReadOnlyCollection<DeliveryReport>?> AdaptAsync(IRequestAdaptorContext adaptorContext)
		{
			if (!ShouldAdapt(adaptorContext))
				return Task.FromResult<IReadOnlyCollection<DeliveryReport>?>(null);

			var payload = ParsePayload(adaptorContext);
			if (payload == null)
				return Task.FromResult<IReadOnlyCollection<DeliveryReport>?>(null);

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
				?? adaptorContext.GetHeaderValue(key)
#pragma warning disable CS0618
				?? adaptorContext.GetValue(key);
#pragma warning restore CS0618
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
