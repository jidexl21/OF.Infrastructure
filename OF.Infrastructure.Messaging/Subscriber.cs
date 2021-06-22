using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace OF.Infrastructure.Messaging
{
    public class Subscriber : ISubscriber
    {
        private IConnection _connection;
        private IModel _channel;
        private CancellationToken cancellationToken;
        private string[] topics;
        private readonly ConnectionFactory connectionFactory;

        public Subscriber(IEnumerable<string> topics)
        {
            this.topics = topics.ToArray();
            // CloudAMQP URL in format amqp://user:pass@hostName:port/vhost
            string url = Environment.GetEnvironmentVariable("ampqhost");
            // create a connection and open a channel, dispose them when done
            connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(url)
            };
            var connectionTask = Connect();
            connectionTask.Wait();
            _connection = connectionTask.Result.Item1;
            _channel = connectionTask.Result.Item2;
        }
        public virtual async Task<(IConnection, IModel)> Connect()
        {
            await Task.Run(() =>
            {
                // CloudAMQP URL in format amqp://user:pass@hostName:port/vhost
                string url = Environment.GetEnvironmentVariable("ampqhost");
                // create a connection and open a channel, dispose them when done
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(url)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            });

            return (_connection, _channel);
        }
        public async Task ListenAsync(CancellationToken token=new CancellationToken())
        {
            cancellationToken = token;
            Console.WriteLine($"Listening on topics: {string.Join(",",topics)}");
            await Task.Run(() =>
            {

                while (!token.IsCancellationRequested)
                {
                    var tasks = topics.Select(x =>
                    {
                        var c = new Consumer(x, connectionFactory);
                        return Task.Run(() => c.ConsumeQueue());
                    });
                    Task.WaitAll(tasks.ToArray(), token);
                    Task.Delay(2000).Wait();
                }
            });
        }
    }

    public class Consumer
    {
        private IConnection _connection;
        private IModel _channel;
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly string topic;
        private readonly ConnectionFactory connectionFactory;
        public Consumer(string topic, ConnectionFactory connectionFactory)
        {
            this.topic = topic;
            this.connectionFactory = connectionFactory;
        }
        public void ConsumeQueue()
        {

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            // ensure that the queue exists before we access it
            var queueName = topic;
            bool durable = false;
            bool exclusive = false;
            bool autoDelete = true;

            _channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);

            var consumer = new EventingBasicConsumer(_channel);
            Console.WriteLine("Trying...");
            // add the message receive event
            consumer.Received += (model, deliveryEventArgs) =>
            {
                var body = deliveryEventArgs.Body.ToArray();
                // convert the message back from byte[] to a string
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("** Received message: {0} by Consumer thread **", message);
                // ack the message, ie. confirm that we have processed it
                // otherwise it will be requeued a bit later
                _channel.BasicAck(deliveryEventArgs.DeliveryTag, false);
            };

            // start consuming
            _ = _channel.BasicConsume(consumer, queueName);
            // Wait for the reset event and clean up when it triggers
            _resetEvent.WaitOne();
            _channel?.Close();
            _channel = null;
            _connection?.Close();
            _connection = null;
        }
    }
}
