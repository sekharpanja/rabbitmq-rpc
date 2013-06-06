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
        private string _onwardsRoutingKey = "rpc";
        private string _returnRoutingKey = "rpc.reply";

        #region Constructors

        public Producer(string hostName, string exchangeName)
        {
            HostName = hostName;
            ExchangeName = exchangeName;
            //Setup(hostName, queueName);
            var connectionFactory = new ConnectionFactory
            {
                HostName = hostName, //"localhost",
                Port = AmqpTcpEndpoint.UseDefaultPort, //5672,
                UserName = ConnectionFactory.DefaultUser,   //"guest"
                Password = ConnectionFactory.DefaultPass,   //"guest"
                VirtualHost = ConnectionFactory.DefaultVHost //"/"
            };
            Connection = connectionFactory.CreateConnection();
            Channel = Connection.CreateModel();

            Channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            _client = new SampleRpcClient(Channel, ExchangeName, ExchangeType.Direct, _onwardsRoutingKey) { TimeoutMilliseconds = 5000 };
            _client.TimedOut += TimedOutHandler;
            _client.Disconnected += DisconnectedHandler;
        }

        #endregion

        #region Operations

        public byte[] Get(byte[] requestMessageBytes, string routingKey)
        {
            //_replyQueueName = Channel.QueueDeclare(_returnRoutingKey, false, false, false, null);
            //string corrId = Guid.NewGuid().ToString();
            IBasicProperties props = Channel.CreateBasicProperties();
            //props.ReplyTo = _replyQueueName;
            //props.CorrelationId = corrId;

            IBasicProperties replyProp;
            var response = _client.Call(props, requestMessageBytes, out replyProp);
            return response;
        }

        private void DisconnectedHandler(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            Console.WriteLine("Disconnected event!");
        }

        private void TimedOutHandler(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            Console.WriteLine("reuqest timed out event!");
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