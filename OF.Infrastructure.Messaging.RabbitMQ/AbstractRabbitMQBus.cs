using Newtonsoft.Json;
using OF.Infrastructure.Messaging.Bus;
using OF.Infrastructure.Messaging.Commands;
using OF.Infrastructure.Messaging.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Messaging.RabbitMQ
{
    public abstract class AbstractRabbitMQBus : IEventBus
    {

        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly ConnectionFactory factory;
        private Dictionary<string, IConnection> SubscriptionChannels; 

        public AbstractRabbitMQBus(ConnectionFactory factory,  Dictionary<string, IConnection> subscriptions)
        {
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            SubscriptionChannels = subscriptions;
            this.factory = factory;
        }


        public virtual void Publish<T>(T @event) where T : Event
        {
            //Uri _url = new Uri(Environment.GetEnvironmentVariable("ampqhost"));
            //var factory = new ConnectionFactory() { Uri = _url };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var eventName = @event.GetType().Name;
                    channel.QueueDeclare(eventName, false, false, false, null);
                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", eventName, true, null, body);
                }
            }
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            string eventName = typeof(T).Name;
            var handlerType = typeof(TH);
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }
            if (_handlers[eventName].Any(x => x.GetType() == handlerType))
            {

                throw new ArgumentException($"The handler Type {handlerType.Name} already exists for '{eventName}'", nameof(handlerType));
            }
            _handlers[eventName].Add(handlerType);
            StartBasicConsume<T>();
            //Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            try
            {

                factory.DispatchConsumersAsync = true;
                var eventName = typeof(T).Name;
                IConnection connection; 
                if (!SubscriptionChannels.TryGetValue(eventName, out connection)) {
                    connection = factory.CreateConnection();
                    SubscriptionChannels.Add(eventName, connection);
                }
                if (!connection.IsOpen) { // check if connection is usable
                    
                    SubscriptionChannels[eventName] = factory.CreateConnection();
                    connection = SubscriptionChannels[eventName];
                }

                //using (var channel = connection.CreateModel())
                //{
                    var channel = connection.CreateModel();
                    channel.QueueDeclare(eventName, false, false, false, null);
                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += Consumer_Received;
                    channel.BasicConsume(eventName, true, consumer);
                    Task.Delay(TimeSpan.FromSeconds(10));
                //};


            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace); 
            }
        }

        protected abstract Task Consumer_Received(object sender, BasicDeliverEventArgs e);


        protected Func<object, List<Type>, string, string, bool> SubscriptionProcessor =  (object handler, List<Type> _eventTypes, string eventName, string message) =>
        {
            if (handler == null) { return false; }
            Type eventType = _eventTypes.SingleOrDefault(x => x.Name == eventName);
            var @event = JsonConvert.DeserializeObject(message, eventType);
            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
            ((Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event })).GetAwaiter().GetResult();
            return true;
        };

        protected async Task ProcessEvent(string eventName, string message, object[] args)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    var handler = Activator.CreateInstance(subscription, args);
                    if (handler == null) {
                        System.Diagnostics.Trace.WriteLine($"subscription for {subscription}' is null");
                        continue;
                    }
                    await Task.FromResult(SubscriptionProcessor(handler, _eventTypes, eventName, message));
                }
            }
        }
    }

}
