using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace rabbitmq.library
{
    public class SampleRpcClient : SimpleRpcClient
    {
        public SampleRpcClient(IModel channel) : base(channel)
        {
        }

        public SampleRpcClient(IModel channel, string queueName) : base(channel, queueName)
        {
        }

        public SampleRpcClient(IModel channel, string exchange, string exchangeType, string routingKey) : base(channel, exchange, exchangeType, routingKey)
        {
        }

        public SampleRpcClient(IModel channel, PublicationAddress address) : base(channel, address)
        {
        }

        protected override void EnsureSubscription()
        {
            if (m_subscription == null)
            {
                //m_model.EnsureReplyQueue();
                string queueName = m_model.QueueDeclare("rpc.reply", false, false, false, null);
                m_subscription = new Subscription(m_model, queueName);
            }
        }

        public override BasicDeliverEventArgs Call(IBasicProperties requestProperties, byte[] body)
        {
            EnsureSubscription();

            if (requestProperties == null)
            {
                requestProperties = m_model.CreateBasicProperties();
            }
            if (string.IsNullOrEmpty(requestProperties.CorrelationId))
                requestProperties.CorrelationId = Guid.NewGuid().ToString();

                requestProperties.ReplyTo = m_subscription.QueueName;

            Cast(requestProperties, body);
            return RetrieveReply(requestProperties.CorrelationId);
        }

    }
}
