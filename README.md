# Transmitly.ChannelProvider.{Name}

A [Transmitly](https://github.com/transmitly/transmitly) channel provider that {description}.

### Getting started

To use the {Name} channel provider, first install the [NuGet package](https://nuget.org/packages/transmitly.channelprovider.{name}):

```shell
dotnet add package Transmitly.ChannelProvider.{Name}
```

Then add the channel provider using `Add{Name}Support()`:

```csharp
using Transmitly;
...
var communicationClient = new CommunicationsClientBuilder()
	.Add{Name}Support(options =>
	{
		
	})
```
* See the [Transmitly](https://github.com/transmitly/transmitly) project for more details on what a channel provider is and how it can be configured.

---
_Copyright © Code Impressions, LLC.  This open-source project is sponsored and maintained by Code Impressions
and is licensed under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
