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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Transmitly.ChannelProvider.Mailgun.Configuration;
using Transmitly.Util;

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	public sealed class EmailChannelProviderDispatcher : ChannelProviderRestDispatcher<IEmail>
	{
		private const string SendMessagePath = "messages";
		private readonly MailgunOptions _options;

		public EmailChannelProviderDispatcher(MailgunOptions mailgunOptions)
			: base(CreateHttpClient(mailgunOptions))
		{
			_options = Guard.AgainstNull(mailgunOptions);
			Guard.AgainstNullOrWhiteSpace(_options.ApiKey);
			Guard.AgainstNullOrWhiteSpace(_options.SendingDomain);
		}

		protected override void ConfigureHttpClient(HttpClient httpClient)
		{
			RestClientConfiguration.Configure(httpClient, _options);
			base.ConfigureHttpClient(httpClient);
		}

		protected override async Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, IEmail email, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			Guard.AgainstNull(email);
			Guard.AgainstNull(communicationContext);

			Dispatch(communicationContext, email);

			var response = await restClient
				.PostAsync(SendMessagePath, await CreateMessageContent(email, communicationContext).ConfigureAwait(false), cancellationToken)
				.ConfigureAwait(false);

			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var result = new MailgunDispatchResult
			{
				Status = response.IsSuccessStatusCode
					? CommunicationsStatus.Success(MailgunConstant.Id, "Dispatched", (int)response.StatusCode, responseContent)
					: CommunicationsStatus.ServerError(MailgunConstant.Id, "Error", (int)response.StatusCode, responseContent)
			};

			if (response.IsSuccessStatusCode)
			{
				var success = TryDeserialize<MailgunSendResponse>(responseContent);
				result.ResourceId = success?.Id;
				Dispatched(communicationContext, email, [result]);
			}
			else
			{
				var error = TryDeserialize<MailgunErrorResponse>(responseContent);
				result.Exception = new MailgunException(error?.Message ?? "Mailgun request failed.", (int)response.StatusCode, responseContent);
				Error(communicationContext, email, [result]);
			}

			return [result];
		}

		private static HttpClient? CreateHttpClient(MailgunOptions? options)
		{
			if (options?.WebProxy == null)
				return null;

			return new HttpClient(new HttpClientHandler
			{
				Proxy = options.WebProxy
			});
		}

		private static async Task<HttpContent> CreateMessageContent(IEmail email, IDispatchCommunicationContext context)
		{
			var emailProperties = new EmailExtendedChannelProperties(email.ExtendedProperties);
			var hasTemplate = !string.IsNullOrWhiteSpace(emailProperties.Template);
			var form = new MultipartFormDataContent();

			AddStringContent(form, EmailField.From, email.From?.ToEmailAddress(), required: true);
			TryAddRecipients(email, form);
			AddStringContent(form, EmailField.Subject, email.Subject, required: !hasTemplate);

			if (!hasTemplate && string.IsNullOrWhiteSpace(email.TextBody) && string.IsNullOrWhiteSpace(email.HtmlBody))
			{
				throw new MailgunException("Either TextBody or HtmlBody is required when no template is provided.");
			}

			AddStringContent(form, EmailField.TextBody, email.TextBody);
			AddStringContent(form, EmailField.HtmlBody, email.HtmlBody);
			await TryAddAmpContent(email, context, form, emailProperties).ConfigureAwait(false);

			AddStringContent(form, EmailField.Template, emailProperties.Template);
			AddStringContent(form, EmailField.TemplateVersion, emailProperties.TemplateVersion);

			AddBooleanContent(form, EmailField.Dkim, emailProperties.DKIMSignatures);
			AddStringContent(form, EmailField.SecondaryDkim, emailProperties.SecondaryDKIM);
			AddStringContent(form, EmailField.SecondaryDkimPublic, emailProperties.PublicSecondaryDKIM);
			AddBooleanContent(form, EmailField.TestMode, emailProperties.TestMode);
			AddBooleanContent(form, EmailField.Tracking, emailProperties.Tracking);
			AddBooleanContent(form, EmailField.TrackingClicks, emailProperties.TrackingClicks);
			AddBooleanContent(form, EmailField.TrackingOpens, emailProperties.TrackingOpens);
			AddTrackingPixelLocation(form, emailProperties.TrackingPixelLocationTop);
			AddBooleanContent(form, EmailField.RequireTls, emailProperties.RequireTls);
			AddStringContent(form, EmailField.SendingIp, emailProperties.SendingIp);

			AddTags(form, emailProperties.Tags);
			AddReplyTo(form, email.ReplyTo);
			AddTemplateVariables(form, context);
			AddProperties(form, emailProperties.Properties);
			AddAttachments(form, email.Attachments);

			return form;
		}

		private static void TryAddRecipients(IEmail email, MultipartFormDataContent form)
		{
			var tos = email.To ?? [];
			var ccs = email.Cc ?? [];
			var bccs = email.Bcc ?? [];
			var total = tos.Length + ccs.Length + bccs.Length;

			if (total == 0)
				throw new MailgunException("At least one recipient is required.");

			if (total > 1000)
				throw new MailgunException("Recipient count exceeds max of 1000.");

			foreach (var to in tos)
			{
				form.Add(new StringContent(to.ToEmailAddress()), EmailField.To);
			}

			foreach (var cc in ccs)
			{
				form.Add(new StringContent(cc.ToEmailAddress()), EmailField.Cc);
			}

			foreach (var bcc in bccs)
			{
				form.Add(new StringContent(bcc.ToEmailAddress()), EmailField.Bcc);
			}
		}

		private static async Task TryAddAmpContent(IEmail email, IDispatchCommunicationContext context, MultipartFormDataContent form, EmailExtendedChannelProperties emailProperties)
		{
			var ampTemplate = emailProperties.AmpHtml.GetTemplateRegistration(context.CultureInfo, false);
			if (ampTemplate == null)
				return;

			if (string.IsNullOrWhiteSpace(email.HtmlBody))
				throw new MailgunException("HtmlBody is required when using AmpHtml.");

			var ampContent = await context.TemplateEngine.RenderAsync(ampTemplate, context).ConfigureAwait(false);
			AddStringContent(form, EmailField.AmpHtml, ampContent, required: true);
		}

		private static void AddStringContent(MultipartFormDataContent form, string key, string? value, bool required = false)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				if (required)
					throw new MailgunException($"Cannot add {key} value is null.");
				return;
			}

			form.Add(new StringContent(value), key);
		}

		private static void AddBooleanContent(MultipartFormDataContent form, string key, bool? value)
		{
			if (!value.HasValue)
				return;

			form.Add(new StringContent(value.Value ? "yes" : "no"), key);
		}

		private static void AddTrackingPixelLocation(MultipartFormDataContent form, bool? trackingPixelLocationTop)
		{
			if (trackingPixelLocationTop == true)
				form.Add(new StringContent("top"), EmailField.TrackingPixelLocation);
		}

		private static void AddTags(MultipartFormDataContent form, IReadOnlyCollection<string>? tags)
		{
			if (tags == null || tags.Count == 0)
				return;

			foreach (var tag in tags.Where(tag => !string.IsNullOrWhiteSpace(tag)))
			{
				form.Add(new StringContent(tag), EmailField.Tag);
			}
		}

		private static void AddReplyTo(MultipartFormDataContent form, IPlatformIdentityAddress[]? replyTo)
		{
			if (replyTo == null || replyTo.Length == 0)
				return;

			var replyToValue = string.Join(", ", replyTo.Select(address => address.ToEmailAddress()));
			AddStringContent(form, EmailField.ReplyTo, replyToValue);
		}

		private static void AddTemplateVariables(MultipartFormDataContent form, IDispatchCommunicationContext context)
		{
			if (context.ContentModel?.Model == null)
				return;

			var templateVariables = JsonSerializer.Serialize(context.ContentModel.Model);
			AddStringContent(form, EmailField.TemplateVariables, templateVariables);
		}

		private static void AddProperties(MultipartFormDataContent form, IDictionary<string, string>? properties)
		{
			if (properties == null || properties.Count == 0)
				return;

			foreach (var property in properties)
			{
				if (string.IsNullOrWhiteSpace(property.Key) || string.IsNullOrWhiteSpace(property.Value))
					continue;

				form.Add(new StringContent(property.Value), property.Key);
			}
		}

		private static void AddAttachments(MultipartFormDataContent form, IReadOnlyCollection<IEmailAttachment> attachments)
		{
			if (attachments == null || attachments.Count == 0)
				return;

			var index = 0;
			foreach (var attachment in attachments)
			{
				if (attachment?.ContentStream == null)
					continue;

				var fileName = string.IsNullOrWhiteSpace(attachment.Name) ? $"attachment-{++index}" : attachment.Name;
				var content = new StreamContent(attachment.ContentStream);

				if (!string.IsNullOrWhiteSpace(attachment.ContentType))
				{
					content.Headers.ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType);
				}

				form.Add(content, EmailField.Attachment, fileName);
			}
		}

		private static T? TryDeserialize<T>(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				return default;

			try
			{
				return JsonSerializer.Deserialize<T>(content);
			}
			catch (JsonException)
			{
				return default;
			}
		}
	}
}
