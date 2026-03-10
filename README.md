# Transmitly.ChannelProvider.Mailgun.Api

Mailgun HTTP API-based email dispatcher implementation for [Transmitly](https://github.com/transmitly/transmitly).

## Recommended Package

Most users should use the convenience package instead:

- [Transmitly.ChannelProvider.Mailgun](https://github.com/transmitly/transmitly-channel-provider-mailgun)

That package registers this API implementation for you.

## What This Package Provides

- `EmailChannelProviderDispatcher` for the `Email` channel.
- `EmailDeliveryStatusReportAdaptor` for Mailgun webhook payloads.
- `EmailExtendedChannelProperties` for Mailgun-specific email settings.
- `DeliveryReportExtendedProperties` for Mailgun-specific delivery-report access.

## Using This Package Directly (Advanced)

```csharp
using Transmitly;
using Transmitly.ChannelProvider.Mailgun.Api;
using Transmitly.ChannelProvider.Mailgun.Api.Email;
using Transmitly.ChannelProvider.Mailgun.Configuration;

var builder = new CommunicationsClientBuilder();

var options = new MailgunOptions
{
	ApiKey = "key-your-mailgun-api-key",
	SendingDomain = "mg.example.com"
};

builder.ChannelProvider
	.Build(Id.ChannelProvider.Mailgun(), options)
	.AddDispatcher<EmailChannelProviderDispatcher, IEmail>(Id.Channel.Email())
	.AddEmailExtendedPropertiesAdaptor<EmailExtendedChannelProperties>()
	.AddDeliveryReportRequestAdaptor<EmailDeliveryStatusReportAdaptor>()
	.AddDeliveryReportExtendedProprtiesAdaptor<DeliveryReportExtendedProperties>()
	.Register();
```

## Related Packages

- [Transmitly](https://github.com/transmitly/transmitly)
- [Transmitly.ChannelProvider.Mailgun](https://github.com/transmitly/transmitly-channel-provider-mailgun)
- [Transmitly.ChannelProvider.Mailgun.Configuration](https://github.com/transmitly/transmitly-channel-provider-mailgun-configuration)

---
_Copyright (c) Code Impressions, LLC. This open-source project is sponsored and maintained by Code Impressions and is licensed under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
