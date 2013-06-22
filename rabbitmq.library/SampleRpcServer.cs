using System;
using System.Text;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace rabbitmq.library
{
    public class SampleRpcServer : SimpleRpcServer
    {
        public SampleRpcServer(Subscription subscription) : base(subscription) {}

        public override byte[] HandleSimpleCall(bool isRedelivered, IBasicProperties requestProperties, byte[] body, out IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            //replyProperties.MessageId = Guid.NewGuid().ToString();

            string message = Encoding.UTF8.GetString(body);
            dynamic rpcRequest = JObject.Parse(message);
            Console.WriteLine("message received");
            Console.WriteLine(message);
            //string responseMessage = string.Format("response:{0}", message);
            string responseMessage = string.Empty;

            if (m_subscription.QueueName == "isabella.core.entitlements.get")
            {
                responseMessage = "{'_status': {'code': 'ok'}, '_response': {'customer_id': 123456}}";
                if (!message.Contains("service_code") ||
                    (message.Contains("service_code") && message.Contains("concierge")))
                    responseMessage =
                        @"{'_status': {'code': 'ok'},'_response': {'services': [{'entitlements': [{'duration': 3,'duration_left': 2,'start_date': '2013-06-14T14: 01: 14.019447','end_date': '2013-09-14T14: 01: 14.019447','level': 'dedicated'},{'duration': 19,'duration_left': 18,'start_date': '2013-06-14T14: 01: 14.019195','end_date': '2015-01-14T14: 01: 14.019195','level': 'classic'}],'name': 'Concierge'},{'entitlements': [{'duration': 3,'duration_left': 2,'start_date': '2013-06-14T14: 01: 14.019447','end_date': '2013-09-14T14: 01: 14.019447','phone_id': 1234,'serial': '217217301230129','remaining_tokens': 1}],'name': 'Certainty'},{'entitlements': [{'level': 'standard'}],'name': 'VertuEmail'},{'name': 'Checker'},{'name': 'Fortress2'},{'name': 'VertuSelect'}],'customer_id': '123456'}}";
                responseMessage.Replace("123456", rpcRequest.customer_id.ToString());
            }
            else if (m_subscription.QueueName == "isabella.core.facts.get_phone_facts")
            {
                responseMessage = "{'_status': {'code': 'ok'}, '_response': {'phone_id': 123456}}";
                if (!message.Contains("service_code") ||
                    (message.Contains("service_code") && message.Contains("concierge")))
                    responseMessage =
                        @"{'_status': {'code': 'ok'},'_response': {'services': [{'facts': [{'end_date': '2014-06-17T15:47:53.346817','level': 'classic','added_date': '2013-06-17T15:47:53.379867','is_active': true,'tied_to': 'customer','id': 4243,'reason_added': 'complimentary','duration': '12','modifier_ident': 'vreg','start_date': '2013-06-17T15:47:53.346817'},{'end_date': '2014-01-01T00: 00: 00','level': 'classic','added_date': '2013-06-17T15:47:53.380246','is_active': true,'tied_to': 'customer','id': 4244,'reason_added': 'complimentary','duration': '7','modifier_ident': 'vreg','start_date': '2013-06-17T15:47:53.376190'},{'end_date': '2013-09-17T15:47:53.378347','level': 'dedicated','added_date': '2013-06-17T15:47:53.380449','is_active': true,'tied_to': 'customer','id': 4245,'reason_added': 'purchase','duration': '3','modifier_ident': 'vreg','start_date': '2013-06-17T15:47:53.378347'}],'name': 'Concierge'},{'facts': [{'level': 'standard'}],'name': 'VertuEmail'},{'name': 'Checker'},{'name': 'Fortress2'},{'name': 'VertuSelect'}],'phone_id': '123456'}}";
                responseMessage.Replace("123456", rpcRequest.phone_id.ToString());
            }
            else if (m_subscription.QueueName == "isabella.core.facts.get_customer_facts")
            {
                responseMessage = "{'_status': {'code': 'ok'}, '_response': {'customer_id': 123456}}";
                if (!message.Contains("service_code") ||
                    (message.Contains("service_code") && message.Contains("concierge")))
                    responseMessage =
                        @"{'_status': {'code': 'ok'},'_response': {'services': [{'facts': [{'end_date': '2014-06-17T15:47:53.346817','level': 'classic','added_date': '2013-06-17T15:47:53.379867','is_active': true,'tied_to': 'customer','id': 4243,'reason_added': 'complimentary','duration': '12','modifier_ident': 'vreg','start_date': '2013-06-17T15:47:53.346817'},{'end_date': '2014-01-01T00:00:00','level': 'classic','added_date': '2013-06-17T15:47:53.380246','is_active': true,'tied_to': 'customer','id': 4244,'reason_added': 'complimentary','duration': '7','modifier_ident': 'vreg','start_date': '2013-06-17T15:47:53.376190'},{'end_date': '2013-09-17T15:47:53.378347','level': 'dedicated','added_date': '2013-06-17T15:47:53.380449','is_active': true,'tied_to': 'customer','id': 4245,'reason_added': 'purchase','duration': '3','modifier_ident': 'vreg','start_date': '2013-06-17T15:47:53.378347'}],'name': 'Concierge'}],'customer_id': '123456'}}";
                responseMessage.Replace("123456", rpcRequest.customer_id.ToString());
            }

            byte[] responseBody = Encoding.UTF8.GetBytes(responseMessage);

            return responseBody;
        }

        public override void ProcessRequest(BasicDeliverEventArgs evt)
        {
            IBasicProperties properties = evt.BasicProperties;
            if (!string.IsNullOrEmpty(properties.ReplyTo))
            {
                // It's a request.

                PublicationAddress replyAddress = PublicationAddress.Parse(properties.ReplyTo) ??
                                                  new PublicationAddress(ExchangeType.Direct, string.Empty, properties.ReplyTo);

                IBasicProperties replyProperties;
                byte[] reply = HandleCall(evt.Redelivered, properties, evt.Body, out replyProperties);
                if (replyProperties == null)
                {
                    replyProperties = m_subscription.Model.CreateBasicProperties();
                }

                replyProperties.CorrelationId = properties.CorrelationId;
                m_subscription.Model.BasicPublish(replyAddress, replyProperties, reply);
            }
            else
            {
                // It's an asynchronous message.
                HandleCast(evt.Redelivered, properties, evt.Body);
            }
        }

    }
}
