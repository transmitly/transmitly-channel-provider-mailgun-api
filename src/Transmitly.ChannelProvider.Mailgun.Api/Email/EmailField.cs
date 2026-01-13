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

namespace Transmitly.ChannelProvider.Mailgun.Api.Email
{
	internal static class EmailField
	{
		public const string From = "from";
		public const string To = "to";
		public const string Cc = "cc";
		public const string Bcc = "bcc";
		public const string Subject = "subject";
		public const string TextBody = "text";
		public const string HtmlBody = "html";
		public const string AmpHtml = "amp-html";
		public const string Template = "template";
		public const string TemplateVersion = "t:version";
		public const string Tag = "o:tag";
		public const string Dkim = "o:dkim";
		public const string SecondaryDkim = "o:secondary-dkim";
		public const string SecondaryDkimPublic = "o:secondary-dkim-public";
		public const string TestMode = "o:testmode";
		public const string Tracking = "o:tracking";
		public const string TrackingClicks = "o:tracking-clicks";
		public const string TrackingOpens = "o:tracking-opens";
		public const string TrackingPixelLocation = "o:tracking-pixel-location";
		public const string RequireTls = "o:require-tls";
		public const string SendingIp = "o:sending-ip";
		public const string ReplyTo = "h:Reply-To";
		public const string TemplateVariables = "h:X-Mailgun-Variables";
		public const string Attachment = "attachment";
	}
}
