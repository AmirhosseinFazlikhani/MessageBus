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
    internal class CommandSubscriber : BackgroundService
    {
        private readonly IChannelPool channelPool;
        private readonly ILogger<EventSubscriber> logger;
        private readonly HandlersStorage handlersStorage;
        private readonly MiddlewaresStorage middlewaresStorage;
        private readonly IServiceProvider serviceProvider;

        public CommandSubscriber(
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
                try
                {
                    var channel = channelPool.Get();

                    var queue = item.GetCommandQueue();
                    channel.QueueDeclare(
                        queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    logger.LogInformation(
                        "Start subscribing {EventName}. Queue: {Queue} & Exchange: {Exchange}",
                        item.Name,
                        string.Empty,
                        queue);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var message = (IMessage)ea.Body.Deserialize(item);

                        logger.LogTrace(
                            "Message {HashCode} was received. Queue: {Queue} & Exchange: {Exchange}",
                            message.GetHashCode(),
                            queue,
                            string.Empty);

                        HandleMessage(message);

                        channel.BasicAck(ea.DeliveryTag, false);

                        logger.LogTrace(
                            "Message {HashCode} was completly processed.",
                            message.GetHashCode());
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
                        "An exception was thrown when trying to start subscribing command {Command}",
                        item.Name);
                }
            }

            return Task.CompletedTask;
        }

        private IEnumerable<Type> GetMessageTypes()
            => handlersStorage.CommandCouples
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
