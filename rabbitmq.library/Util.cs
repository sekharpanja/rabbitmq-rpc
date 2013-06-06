using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace rabbitmq.library
{
    public static class Util
    {
        public const string ReplyQueueId = "rpc.reply";
        public const string ExchangeName = "rpcExchange";

        public static string EnsureReplyQueue(this IModel channel)
        {
            Console.WriteLine("Creating a queue");

            string queueName = channel.QueueDeclare(ReplyQueueId, false, false, false, null);

            //channel.QueueBind(queueName, ExchangeName, "", null);

            Console.WriteLine("Done. Created queue {0}.\n", queueName);

            return queueName;
        }
    }
}
