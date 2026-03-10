# Transmitly.ChannelProvider.Mailgun.Api

A [Transmitly](https://github.com/transmitly/transmitly) Mailgun channel provider dispatcher implementation for email and Mailgun delivery-report adaptation.

## Recommended Package

Most users should use the convenience package:

- [transmitly-channel-provider-mailgun](https://github.com/transmitly/transmitly-channel-provider-mailgun)

It provides `AddMailgunSupport(...)` and registers this dispatcher for you.

## Using This Package Directly (Advanced)

Use this package directly if you want explicit control over channel-provider registration and dispatcher wiring.

The convenience package registers this provider with the equivalent of:

```csharp
using Transmitly;
using Transmitly.ChannelProvider.Mailgun.Api;
using Transmitly.ChannelProvider.Mailgun.Api.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;

var builder = new CommunicationsClientBuilder();

var options = new MailgunOptions
{
	ApiKey = "key-your-mailgun-api-key",
	SendingDomain = "mg.example.com",
	// Optional for EU-region Mailgun accounts.
	// ApiHost = "https://api.eu.mailgun.net"
};

builder
	.ChannelProvider
	.Build(Id.ChannelProvider.Mailgun(), options)
	.AddDispatcher<EmailChannelProviderDispatcher, IEmail>(Id.Channel.Email())
	.AddEmailExtendedPropertiesAdaptor<EmailExtendedChannelProperties>()
	.AddDeliveryReportRequestAdaptor<EmailDeliveryStatusReportAdaptor>()
	.AddDeliveryReportExtendedProprtiesAdaptor<DeliveryReportExtendedProperties>()
	.Register();
```

See the [Transmitly](https://github.com/transmitly/transmitly) project for pipeline/channel concepts and end-to-end setup.

---
_Copyright (c) Code Impressions, LLC. This open-source project is sponsored and maintained by Code Impressions and is licensed under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
