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
using Transmitly.ChannelProvider.Mailgun.Api.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;

namespace Transmitly
{
	public static class MailgunChannelProviderExtensions
	{
		/// <summary>
		/// Adds channel provider support for Mailgun.
		/// </summary>
		/// <param name="communicationsClientBuilder">Communications builder.</param>
		/// <param name="options">Mailgun channel provider options and settings.</param>
		/// <param name="providerId">Optional channel provider Id.</param>
		/// <returns>The provided builder object.</returns>
		public static CommunicationsClientBuilder AddMailgunSupport(this CommunicationsClientBuilder communicationsClientBuilder, Action<MailgunOptions> options, string? providerId = null)
		{
			var optionObj = new MailgunOptions();
			options(optionObj);

			communicationsClientBuilder.ChannelProvider.Build(Id.ChannelProvider.Mailgun(providerId), optionObj)
				.AddDispatcher<EmailChannelProviderDispatcher, IEmail>(Id.Channel.Email())
				.AddEmailExtendedPropertiesAdaptor<EmailExtendedChannelProperties>()
				.Register();

			return communicationsClientBuilder;
		}
	}
}
