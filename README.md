# Case00031699
Case 00031699 Reproduction

## Reproduction

For MSMQ to subscribe it must know which endpoint publishes the message :

```csharp
transport.Routing().RegisterPublisher(typeof(PcAddedEventV1), "Case00031699");
```
This will tell the endpoint where to send the subscription message to.

In a simple example like this, where a `Publish` is called immediately after startup, a race condition can occur where the subscription message sent above is not received and processed before the publish is sent. 

We can alleviate this by making sure the subscription message is processed before the `Publish` is made with a simple pause, in this case a `Console.Readline` prompting the user to wait until the subscription is processed before proceeding.

In production you will need to configure a durable subscription persistence for MSMQ subscriptions:

```csharp
config.UsePersistence<MsmqPersistence, StorageType.Subscriptions>();
```

This means on subsequent startups the publisher will _already_ have the subscription stored.

## Production Code

In production you may _still_ need to deal with this potential race condition immediately after deployment, otherwise message publishes may not be sent if the subscriptions have not yet been processed.

In this scenario you will want to _preload_ the subscriptions in the persistence of your choice. For instance, in the case of SQL persistence, this would mean creating a script that is run as part of deployment which already has the necessary subscription entries.

Generating those scripts by querying the persistence in lower environments is usually the easiest approach. 