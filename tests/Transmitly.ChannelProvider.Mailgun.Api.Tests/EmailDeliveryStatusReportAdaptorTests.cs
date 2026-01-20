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

using Transmitly.ChannelProvider.Mailgun.Api.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Delivery;

namespace Transmitly.ChannelProvider.Mailgun.Api.Tests
{
	[TestClass]
	public sealed class EmailDeliveryStatusReportAdaptorTests
	{
		[TestMethod]
		public async Task AdaptAsync_WithJsonPayload_ReturnsReport()
		{
			var context = new TestRequestAdaptorContext
			{
				Content = "{\"event-data\":{\"event\":\"delivered\",\"message\":{\"headers\":{\"message-id\":\"<msg-1>\"}},\"delivery-status\":{\"code\":250,\"message\":\"OK\"}}}",
				PipelineIntent = "delivery",
				PipelineId = "pipeline-1"
			};
			context.Query[DeliveryUtil.ChannelIdKey] = Id.Channel.Email();
			context.Query[DeliveryUtil.ChannelProviderIdKey] = MailgunConstant.Id;

			var adaptor = new EmailDeliveryStatusReportAdaptor();
			var reports = await adaptor.AdaptAsync(context);

			Assert.IsNotNull(reports);
			Assert.AreEqual(1, reports!.Count);

			var report = reports.Single();
			Assert.AreEqual(DeliveryReport.Event.StatusChanged(), report.EventName);
			Assert.AreEqual(Id.Channel.Email(), report.ChannelId);
			Assert.AreEqual(MailgunConstant.Id, report.ChannelProviderId);
			Assert.AreEqual("<msg-1>", report.ResourceId);
			Assert.AreEqual("delivery", report.PipelineIntent);
			Assert.AreEqual("pipeline-1", report.PipelineId);
			Assert.IsTrue(report.Status.Code is >= CommunicationsStatus.SuccessMin and <= CommunicationsStatus.SuccessMax);
		}

		[TestMethod]
		public async Task AdaptAsync_WithFormPayload_ReturnsReport()
		{
			var context = new TestRequestAdaptorContext
			{
				PipelineIntent = "delivery",
				PipelineId = "pipeline-2"
			};
			context.Query[DeliveryUtil.ChannelIdKey] = Id.Channel.Email();
			context.Query[DeliveryUtil.ChannelProviderIdKey] = MailgunConstant.Id;
			context.Form["event"] = "failed";
			context.Form["message-id"] = "<msg-2>";
			context.Form["code"] = "550";
			context.Form["message"] = "bounce";

			var adaptor = new EmailDeliveryStatusReportAdaptor();
			var reports = await adaptor.AdaptAsync(context);

			Assert.IsNotNull(reports);
			var report = reports!.Single();
			Assert.AreEqual("<msg-2>", report.ResourceId);
			Assert.IsTrue(report.Status.Code is >= CommunicationsStatus.ServerErrMin and <= CommunicationsStatus.ServerErrMax);
		}

		[TestMethod]
		public async Task AdaptAsync_WhenProviderMismatch_ReturnsNull()
		{
			var context = new TestRequestAdaptorContext
			{
				Content = "{\"event-data\":{\"event\":\"delivered\"}}"
			};
			context.Query[DeliveryUtil.ChannelIdKey] = Id.Channel.Email();
			context.Query[DeliveryUtil.ChannelProviderIdKey] = "OtherProvider";

			var adaptor = new EmailDeliveryStatusReportAdaptor();
			var reports = await adaptor.AdaptAsync(context);

			Assert.IsNull(reports);
		}

		private sealed class TestRequestAdaptorContext : IRequestAdaptorContext
		{
			public Dictionary<string, string> Query { get; } = new(StringComparer.OrdinalIgnoreCase);
			public Dictionary<string, string> Form { get; } = new(StringComparer.OrdinalIgnoreCase);
			public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

			public string? Content { get; set; }
			public string? PipelineIntent { get; set; }
			public string? PipelineId { get; set; }
			public string? ResourceId { get; set; }

			public string? GetQueryValue(string key)
			{
				return Query.TryGetValue(key, out var value) ? value : null;
			}

			public string? GetFormValue(string key)
			{
				return Form.TryGetValue(key, out var value) ? value : null;
			}

			public string? GetHeaderValue(string key)
			{
				return Headers.TryGetValue(key, out var value) ? value : null;
			}

			public string? GetValue(string key)
			{
				return GetQueryValue(key) ?? GetFormValue(key) ?? GetHeaderValue(key);
			}
		}
	}
}
