using MessageBus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Concretes.Subscribers
{
    internal class EventSubscriber : BackgroundService
    {
        private readonly IChannelPool channelPool;
        private readonly ILogger<EventSubscriber> logger;
        private readonly HandlersStorage handlersStorage;
        private readonly MiddlewaresStorage middlewaresStorage;
        private readonly IServiceProvider serviceProvider;

        public EventSubscriber(
            IChannelPool channelPool,
            ILogger<EventSubscriber> logger,
            HandlersStorage handlersStorage,
            MiddlewaresStorage middlewaresStorage,
            IServiceProvider serviceProvider)
        {
            this.channelPool = channelPool;
            this.logger = logger;
            this.handlersStorage = handlersStorage;
            this.middlewaresStorage = middlewaresStorage;
            this.serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var item in GetMessageTypes())
            {
                var exchange = item.GetEventExchange();
                var channel = channelPool.Get();

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
                    item.Name,
                    exchange,
                    queue);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var message = (IMessage)ea.Body.Deserialize(item);

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

            return Task.CompletedTask;
        }

        private IEnumerable<Type> GetMessageTypes()
            => handlersStorage.EventCouples
                .ToList()
                .Select(x => x.Message)
                .Distinct();

        private async void HandleMessage(IMessage message)
        {
            var middlewareContext = ActivatorUtilities.CreateInstance<MiddlewareContext>(
                    serviceProvider,
                    new object[] {
                        middlewaresStorage.SubscriberMiddlewares
                    });

            await middlewareContext.Next(message);
        }
    }
}
