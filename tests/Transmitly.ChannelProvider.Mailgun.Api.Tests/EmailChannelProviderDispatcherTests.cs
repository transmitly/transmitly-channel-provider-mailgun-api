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

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider.Mailgun.Api.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly.ChannelProvider.Mailgun.Api.Tests
{
	[TestClass]
	public sealed class EmailChannelProviderDispatcherTests
	{
		[TestMethod]
		public async Task DispatchAsync_WithTemplate_SendsExpectedPayload()
		{
			var handler = new TestHttpMessageHandler
			{
				StatusCode = HttpStatusCode.OK,
				ResponseBody = "{\"id\":\"<test>\",\"message\":\"Queued\"}"
			};
			var client = new HttpClient(handler);
			var options = new MailgunOptions
			{
				ApiKey = "key-test",
				SendingDomain = "mg.example.com",
				ApiHost = "https://api.mailgun.test",
				ApiVersion = "v3"
			};
			var dispatcher = new EmailChannelProviderDispatcher(options, client);

			var extendedProperties = new ExtendedProperties();
			_ = new EmailExtendedChannelProperties(extendedProperties)
			{
				Template = "welcome",
				TemplateVersion = "v1"
			};

			var email = new TestEmail
			{
				From = new PlatformIdentityAddress("from@example.com"),
				To = [new PlatformIdentityAddress("to@example.com")],
				ExtendedProperties = extendedProperties
			};

			var context = new TestDispatchCommunicationContext(new TestContentModel(new { firstName = "Sally" }));

			await dispatcher.DispatchAsync(email, context, CancellationToken.None);

			Assert.IsNotNull(handler.Request);
			Assert.AreEqual(HttpMethod.Post, handler.Request!.Method);
			Assert.AreEqual("https://api.mailgun.test/v3/mg.example.com/messages", handler.Request.RequestUri!.ToString());

			var auth = handler.Request.Headers.Authorization;
			Assert.IsNotNull(auth);
			Assert.AreEqual("Basic", auth!.Scheme);
			Assert.AreEqual(Convert.ToBase64String(Encoding.ASCII.GetBytes("api:key-test")), auth.Parameter);

			var parts = MultipartFormDataParser.Parse(handler.CapturedContentType, handler.CapturedContent);
			AssertFieldValue(parts, "template", "welcome");
			AssertFieldValue(parts, "t:version", "v1");
			AssertFieldValue(parts, "from", "from@example.com");
			AssertFieldValue(parts, "to", "to@example.com");

			var variables = GetFieldValue(parts, "h:X-Mailgun-Variables");
			Assert.IsNotNull(variables);
			using var doc = JsonDocument.Parse(variables!);
			Assert.AreEqual("Sally", doc.RootElement.GetProperty("firstName").GetString());
		}

		[TestMethod]
		public async Task DispatchAsync_WithTrackingTagsAndAttachments_SendsExpectedFields()
		{
			var handler = new TestHttpMessageHandler
			{
				StatusCode = HttpStatusCode.OK,
				ResponseBody = "{\"id\":\"<test>\",\"message\":\"Queued\"}"
			};
			var client = new HttpClient(handler);
			var options = new MailgunOptions
			{
				ApiKey = "key-test",
				SendingDomain = "mg.example.com",
				ApiHost = "https://api.mailgun.test"
			};
			var dispatcher = new EmailChannelProviderDispatcher(options, client);

			var extendedProperties = new ExtendedProperties();
			_ = new EmailExtendedChannelProperties(extendedProperties)
			{
				Tags = ["alpha", "beta"],
				Tracking = true,
				TrackingClicks = false,
				TrackingOpens = true,
				TrackingPixelLocationTop = true,
				RequireTls = true,
				TestMode = true,
				Properties = new Dictionary<string, string> { ["v:customer-id"] = "123" }
			};

			var email = new TestEmail
			{
				From = new PlatformIdentityAddress("from@example.com"),
				To = [new PlatformIdentityAddress("to@example.com")],
				ReplyTo = [new PlatformIdentityAddress("reply@example.com")],
				Subject = "Hello",
				TextBody = "Hello text",
				HtmlBody = "<p>Hello</p>",
				Attachments =
				[
					new TestEmailAttachment(
						"report.txt",
						"text/plain",
						new MemoryStream(Encoding.UTF8.GetBytes("hello")))
				],
				ExtendedProperties = extendedProperties
			};

			var context = new TestDispatchCommunicationContext();
			await dispatcher.DispatchAsync(email, context, CancellationToken.None);

			var parts = MultipartFormDataParser.Parse(handler.CapturedContentType, handler.CapturedContent);
			AssertFieldValue(parts, "subject", "Hello");
			AssertFieldValue(parts, "text", "Hello text");
			AssertFieldValue(parts, "html", "<p>Hello</p>");
			AssertFieldValue(parts, "h:Reply-To", "reply@example.com");
			AssertFieldValue(parts, "o:tracking", "yes");
			AssertFieldValue(parts, "o:tracking-clicks", "no");
			AssertFieldValue(parts, "o:tracking-opens", "yes");
			AssertFieldValue(parts, "o:tracking-pixel-location", "top");
			AssertFieldValue(parts, "o:require-tls", "yes");
			AssertFieldValue(parts, "o:testmode", "yes");
			AssertFieldValue(parts, "v:customer-id", "123");

			var tags = GetFieldValues(parts, "o:tag");
			CollectionAssert.AreEquivalent(new[] { "alpha", "beta" }, tags);

			var attachment = parts.SingleOrDefault(part => part.Name == "attachment");
			Assert.IsNotNull(attachment);
			Assert.AreEqual("report.txt", attachment!.FileName);
			Assert.AreEqual("hello", attachment.Body);
		}

		[TestMethod]
		public async Task DispatchAsync_WhenNoRecipients_ThrowsMailgunException()
		{
			var dispatcher = CreateDispatcher();
			var email = new TestEmail
			{
				From = new PlatformIdentityAddress("from@example.com"),
				ExtendedProperties = new ExtendedProperties()
			};

			await Assert.ThrowsExactlyAsync<MailgunException>(() =>
				dispatcher.DispatchAsync(email, new TestDispatchCommunicationContext(), CancellationToken.None));
		}

		[TestMethod]
		public async Task DispatchAsync_WhenMissingContentAndTemplate_ThrowsMailgunException()
		{
			var dispatcher = CreateDispatcher();
			var email = new TestEmail
			{
				From = new PlatformIdentityAddress("from@example.com"),
				To = [new PlatformIdentityAddress("to@example.com")],
				ExtendedProperties = new ExtendedProperties()
			};

			await Assert.ThrowsExactlyAsync<MailgunException>(() =>
				dispatcher.DispatchAsync(email, new TestDispatchCommunicationContext(), CancellationToken.None));
		}

		private static EmailChannelProviderDispatcher CreateDispatcher(TestHttpMessageHandler? handler = null)
		{
			handler ??= new TestHttpMessageHandler
			{
				StatusCode = HttpStatusCode.OK,
				ResponseBody = "{\"id\":\"<test>\",\"message\":\"Queued\"}"
			};

			var options = new MailgunOptions
			{
				ApiKey = "key-test",
				SendingDomain = "mg.example.com",
				ApiHost = "https://api.mailgun.test"
			};

			return new EmailChannelProviderDispatcher(options, new HttpClient(handler));
		}

		private static void AssertFieldValue(IReadOnlyList<MultipartPart> parts, string name, string expected)
		{
			var value = GetFieldValue(parts, name);
			Assert.IsNotNull(value);
			Assert.AreEqual(expected, value);
		}

		private static string? GetFieldValue(IReadOnlyList<MultipartPart> parts, string name)
		{
			return parts.FirstOrDefault(part => part.Name == name)?.Body;
		}

		private static string[] GetFieldValues(IReadOnlyList<MultipartPart> parts, string name)
		{
			return [.. parts.Where(part => part.Name == name).Select(part => part.Body)];
		}

		private sealed class TestEmail : IEmail
		{
			public string? Subject { get; set; }
			public string? HtmlBody { get; set; }
			public string? TextBody { get; set; }
			public MessagePriority Priority { get; set; } = MessagePriority.Normal;
			public TransportPriority TransportPriority { get; set; } = TransportPriority.Normal;
			public IPlatformIdentityAddress From { get; set; } = new PlatformIdentityAddress("default@example.com");
			public IPlatformIdentityAddress[]? ReplyTo { get; set; }
			public IPlatformIdentityAddress[]? To { get; set; }
			public IPlatformIdentityAddress[]? Cc { get; set; }
			public IPlatformIdentityAddress[]? Bcc { get; set; }
			public IReadOnlyCollection<IEmailAttachment> Attachments { get; set; } = Array.Empty<IEmailAttachment>();
			public IExtendedProperties ExtendedProperties { get; set; } = new ExtendedProperties();
			public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }
		}

		private sealed class TestEmailAttachment(string name, string contentType, Stream contentStream) : IEmailAttachment
		{
			public string? Name { get; } = name;
			public string? ContentType { get; } = contentType;
			public Stream? ContentStream { get; } = contentStream;
		}

		private sealed class TestDispatchCommunicationContext(IContentModel? contentModel = null) : IDispatchCommunicationContext
		{
			public IContentModel? ContentModel { get; } = contentModel;
			public CultureInfo CultureInfo { get; } = CultureInfo.InvariantCulture;
			public IReadOnlyCollection<IPlatformIdentityProfile> PlatformIdentities { get; } = Array.Empty<IPlatformIdentityProfile>();
			public TransportPriority TransportPriority { get; } = TransportPriority.Normal;
			public MessagePriority MessagePriority { get; } = MessagePriority.Normal;
			public IPipelineConfiguration ChannelConfiguration { get; } = new TestPipelineConfiguration();
			public ICollection<IDispatchResult> DispatchResults { get; } = [];
			public IDeliveryReportService DeliveryReportManager { get; } = new TestDeliveryReportService();
			public string? ChannelId { get; } = "Email";
			public string? ChannelProviderId { get; } = "Mailgun";
			public ITemplateEngine TemplateEngine { get; } = new TestTemplateEngine();
			public string PipelineIntent { get; } = "test";
			public string? PipelineId { get; } = "pipeline";
		}

		private sealed class TestDeliveryReportService : IDeliveryReportService
		{
			public List<DeliveryReport> Reports { get; } = [];

			public Task DispatchAsync(DeliveryReport deliveryReport)
			{
				Reports.Add(deliveryReport);
				return Task.CompletedTask;
			}

			public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> deliveryReports)
			{
				Reports.AddRange(deliveryReports);
				return Task.CompletedTask;
			}

			public IDisposable Subscribe(IObserver<DeliveryReport> observer)
			{
				return new NoopDisposable();
			}

			private sealed class NoopDisposable : IDisposable
			{
				public void Dispose()
				{
				}
			}
		}

		private sealed class TestPipelineConfiguration : IPipelineConfiguration
		{
			public string? PipelineId { get; } = "pipeline";
			public IReadOnlyCollection<IChannel> Channels { get; } = Array.Empty<IChannel>();
			public IReadOnlyCollection<string> PersonaFilters { get; } = Array.Empty<string>();
			public BasePipelineDeliveryStrategyProvider PipelineDeliveryStrategyProvider { get; } = new TestPipelineDeliveryStrategyProvider();
			public bool IsDispatchRequirementsAllowed { get; } = true;
			public bool IsDispatchChannelPriorityPreferenceAllowed { get; } = true;

			public IPipelineConfiguration AddChannel(IChannel channel) => this;
			public IPipelineConfiguration UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider) => this;
			public IPipelineConfiguration AddPersonaFilter(string personaName) => this;
			public IPipelineConfiguration Id(string id) => this;
			public IPipelineConfiguration AllowDispatchRequirements(bool allowed) => this;
			public IPipelineConfiguration AllowDispatchChannelPriorityPreference(bool allowed) => this;
		}

		private sealed class TestPipelineDeliveryStrategyProvider : BasePipelineDeliveryStrategyProvider
		{
			public override Task<IDispatchCommunicationResult> DispatchAsync(IReadOnlyCollection<RecipientDispatchCommunicationContext> sendingGroups, CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class TestTemplateEngine : ITemplateEngine
		{
			public Task<string?> RenderAsync(IContentTemplateRegistration? registration, IDispatchCommunicationContext context)
			{
				return Task.FromResult<string?>("amp-content");
			}
		}

		private sealed class TestContentModel(object model) : IContentModel
		{
			public object Model { get; } = model;
			public IReadOnlyList<Resource> Resources { get; } = Array.Empty<Resource>();
			public IReadOnlyList<LinkedResource> LinkedResources { get; } = Array.Empty<LinkedResource>();
		}

		private sealed class TestHttpMessageHandler : HttpMessageHandler
		{
			public HttpRequestMessage? Request { get; private set; }
			public string? CapturedContent { get; private set; }
			public string? CapturedContentType { get; private set; }
			public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
			public string ResponseBody { get; set; } = "{}";

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Request = request;
				if (request.Content != null)
				{
					CapturedContentType = request.Content.Headers.ContentType?.ToString();
#if NET5_0_OR_GREATER
					CapturedContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
					CapturedContent = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
				}
				return new HttpResponseMessage(StatusCode)
				{
					Content = new StringContent(ResponseBody)
				};
			}
		}

		private sealed class MultipartPart(string name, string body, string? fileName, IReadOnlyDictionary<string, string> headers)
		{
			public string Name { get; } = name;
			public string Body { get; } = body;
			public string? FileName { get; } = fileName;
			public IReadOnlyDictionary<string, string> Headers { get; } = headers;
		}

		private static class MultipartFormDataParser
		{
			public static IReadOnlyList<MultipartPart> Parse(string? contentType, string? payload)
			{
				if (string.IsNullOrWhiteSpace(contentType))
					throw new InvalidOperationException("Missing content type header.");
				if (string.IsNullOrWhiteSpace(payload))
					throw new InvalidOperationException("Missing multipart payload.");

				var boundary = GetBoundary(contentType);
				if (string.IsNullOrWhiteSpace(boundary))
					throw new InvalidOperationException("Missing multipart boundary.");

				boundary = boundary!.Trim('\"');
				return ParseParts(payload, boundary);
			}

			private static string? GetBoundary(string? contentType)
			{
				if (string.IsNullOrWhiteSpace(contentType))
					return null;

				var parts = contentType!.Split(';');
				foreach (var part in parts)
				{
					var trimmed = part.Trim();
					if (!trimmed.StartsWith("boundary=", StringComparison.OrdinalIgnoreCase))
						continue;
					return trimmed["boundary=".Length..].Trim();
				}

				return null;
			}

			private static List<MultipartPart> ParseParts(string? payload, string boundary)
			{
				var parts = new List<MultipartPart>();
				if (string.IsNullOrWhiteSpace(payload))
					return parts;

				var delimiter = "--" + boundary;
				var sections = payload!.Split([delimiter], StringSplitOptions.RemoveEmptyEntries);

				foreach (var section in sections)
				{
					var trimmed = section.Trim('\r', '\n');
					if (trimmed == "--" || string.IsNullOrWhiteSpace(trimmed))
						continue;

					var split = trimmed.Split(["\r\n\r\n"], 2, StringSplitOptions.None);
					if (split.Length < 2)
						continue;

					var headers = ParseHeaders(split[0]);
					if (!headers.TryGetValue("Content-Disposition", out var contentDisposition))
						continue;

					var name = GetContentDispositionValue(contentDisposition, "name");
					if (string.IsNullOrWhiteSpace(name))
						continue;

					var fileName = GetContentDispositionValue(contentDisposition, "filename");
					var body = split[1].TrimEnd('\r', '\n');
					parts.Add(new MultipartPart(name!, body, fileName, headers));
				}

				return parts;
			}

			private static Dictionary<string, string> ParseHeaders(string headerBlock)
			{
				var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				var lines = headerBlock.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
				foreach (var line in lines)
				{
					var index = line.IndexOf(':');
					if (index <= 0)
						continue;

					var name = line[..index].Trim();
					var value = line[(index + 1)..].Trim();
					headers[name] = value;
				}

				return headers;
			}

			private static string? GetContentDispositionValue(string contentDisposition, string key)
			{
				var segments = contentDisposition.Split(';');
				foreach (var segment in segments)
				{
					var trimmed = segment.Trim();
					if (!trimmed.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
						continue;

					var value = trimmed[(key.Length + 1)..].Trim();
					if (value.Length >= 2 && value[0] == '\"' && value[^1] == '\"')
						value = value[1..^1];

					return value;
				}

				return null;
			}
		}
	}
}
