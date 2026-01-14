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
using System.Text.Json;
using Transmitly;
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

			var report = new DeliveryReport(
					DeliveryReport.Event.StatusChanged(),
					Id.Channel.Email(),
					MailgunConstant.Id,
					adaptorContext.PipelineIntent,
					adaptorContext.PipelineId,
					payload.MessageId ?? adaptorContext.ResourceId,
					ConvertStatus(payload.EventName, payload.DeliveryCode, payload.DeliveryMessage),
					null,
					null,
					null
				);

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

		private static MailgunEventPayload? ParsePayload(IRequestAdaptorContext adaptorContext)
		{
			if (!string.IsNullOrWhiteSpace(adaptorContext.Content))
			{
				var payload = TryParseJson(adaptorContext.Content!);
				if (payload != null)
					return payload;
			}

			return TryParseForm(adaptorContext);
		}

		private static MailgunEventPayload? TryParseJson(string content)
		{
			try
			{
				using var doc = JsonDocument.Parse(content);
				var root = doc.RootElement;
				var eventData = GetEventData(root);

				var eventName = GetString(eventData, "event") ?? GetString(root, "event");
				if (string.IsNullOrWhiteSpace(eventName))
					return null;

				var messageId = GetMessageId(eventData)
					?? GetString(eventData, "message-id")
					?? GetString(eventData, "message_id")
					?? GetMessageId(root);

				var deliveryCode = GetInt(eventData, "delivery-status", "code");
				var deliveryMessage = GetString(eventData, "delivery-status", "message")
					?? GetString(eventData, "reason")
					?? GetString(eventData, "description");

				return new MailgunEventPayload(eventName!, messageId, deliveryCode, deliveryMessage);
			}
			catch (JsonException)
			{
				return null;
			}
		}

		private static JsonElement GetEventData(JsonElement root)
		{
			if (root.ValueKind == JsonValueKind.Object)
			{
				if (root.TryGetProperty("event-data", out var eventData))
					return eventData;
				if (root.TryGetProperty("event_data", out eventData))
					return eventData;
			}

			return root;
		}

		private static MailgunEventPayload? TryParseForm(IRequestAdaptorContext adaptorContext)
		{
			var eventName = GetContextValue(adaptorContext, "event");
			if (string.IsNullOrWhiteSpace(eventName))
				return null;

			var messageId = GetContextValue(adaptorContext, "message-id")
				?? GetContextValue(adaptorContext, "Message-Id")
				?? GetContextValue(adaptorContext, "message_id");

			var deliveryCode = ParseInt(GetContextValue(adaptorContext, "code"));
			var deliveryMessage = GetContextValue(adaptorContext, "message")
				?? GetContextValue(adaptorContext, "description")
				?? GetContextValue(adaptorContext, "reason");

			return new MailgunEventPayload(eventName!, messageId, deliveryCode, deliveryMessage);
		}

		private static string? GetMessageId(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (element.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object &&
				message.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
			{
				var messageId = GetString(headers, "message-id") ?? GetString(headers, "Message-Id");
				if (!string.IsNullOrWhiteSpace(messageId))
					return messageId;
			}

			return null;
		}

		private static string? GetString(JsonElement element, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind == JsonValueKind.String)
				return property.GetString();

			return null;
		}

		private static string? GetString(JsonElement element, string objectName, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(objectName, out var child))
				return null;

			return GetString(child, propertyName);
		}

		private static int? GetInt(JsonElement element, string objectName, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(objectName, out var child))
				return null;

			if (child.ValueKind != JsonValueKind.Object || !child.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
				return number;

			if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var parsed))
				return parsed;

			return null;
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

		private static int? ParseInt(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			return int.TryParse(value, out var parsed) ? parsed : null;
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

		private sealed record MailgunEventPayload(string EventName, string? MessageId, int? DeliveryCode, string? DeliveryMessage);
	}
}
