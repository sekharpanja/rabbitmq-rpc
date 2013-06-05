using System;
using System.Linq;
using System.Threading;
using rabbitmq.library;

namespace rabbitmq_consumer
{
    class ConsumerConsole
    {
        public static string HostName = "localhost";
        public static string ExchangeName = "sampleExchange";
        public static string QueueName = "rpc";

        private static Consumer _consumer;
        
        static void Main(string[] args)
        {
            //create the consumer
            using (_consumer = new Consumer(HostName, ExchangeName, QueueName))
            {

                //listen for message events
                _consumer.OnMessageReceived += HandleMessage;

                //start consuming
                _consumer.StartConsuming();
            }
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
}
