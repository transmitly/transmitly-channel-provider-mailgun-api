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
			var email = deliveryReport.Mailgun().Email;
			email.Event = report.Event;
			email.Id = report.Id;
			email.Timestamp = report.Timestamp;
			email.LogLevel = report.LogLevel;
			email.Recipient = report.Recipient;
			email.RecipientDomain = report.RecipientDomain;
			email.MessageId = report.MessageId;
			email.MessageTo = report.MessageTo;
			email.MessageFrom = report.MessageFrom;
			email.MessageSubject = report.MessageSubject;
			email.MessageSize = report.MessageSize;
			email.DeliveryStatusCode = report.DeliveryCode;
			email.DeliveryStatusMessage = report.DeliveryMessage;
			email.DeliveryStatusDescription = report.DeliveryDescription;
			email.DeliveryStatusReason = report.DeliveryReason;
			email.DeliveryStatusMxHost = report.DeliveryMxHost;
			email.DeliveryStatusTls = report.DeliveryTls;
			email.DeliveryStatusUtf8 = report.DeliveryUtf8;
			email.DeliveryStatusAttemptNo = report.DeliveryAttemptNo;
			email.DeliveryStatusSessionSeconds = report.DeliverySessionSeconds;
			email.DeliveryStatusCertificateVerified = report.DeliveryCertificateVerified;
			email.FlagIsRouted = report.FlagIsRouted;
			email.FlagIsAuthenticated = report.FlagIsAuthenticated;
			email.FlagIsSystemTest = report.FlagIsSystemTest;
			email.FlagIsTestMode = report.FlagIsTestMode;
			email.EnvelopeTransport = report.EnvelopeTransport;
			email.EnvelopeSender = report.EnvelopeSender;
			email.EnvelopeSendingIp = report.EnvelopeSendingIp;
			email.EnvelopeTargets = report.EnvelopeTargets;
			email.StorageUrl = report.StorageUrl;
			email.StorageKey = report.StorageKey;
			email.Tags = report.Tags;
			email.UserVariables = report.UserVariables;
			email.SignatureToken = report.SignatureToken;
			email.SignatureTimestamp = report.SignatureTimestamp;
			email.SignatureValue = report.SignatureValue;

			return deliveryReport;
		}
	}
}
