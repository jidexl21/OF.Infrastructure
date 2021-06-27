using MediatR;
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
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly Uri _uri;
        public RabbitMQBus(IMediator mediator, Dictionary<string, string> ampqConfig)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _uri = new Uri(ampqConfig["ampqhost"]);
            _eventTypes = new List<Type>(); 
        }

        public void Publish<T>(T @event) where T : Event
        {
            Uri _url = new Uri(Environment.GetEnvironmentVariable("ampqhost"));
            var factory = new ConnectionFactory() { Uri = _url };
            using (var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    var eventName = @event.GetType().Name;
                    channel.QueueDeclare(eventName, false, false, false, null);
                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", eventName, null, body);
                }
            }
        }

        public Task SendCommand<T>(T Command) where T : Command
        {
            return _mediator.Send(Command);
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            string eventName = typeof(T).Name;
            var handlerType = typeof(TH);
            if (!_eventTypes.Contains(typeof(T))) {
                _eventTypes.Add(typeof(T));
            }
            if (!_handlers.ContainsKey(eventName)) {
                _handlers.Add(eventName, new List<Type>());
            }
            if (_handlers[eventName].Any(x => x.GetType() == handlerType)) {

                throw new ArgumentException($"The handler Type {handlerType.Name} already exists for '{eventName}'", nameof(handlerType));
            }
            _handlers[eventName].Add(handlerType);
            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            
            var factory = new ConnectionFactory() { Uri = _uri, 
                DispatchConsumersAsync = true
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var eventName = typeof(T).Name;
            channel.QueueDeclare(eventName, false, false, false, null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            try {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception exc){ 
                
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName)) {
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions) {
                    var handler = Activator.CreateInstance(subscription); 
                    if(handler == null) { continue;  }
                    var eventType = _eventTypes.SingleOrDefault(x => x.Name == eventName);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }            
            }
        }
    }
}
