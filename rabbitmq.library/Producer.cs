using System;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;

namespace rabbitmq.library
{
    public class Producer : IDisposable
    {
        protected string HostName;
        protected IModel Channel;
        protected IConnection Connection;
        protected string ExchangeName;
        private string _replyQueueName;
        private SimpleRpcClient _client;

        #region Constructors

        public Producer(string hostName, string exchangeName)
        {
            HostName = hostName;
            ExchangeName = exchangeName;
            //Setup(hostName, queueName);
            var connectionFactory = new ConnectionFactory
            {
                HostName = hostName, //"localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
            Connection = connectionFactory.CreateConnection();
            Channel = Connection.CreateModel();

            Channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

        }

        #endregion

        #region Operations

        public byte[] Get(byte[] requestMessageBytes, string routingKey)
        {
            _client = new SimpleRpcClient(Channel, ExchangeName, ExchangeType.Direct, "rpc");
            _client.TimeoutMilliseconds = 5000; // defaults to infinity
            _client.TimedOut += TimedOutHandler;
            _client.Disconnected += DisconnectedHandler;

            _replyQueueName = Channel.QueueDeclare("rpc-reply", true, false, false, null);
            string corrId = Guid.NewGuid().ToString();
            IBasicProperties props = Channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = corrId;

            IBasicProperties replyProp;
            var response = _client.Call(props, requestMessageBytes, out replyProp);
            return response;
        }

        private void DisconnectedHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TimedOutHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            if (Connection != null)
                Connection.Close();
            if (Channel != null)
                Channel.Abort();
            if(_client != null)
                _client.Close();
        }
        #endregion
    }
}