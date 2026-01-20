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
using Transmitly.Channel.Configuration.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Delivery;
using Transmitly.Util;

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	public sealed class ExtendedEmailDeliveryReportProperties : IEmailExtendedDeliveryReportProperties
	{
		private const string ProviderKey = MailgunConstant.EmailPropertiesKey;
		private readonly IExtendedProperties _extendedProperties;

		internal ExtendedEmailDeliveryReportProperties(DeliveryReport deliveryReport)
		{
			_extendedProperties = Guard.AgainstNull(deliveryReport).ExtendedProperties;
		}

		internal ExtendedEmailDeliveryReportProperties(IExtendedProperties properties)
		{
			_extendedProperties = Guard.AgainstNull(properties);
		}

		public IEmailExtendedChannelProperties Adapt(IEmailChannelConfiguration email)
		{
			return new EmailExtendedChannelProperties(email);
		}

		public string? Event
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(Event));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(Event), value);
		}

		public string? Id
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(Id));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(Id), value);
		}

		public DateTimeOffset? Timestamp
		{
			get => _extendedProperties.GetValue<DateTimeOffset?>(ProviderKey, nameof(Timestamp));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(Timestamp), value);
		}

		public string? LogLevel
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(LogLevel));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(LogLevel), value);
		}

		public string? Recipient
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(Recipient));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(Recipient), value);
		}

		public string? RecipientDomain
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(RecipientDomain));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(RecipientDomain), value);
		}

		public string? MessageId
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(MessageId));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(MessageId), value);
		}

		public string? MessageTo
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(MessageTo));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(MessageTo), value);
		}

		public string? MessageFrom
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(MessageFrom));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(MessageFrom), value);
		}

		public string? MessageSubject
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(MessageSubject));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(MessageSubject), value);
		}

		public int? MessageSize
		{
			get => _extendedProperties.GetValue<int?>(ProviderKey, nameof(MessageSize));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(MessageSize), value);
		}

		public int? DeliveryStatusCode
		{
			get => _extendedProperties.GetValue<int?>(ProviderKey, nameof(DeliveryStatusCode));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusCode), value);
		}

		public string? DeliveryStatusMessage
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(DeliveryStatusMessage));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusMessage), value);
		}

		public string? DeliveryStatusDescription
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(DeliveryStatusDescription));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusDescription), value);
		}

		public string? DeliveryStatusReason
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(DeliveryStatusReason));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusReason), value);
		}

		public string? DeliveryStatusMxHost
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(DeliveryStatusMxHost));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusMxHost), value);
		}

		public bool? DeliveryStatusTls
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(DeliveryStatusTls));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusTls), value);
		}

		public bool? DeliveryStatusUtf8
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(DeliveryStatusUtf8));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusUtf8), value);
		}

		public int? DeliveryStatusAttemptNo
		{
			get => _extendedProperties.GetValue<int?>(ProviderKey, nameof(DeliveryStatusAttemptNo));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusAttemptNo), value);
		}

		public double? DeliveryStatusSessionSeconds
		{
			get => _extendedProperties.GetValue<double?>(ProviderKey, nameof(DeliveryStatusSessionSeconds));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusSessionSeconds), value);
		}

		public bool? DeliveryStatusCertificateVerified
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(DeliveryStatusCertificateVerified));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(DeliveryStatusCertificateVerified), value);
		}

		public bool? FlagIsRouted
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(FlagIsRouted));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(FlagIsRouted), value);
		}

		public bool? FlagIsAuthenticated
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(FlagIsAuthenticated));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(FlagIsAuthenticated), value);
		}

		public bool? FlagIsSystemTest
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(FlagIsSystemTest));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(FlagIsSystemTest), value);
		}

		public bool? FlagIsTestMode
		{
			get => _extendedProperties.GetValue<bool?>(ProviderKey, nameof(FlagIsTestMode));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(FlagIsTestMode), value);
		}

		public string? EnvelopeTransport
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(EnvelopeTransport));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(EnvelopeTransport), value);
		}

		public string? EnvelopeSender
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(EnvelopeSender));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(EnvelopeSender), value);
		}

		public string? EnvelopeSendingIp
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(EnvelopeSendingIp));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(EnvelopeSendingIp), value);
		}

		public string? EnvelopeTargets
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(EnvelopeTargets));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(EnvelopeTargets), value);
		}

		public string? StorageUrl
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(StorageUrl));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(StorageUrl), value);
		}

		public string? StorageKey
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(StorageKey));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(StorageKey), value);
		}

		public IReadOnlyCollection<string>? Tags
		{
			get => _extendedProperties.GetValue<List<string>?>(ProviderKey, nameof(Tags));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(Tags), value);
		}

		public IReadOnlyDictionary<string, string?>? UserVariables
		{
			get => _extendedProperties.GetValue<Dictionary<string, string?>?>(ProviderKey, nameof(UserVariables));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(UserVariables), value);
		}

		public string? SignatureToken
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(SignatureToken));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(SignatureToken), value);
		}

		public string? SignatureTimestamp
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(SignatureTimestamp));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(SignatureTimestamp), value);
		}

		public string? SignatureValue
		{
			get => _extendedProperties.GetValue<string?>(ProviderKey, nameof(SignatureValue));
			set => _extendedProperties.AddOrUpdate(ProviderKey, nameof(SignatureValue), value);
		}
	}
}
