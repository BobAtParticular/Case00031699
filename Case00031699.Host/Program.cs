using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Legacy;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

namespace Case00031699
{
    class Program
    {
        static void Main(string[] args)
        {
        	ServiceBus().GetAwaiter().GetResult();
        }

        static async Task ServiceBus()
        {
		    var config = new EndpointConfiguration("Case00031699");
		    config.SendFailedMessagesTo("Case00031699.Error");
		    config.AuditProcessedMessagesTo("Case00031699.Audit");
		    var transport = config.UseTransport<MsmqTransport>();
		    transport.Transactions(TransportTransactionMode.TransactionScope);
            transport.Routing().RegisterPublisher(typeof(PcAddedEventV1), "Case00031699");

		    config.AutoSubscribe();

            config.UsePersistence<InMemoryPersistence>();
            config.UsePersistence<MsmqPersistence, StorageType.Subscriptions>();

		    config.DisableFeature<TimeoutManager>();
		    config.UseSerialization<JsonSerializer>();
		    var bus = await Endpoint.Start(config);

            Console.WriteLine("Wait until the subscription is received and then press enter to publish");
            Console.ReadLine();

            await bus.Publish(new PcAddedEventV1());

            Console.WriteLine("Wait until the published event is processed and then press enter to exit");
            Console.ReadLine();

            await bus.Stop();
        }

        public class PcAddedEventV1 : IEvent
        { }

        public class PcAddedEventV1Handler : IHandleMessages<PcAddedEventV1>
        {
            static ILog Logger = LogManager.GetLogger<PcAddedEventV1Handler>();
            public Task Handle(PcAddedEventV1 message, IMessageHandlerContext context)
            {
                Logger.Info("PcAddedEventV1 event processed");
                return Task.FromResult(0);
            }
        }
    }
}
