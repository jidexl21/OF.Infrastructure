using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace OF.Infrastructure.Messaging
{
    public class Subscriber : ISubscriber
    {
        private IConnection _connection;
        private IModel _channel; 
        public Subscriber()
        {

        }
        public virtual async Task<(IConnection, IModel)> Connect() {
            await Task.Run(() => {
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
        public Task ListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested) { 
            
            
            }
            
        }
    }

    public class Consumer
    {
        private IConnection _connection;
        private IModel _channel;
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public void ConsumeQueue()
        {
            // CloudAMQP URL in format amqp://user:pass@hostName:port/vhost
            string _url = "amqp://guest:guest@localhost/%2f";
            // create a connection and open a channel, dispose them when done
            var factory = new ConnectionFactory
            {
                Uri = new Uri(url)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // ensure that the queue exists before we access it
            var queueName = "queue1";
            bool durable = false;
            bool exclusive = false;
            bool autoDelete = true;

            _channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);

            var consumer = new EventingBasicConsumer(_channel);

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
