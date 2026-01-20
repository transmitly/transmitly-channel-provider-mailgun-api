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
using System.Globalization;
using System.Text.Json;
using Transmitly.Delivery;

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	internal sealed class MailgunEmailWebhookEvent
	{
		public string? Event { get; init; }
		public string? MessageId { get; init; }
		public int? DeliveryCode { get; init; }
		public string? DeliveryMessage { get; init; }
		public string? DeliveryDescription { get; init; }
		public string? DeliveryReason { get; init; }
		public string? Id { get; init; }
		public DateTimeOffset? Timestamp { get; init; }
		public string? LogLevel { get; init; }
		public bool? DeliveryTls { get; init; }
		public bool? DeliveryUtf8 { get; init; }
		public bool? DeliveryCertificateVerified { get; init; }
		public int? DeliveryAttemptNo { get; init; }
		public double? DeliverySessionSeconds { get; init; }
		public string? DeliveryMxHost { get; init; }
		public bool? FlagIsRouted { get; init; }
		public bool? FlagIsAuthenticated { get; init; }
		public bool? FlagIsSystemTest { get; init; }
		public bool? FlagIsTestMode { get; init; }
		public string? EnvelopeTransport { get; init; }
		public string? EnvelopeSender { get; init; }
		public string? EnvelopeSendingIp { get; init; }
		public string? EnvelopeTargets { get; init; }
		public string? StorageUrl { get; init; }
		public string? StorageKey { get; init; }
		public IReadOnlyCollection<string>? Tags { get; init; }
		public IReadOnlyDictionary<string, string?>? UserVariables { get; init; }
		public string? Recipient { get; init; }
		public string? RecipientDomain { get; init; }
		public string? MessageTo { get; init; }
		public string? MessageFrom { get; init; }
		public string? MessageSubject { get; init; }
		public int? MessageSize { get; init; }
		public string? SignatureToken { get; init; }
		public string? SignatureTimestamp { get; init; }
		public string? SignatureValue { get; init; }

		public static MailgunEmailWebhookEvent? TryParseJson(string content)
		{
			try
			{
				using var doc = JsonDocument.Parse(content);
				var root = doc.RootElement;
				var eventData = GetEventData(root);

				var eventName = GetString(eventData, "event") ?? GetString(root, "event");
				if (string.IsNullOrWhiteSpace(eventName))
					return null;

				return new MailgunEmailWebhookEvent
				{
					Event = eventName,
					MessageId = GetMessageId(eventData)
						?? GetString(eventData, "message-id")
						?? GetString(eventData, "message_id")
						?? GetMessageId(root),
					DeliveryCode = GetInt(eventData, "delivery-status", "code"),
					DeliveryMessage = GetString(eventData, "delivery-status", "message"),
					DeliveryDescription = GetString(eventData, "delivery-status", "description"),
					DeliveryReason = GetString(eventData, "reason"),
					DeliveryMxHost = GetString(eventData, "delivery-status", "mx-host"),
					DeliveryTls = GetBool(eventData, "delivery-status", "tls"),
					DeliveryUtf8 = GetBool(eventData, "delivery-status", "utf8"),
					DeliveryCertificateVerified = GetBool(eventData, "delivery-status", "certificate-verified"),
					DeliveryAttemptNo = GetInt(eventData, "delivery-status", "attempt-no"),
					DeliverySessionSeconds = GetDouble(eventData, "delivery-status", "session-seconds"),
					Id = GetString(eventData, "id"),
					LogLevel = GetString(eventData, "log-level") ?? GetString(eventData, "log_level"),
					Timestamp = GetUnixTimestamp(eventData, "timestamp") ?? GetUnixTimestamp(root, "timestamp"),
					Recipient = GetString(eventData, "recipient"),
					RecipientDomain = GetString(eventData, "recipient-domain") ?? GetString(eventData, "recipient_domain"),
					MessageTo = GetString(eventData, "message", "headers", "to"),
					MessageFrom = GetString(eventData, "message", "headers", "from"),
					MessageSubject = GetString(eventData, "message", "headers", "subject"),
					MessageSize = GetInt(eventData, "message", "size"),
					EnvelopeTransport = GetString(eventData, "envelope", "transport"),
					EnvelopeSender = GetString(eventData, "envelope", "sender"),
					EnvelopeSendingIp = GetString(eventData, "envelope", "sending-ip"),
					EnvelopeTargets = GetString(eventData, "envelope", "targets"),
					StorageUrl = GetString(eventData, "storage", "url"),
					StorageKey = GetString(eventData, "storage", "key"),
					Tags = GetStringArray(eventData, "tags"),
					UserVariables = GetStringDictionary(eventData, "user-variables") ?? GetStringDictionary(eventData, "user_variables"),
					FlagIsRouted = GetBool(eventData, "flags", "is-routed"),
					FlagIsAuthenticated = GetBool(eventData, "flags", "is-authenticated"),
					FlagIsSystemTest = GetBool(eventData, "flags", "is-system-test"),
					FlagIsTestMode = GetBool(eventData, "flags", "is-test-mode"),
					SignatureToken = GetString(root, "signature", "token"),
					SignatureTimestamp = GetString(root, "signature", "timestamp"),
					SignatureValue = GetString(root, "signature", "signature")
				};
			}
			catch (JsonException)
			{
				return null;
			}
		}

		public static MailgunEmailWebhookEvent? TryParseForm(IRequestAdaptorContext adaptorContext)
		{
			var eventName = GetContextValue(adaptorContext, "event");
			if (string.IsNullOrWhiteSpace(eventName))
				return null;

			return new MailgunEmailWebhookEvent
			{
				Event = eventName,
				MessageId = GetContextValue(adaptorContext, "message-id")
					?? GetContextValue(adaptorContext, "Message-Id")
					?? GetContextValue(adaptorContext, "message_id"),
				DeliveryCode = ParseInt(GetContextValue(adaptorContext, "code")),
				DeliveryMessage = GetContextValue(adaptorContext, "message"),
				DeliveryDescription = GetContextValue(adaptorContext, "description"),
				DeliveryReason = GetContextValue(adaptorContext, "reason"),
				Recipient = GetContextValue(adaptorContext, "recipient"),
				RecipientDomain = GetContextValue(adaptorContext, "recipient-domain")
					?? GetContextValue(adaptorContext, "recipient_domain"),
				SignatureToken = GetContextValue(adaptorContext, "token"),
				SignatureTimestamp = GetContextValue(adaptorContext, "timestamp"),
				SignatureValue = GetContextValue(adaptorContext, "signature")
			};
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

		private static string? GetString(JsonElement element, string objectName, string childName, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(objectName, out var child))
				return null;

			if (!child.TryGetProperty(childName, out var grandChild))
				return null;

			return GetString(grandChild, propertyName);
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

			if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
				return parsed;

			return null;
		}

		private static int? GetInt(JsonElement element, string objectName, string childName, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(objectName, out var child))
				return null;

			if (!child.TryGetProperty(childName, out var grandChild))
				return null;

			if (grandChild.ValueKind == JsonValueKind.Object && grandChild.TryGetProperty(propertyName, out var property))
			{
				if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
					return number;

				if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
					return parsed;
			}

			return null;
		}

		private static double? GetDouble(JsonElement element, string objectName, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(objectName, out var child))
				return null;

			if (child.ValueKind != JsonValueKind.Object || !child.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var number))
				return number;

			if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
				return parsed;

			return null;
		}

		private static bool? GetBool(JsonElement element, string objectName, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(objectName, out var child))
				return null;

			if (child.ValueKind != JsonValueKind.Object || !child.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind == JsonValueKind.True)
				return true;

			if (property.ValueKind == JsonValueKind.False)
				return false;

			if (property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out var parsed))
				return parsed;

			return null;
		}

		private static DateTimeOffset? GetUnixTimestamp(JsonElement element, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var number))
				return FromUnixTimeSeconds(number);

			if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
				return FromUnixTimeSeconds(parsed);

			return null;
		}

		private static DateTimeOffset? FromUnixTimeSeconds(double seconds)
		{
			if (seconds <= 0)
				return null;

			var wholeSeconds = (long)Math.Floor(seconds);
			var fraction = seconds - wholeSeconds;
			var offset = DateTimeOffset.FromUnixTimeSeconds(wholeSeconds);
			if (fraction <= 0)
				return offset;

			return offset.AddSeconds(fraction);
		}

		private static IReadOnlyCollection<string>? GetStringArray(JsonElement element, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind != JsonValueKind.Array)
				return null;

			var list = new List<string>();
			foreach (var item in property.EnumerateArray())
			{
				if (item.ValueKind == JsonValueKind.String)
					list.Add(item.GetString() ?? string.Empty);
				else
					list.Add(item.ToString());
			}

			return list.Count == 0 ? null : list;
		}

		private static IReadOnlyDictionary<string, string?>? GetStringDictionary(JsonElement element, string propertyName)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			if (!element.TryGetProperty(propertyName, out var property))
				return null;

			if (property.ValueKind != JsonValueKind.Object)
				return null;

			var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
			foreach (var item in property.EnumerateObject())
			{
				if (item.Value.ValueKind == JsonValueKind.String)
					dict[item.Name] = item.Value.GetString();
				else
					dict[item.Name] = item.Value.ToString();
			}

			return dict.Count == 0 ? null : dict;
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

			return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
		}
	}
}
