using System;
using System.Text;
using RabbitMQ.Client.MessagePatterns;

namespace rabbitmq.library
{
    public class SampleRpcServer : SimpleRpcServer
    {
        public SampleRpcServer(Subscription subscription) : base(subscription)
        {
        }

        //public override byte[] HandleCall(bool isRedelivered, RabbitMQ.Client.IBasicProperties requestProperties, byte[] body, out RabbitMQ.Client.IBasicProperties replyProperties)
        //{
        //    string message = Encoding.UTF8.GetString(body);
        //    Console.WriteLine("message received {0}", message);
        //    string responseMessage = string.Format("response:{0}", message);
        //    byte[] responseBody = Encoding.UTF8.GetBytes(responseMessage);
        //    return base.HandleCall(isRedelivered, requestProperties, responseBody, out replyProperties);
        //}

        //public override void ProcessRequest(RabbitMQ.Client.Events.BasicDeliverEventArgs evt)
        //{
        //    string message = Encoding.UTF8.GetString(evt.Body);
        //    Console.WriteLine("message received {0}", message);
        //    string responseMessage = string.Format("response:{0}", message);
        //    byte[] responseBody = Encoding.UTF8.GetBytes(responseMessage);
        //    evt.Body = Encoding.UTF8.GetBytes(responseMessage);
        //    base.ProcessRequest(evt);
        //}

        public override byte[] HandleSimpleCall(bool isRedelivered, RabbitMQ.Client.IBasicProperties requestProperties, byte[] body, out RabbitMQ.Client.IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            //replyProperties.MessageId = Guid.NewGuid().ToString();

            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine("message received {0}", message);
            string responseMessage = string.Format("response:{0}", message);
            byte[] responseBody = Encoding.UTF8.GetBytes(responseMessage);

            return responseBody;
            //return base.HandleSimpleCall(isRedelivered, requestProperties, body, out replyProperties);
        }
    }
}
