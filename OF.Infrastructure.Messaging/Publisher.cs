using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Messaging
{

    public class Publisher : IPublisher
    {
        private readonly ConnectionFactory connectionFactory;
        public Publisher(ConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }
        /// <summary>
        /// requires a environment variable set as ampqhost to contain the connection string
        /// </summary>
        public Publisher()
        {
            string env  = Environment.GetEnvironmentVariable("ampqhost");
            this.connectionFactory = new ConnectionFactory() { Uri=new Uri(env)};
            
        }
        public async Task<bool> PublishAsync(IQueueMessage message)
        {
            await Task.Run(() => {
                using (var connection = connectionFactory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: message.Topic, type: ExchangeType.Fanout);
                        var body = Encoding.UTF8.GetBytes(message.Message);
                        channel.BasicPublish(exchange: message.Topic,
                                             routingKey: "",
                                             basicProperties: null,
                                             body: body);
                        Console.WriteLine(" [x] Sent {0} to Topic {1}", message.Message, message.Topic);
                    }
                }

            });
            return true;

        }
        
    }
}