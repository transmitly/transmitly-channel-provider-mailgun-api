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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Util;

namespace Transmitly.ChannelProvider.Mailgun.Api
{
	internal static class RestClientConfiguration
	{
		public static void Configure(HttpClient httpClient, MailgunOptions mailgunOptions)
		{
			Guard.AgainstNull(httpClient);
			Guard.AgainstNull(mailgunOptions);

			var host = Guard.AgainstNullOrWhiteSpace(mailgunOptions.ApiHost).TrimEnd('/');
			var version = Guard.AgainstNullOrWhiteSpace(mailgunOptions.ApiVersion).Trim('/');
			var domain = Guard.AgainstNullOrWhiteSpace(mailgunOptions.SendingDomain).Trim('/');

			httpClient.BaseAddress = new Uri($"{host}/{version}/{domain}/");
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(mailgunOptions.UserAgent);

			var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{mailgunOptions.ApiKey}"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
		}
	}
}
