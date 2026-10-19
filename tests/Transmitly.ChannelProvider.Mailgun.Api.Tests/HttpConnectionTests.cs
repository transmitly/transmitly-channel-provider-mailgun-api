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

using System.Net;
using Transmitly.ChannelProvider.Mailgun.Api.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;

namespace Transmitly.ChannelProvider.Mailgun.Api.Tests
{
	[TestClass]
	public sealed class HttpConnectionTests
	{
		private const int DispatchCount = 5;
		// .invalid never resolves, so a request that skips the proxy cannot leave the machine.
		private const string ProxiedTarget = "http://api.mailgun.invalid/";

		[TestMethod]
		public async Task ConfiguredWebProxyShouldBeUsed()
		{
			using var proxy = new LoopbackHttpServer();
			var options = CreateOptions(new WebProxy(proxy.Url));

			await LoopbackHttpServer.SendWithDispatcherClientAsync(new EmailChannelProviderDispatcher(options), ProxiedTarget);

			Assert.AreEqual(1, proxy.RequestCount, "The request did not go through the configured WebProxy.");
		}

		[TestMethod]
		public async Task ProxiedDispatchersShouldShareOneConnection()
		{
			using var proxy = new LoopbackHttpServer();
			var options = CreateOptions(new WebProxy(proxy.Url));

			for (var i = 0; i < DispatchCount; i++)
				await LoopbackHttpServer.SendWithDispatcherClientAsync(new EmailChannelProviderDispatcher(options), ProxiedTarget);

			Assert.AreEqual(DispatchCount, proxy.RequestCount, "Precondition: every dispatch went through the configured WebProxy.");
			Assert.AreEqual(1, proxy.ConnectionCount, "Each dispatcher opened its own connection to the proxy.");
		}

		[TestMethod]
		public async Task DispatchersWithoutProxyShouldShareOneConnection()
		{
			using var server = new LoopbackHttpServer();
			var options = CreateOptions(webProxy: null);

			for (var i = 0; i < DispatchCount; i++)
				await LoopbackHttpServer.SendWithDispatcherClientAsync(new EmailChannelProviderDispatcher(options), server.Url);

			Assert.AreEqual(DispatchCount, server.RequestCount, "Precondition: every dispatch reached the server.");
			Assert.AreEqual(1, server.ConnectionCount, "Each dispatcher opened its own connection; requires the Transmitly core fix.");
		}

		private static MailgunOptions CreateOptions(WebProxy? webProxy) => new()
		{
			ApiKey = "key-test",
			SendingDomain = "mg.example.com",
			WebProxy = webProxy
		};
	}
}
