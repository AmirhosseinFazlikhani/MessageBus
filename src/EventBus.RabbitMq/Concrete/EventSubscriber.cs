using EventBus.RabbitMq.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace EventBus.RabbitMq.Concrete
{
    public class EventSubscriber<T> : IEventSubscriber<T> where T : IntegrativeEvent
    {
        private readonly IConnection _connection;
        private readonly ILogger<EventSubscriber<T>> _logger;

        public EventSubscriber(
            IConnection connection,
            ILogger<EventSubscriber<T>> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        private IModel _channel;

        private string _queue;

        public void Connect()
        {
            string exchange = typeof(T).GetExchange();

            try
            {
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: exchange,
                    type: ExchangeType.Fanout);

                _queue = _channel.QueueDeclare().QueueName;

                _channel.QueueBind(
                    queue: _queue,
                    exchange: exchange,
                    routingKey: "");

                _logger.LogInformation("Successfully connected to {Exchange}", exchange);
            }
            catch
            {
                _logger.LogCritical("Faild to connect to {Exchange}", exchange);
            }
        }

        public void Disconnect()
        {
            _channel.Close();
            _channel.Dispose();
        }

        public void Receive(Action<T> action)
        {
            var exchange = typeof(T).GetExchange();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var @event = ea.Body.Deserialize<T>();

                _logger.LogTrace("Event {Id} received from {Exchange}",
                    @event.Id,
                    exchange);

                action.Invoke(@event);

                _logger.LogTrace("Event {Id} handled", @event.Id);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(
                queue: _queue,
                autoAck: false,
                consumer: consumer);
        }
    }
}
