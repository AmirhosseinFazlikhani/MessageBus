using MessageBus.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace MessageBus.Concretes.Subscribers
{
    internal class EventSubscriber : Subscriber<IEvent>
    {
        private readonly ILogger<EventSubscriber> logger;

        public EventSubscriber(
            IServiceProvider serviceProvider,
            ILogger<EventSubscriber> logger) : base(serviceProvider)
        {
            this.logger = logger;
        }

        protected override void Subscribe(Type messageType, IModel channel)
        {
            try
            {
                var exchange = messageType.GetEventExchange();
                channel.ExchangeDeclare(
                    exchange: exchange,
                    type: ExchangeType.Fanout);

                var queue = channel.QueueDeclare().QueueName;

                channel.QueueBind(
                    queue: queue,
                    exchange: exchange,
                    routingKey: "");

                logger.LogInformation(
                    "Start subscribing {EventName}. Queue: {Queue} & Exchange: {Exchange}",
                    messageType.Name,
                    exchange,
                    queue);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var message = (IMessage)ea.Body.Deserialize(messageType);

                    logger.LogTrace(
                        "Message {HashCode} was received. Queue: {Queue} & Exchange: {Exchange}",
                        message.GetHashCode(),
                        queue,
                        exchange);

                    HandleMessage(message);

                    channel.BasicAck(ea.DeliveryTag, false);

                    logger.LogTrace("Message {HashCode} was completly processed.", message.GetHashCode());
                };

                channel.BasicConsume(
                    queue: queue,
                    autoAck: false,
                    consumer: consumer);
            }
            catch (Exception exp)
            {
                logger.LogError(
                    exp,
                    "An exception was thrown when trying to start subscribing event {Event}",
                    messageType.Name);
            }
        }
    }
}
