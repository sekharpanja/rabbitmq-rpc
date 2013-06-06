using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace rabbitmq.library
{
    public class SampleRpcServer : SimpleRpcServer
    {
        public SampleRpcServer(Subscription subscription) : base(subscription)
        {
        }

        public override byte[] HandleSimpleCall(bool isRedelivered, IBasicProperties requestProperties, byte[] body, out IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            Console.WriteLine(replyProperties.ReplyTo);
            //replyProperties.MessageId = Guid.NewGuid().ToString();

            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine("message received {0}", message);
            string responseMessage = string.Format("response:{0}", message);
            byte[] responseBody = Encoding.UTF8.GetBytes(responseMessage);

            return responseBody;
        }

        public override void ProcessRequest(BasicDeliverEventArgs evt)
        {
            IBasicProperties properties = evt.BasicProperties;
            if (!string.IsNullOrEmpty(properties.ReplyTo))
            {
                // It's a request.

                PublicationAddress replyAddress = PublicationAddress.Parse(properties.ReplyTo);
                if (replyAddress == null)
                {
                    replyAddress = new PublicationAddress(ExchangeType.Direct,
                                                          "",
                                                          properties.ReplyTo);
                }

                IBasicProperties replyProperties;
                byte[] reply = HandleCall(evt.Redelivered,
                                          properties,
                                          evt.Body,
                                          out replyProperties);
                if (replyProperties == null)
                {
                    replyProperties = m_subscription.Model.CreateBasicProperties();
                }

                replyProperties.CorrelationId = properties.CorrelationId;
                m_subscription.Model.BasicPublish(replyAddress,
                                                  replyProperties,
                                                  reply);
            }
            else
            {
                // It's an asynchronous message.
                HandleCast(evt.Redelivered, properties, evt.Body);
            }
        }

        //public override void ProcessRequest(BasicDeliverEventArgs evt)
        //{
        //    IBasicProperties properties = evt.BasicProperties;
        //    if (!string.IsNullOrEmpty(properties.ReplyTo))
        //    {
        //        // It's a request.

        //        PublicationAddress replyAddress = PublicationAddress.Parse(properties.ReplyTo) ??
        //                                          new PublicationAddress(ExchangeType.Direct, "rpcExchange",
        //                                                                 properties.ReplyTo);

        //        IBasicProperties replyProperties;
        //        byte[] reply = HandleCall(evt.Redelivered, properties, evt.Body, out replyProperties);
        //        if (replyProperties == null)
        //        {
        //            replyProperties = m_subscription.Model.CreateBasicProperties();
        //        }

        //        replyProperties.CorrelationId = properties.CorrelationId;
        //        m_subscription.Model.BasicPublish(replyAddress, replyProperties, reply);
        //    }
        //    else
        //    {
        //        // It's an asynchronous message.
        //        HandleCast(evt.Redelivered, properties, evt.Body);
        //    }
        //}

    }
}
