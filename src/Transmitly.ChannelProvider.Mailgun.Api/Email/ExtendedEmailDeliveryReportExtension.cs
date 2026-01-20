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

using Transmitly.Delivery;

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	internal static class ExtendedEmailDeliveryReportExtension
	{
		public static DeliveryReport ApplyExtendedProperties(this DeliveryReport deliveryReport, MailgunEmailWebhookEvent report)
		{
			_ = new ExtendedEmailDeliveryReportProperties(deliveryReport)
			{
				Event = report.Event,
				Id = report.Id,
				Timestamp = report.Timestamp,
				LogLevel = report.LogLevel,
				Recipient = report.Recipient,
				RecipientDomain = report.RecipientDomain,
				MessageId = report.MessageId,
				MessageTo = report.MessageTo,
				MessageFrom = report.MessageFrom,
				MessageSubject = report.MessageSubject,
				MessageSize = report.MessageSize,
				DeliveryStatusCode = report.DeliveryCode,
				DeliveryStatusMessage = report.DeliveryMessage,
				DeliveryStatusDescription = report.DeliveryDescription,
				DeliveryStatusReason = report.DeliveryReason,
				DeliveryStatusMxHost = report.DeliveryMxHost,
				DeliveryStatusTls = report.DeliveryTls,
				DeliveryStatusUtf8 = report.DeliveryUtf8,
				DeliveryStatusAttemptNo = report.DeliveryAttemptNo,
				DeliveryStatusSessionSeconds = report.DeliverySessionSeconds,
				DeliveryStatusCertificateVerified = report.DeliveryCertificateVerified,
				FlagIsRouted = report.FlagIsRouted,
				FlagIsAuthenticated = report.FlagIsAuthenticated,
				FlagIsSystemTest = report.FlagIsSystemTest,
				FlagIsTestMode = report.FlagIsTestMode,
				EnvelopeTransport = report.EnvelopeTransport,
				EnvelopeSender = report.EnvelopeSender,
				EnvelopeSendingIp = report.EnvelopeSendingIp,
				EnvelopeTargets = report.EnvelopeTargets,
				StorageUrl = report.StorageUrl,
				StorageKey = report.StorageKey,
				Tags = report.Tags,
				UserVariables = report.UserVariables,
				SignatureToken = report.SignatureToken,
				SignatureTimestamp = report.SignatureTimestamp,
				SignatureValue = report.SignatureValue
			};

			return deliveryReport;
		}
	}
}
