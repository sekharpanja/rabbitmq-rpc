using System;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;

namespace rabbitmq.library
{
    public class Consumer : IDisposable
    {
        protected string HostName;
        protected IModel Channel;
        protected IConnection Connection;
        protected string ExchangeName;
        protected string QueueName;
        protected Subscription subscription;
        private SampleRpcServer _sampleRpcServer;

        protected bool isConsuming;

        // used to pass messages back to UI for processing
        public delegate void OnReceiveMessage(byte[] message);
        public event OnReceiveMessage OnMessageReceived;

        public Consumer(string hostName, string exchangeName, string queueName)
        {
            HostName = hostName;
            ExchangeName = exchangeName;
            QueueName = queueName;
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

            Channel.QueueDeclare(QueueName, false, false, false, null);
            Channel.QueueBind(QueueName, ExchangeName, QueueName);
            subscription = new Subscription(Channel, QueueName);
        }

        //internal delegate to run the queue consumer on a seperate thread
        private delegate void ConsumeDelegate();

        public void StartConsuming()
        {
            isConsuming = true;
            ConsumeDelegate c = Consume;
            c.BeginInvoke(null, null);
        }

        public void Consume()
        {
            _sampleRpcServer = new SampleRpcServer(subscription);
            _sampleRpcServer.MainLoop();
        }

        public void Dispose()
        {
            isConsuming = false;
            if (Connection != null)
                Connection.Close();
            if (Channel != null)
                Channel.Abort();
            if(subscription != null)
                subscription.Close();
            if(_sampleRpcServer != null)
                _sampleRpcServer.Close();
        }
    }
}