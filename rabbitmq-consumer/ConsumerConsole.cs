using System;
using System.Linq;
using System.Threading;
using rabbitmq.library;
using Topshelf;

namespace rabbitmq_consumer
{
    internal class AmqpConsumer
    {
        public static string HostName = "localhost";
        public static string ExchangeName = "rpc.test";
        private static string getEntsQuName = "isabella.core.entitlements.get";
        private static string getPhoneFactsQuName = "isabella.core.facts.get_phone_facts";
        private static string getCustomerFactsQuName = "isabella.core.facts.get_customer_facts";

        private static Consumer _entsConsumer;
        private static Consumer _pfactsConsumer;
        private static Consumer _cfactsConsumer;

        public void Start()
        {
            //create the consumer
            _entsConsumer = new Consumer(HostName, ExchangeName, getEntsQuName);
            _pfactsConsumer = new Consumer(HostName, ExchangeName, getPhoneFactsQuName);
            _cfactsConsumer = new Consumer(HostName, ExchangeName, getCustomerFactsQuName);


            //listen for message events
            _entsConsumer.OnMessageReceived += HandleMessage;

            //start consuming
            _entsConsumer.StartConsuming();
            _pfactsConsumer.StartConsuming();
            _cfactsConsumer.StartConsuming();
        }

        public void Stop()
        {
            _entsConsumer.Dispose();
            _pfactsConsumer.Dispose();
            _cfactsConsumer.Dispose();
        }

        //Callback for message receive
        public static void HandleMessage(byte[] message)
        {

            //this.Invoke(s, System.Text.Encoding.UTF8.GetString(message) + Environment.NewLine);
            string msgString = System.Text.Encoding.UTF8.GetString(message);
            Thread.Sleep(msgString.Count(d => d == '.'));
            Console.WriteLine(msgString);
        }
    }

    public class Program
    {
        public static void Main()
        {
            HostFactory.Run(x =>
                                {
                                    x.Service<AmqpConsumer>(s =>
                                                                {
                                                                    s.ConstructUsing(name => new AmqpConsumer());
                                                                    s.WhenStarted(tc => tc.Start());
                                                                    s.WhenStopped(tc => tc.Stop());
                                                                });
                                    x.RunAsLocalSystem();

                                    x.SetDescription("Sample Amqp Consumer Host for Isabella");
                                    x.SetDisplayName("IsabellaAmqpConsumer");
                                    x.SetServiceName("IsabellaAmqpConsumer");
                                });
        }
    }
}
