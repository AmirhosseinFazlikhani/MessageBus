using MessageBus.RabbitMq.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Concrete
{
    internal class EventSubscriber : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly ILogger<EventSubscriber> _logger;
        private readonly IReadOnlyCollection<EventModule> _modules;

        public EventSubscriber(
            IServiceScopeFactory scopeFactory,
            IConnection connection,
            ILogger<EventSubscriber> logger,
            IReadOnlyCollection<EventModule> modules)
        {
            _scopeFactory = scopeFactory;
            _connection = connection;
            _logger = logger;
            _modules = modules;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var module in _modules)
            {
                var exchange = module.Event.GetEventExchange();

                try
                {
                    var channel = _connection.CreateModel();
                    channel.ExchangeDeclare(
                        exchange: exchange,
                        type: ExchangeType.Fanout);

                    var queue = channel.QueueDeclare().QueueName;
                    channel.QueueBind(
                        queue: queue,
                        exchange: exchange,
                        routingKey: "");

                    _logger.LogInformation("Successfully connected to {Exchange}", exchange);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var @event = (dynamic)ea.Body.Deserialize(module.Event);

                        _logger.LogTrace("Event {Id} received from {Exchange}", (Guid)@event.Id, exchange);

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var service = (dynamic)ActivatorUtilities.CreateInstance(
                                scope.ServiceProvider,
                                module.Handler);

                            service.HandleAsync(@event).Wait();
                        }

                        _logger.LogTrace("Event {Id} handled", (Guid)@event.Id);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    channel.BasicConsume(
                        queue: queue,
                        autoAck: false,
                        consumer: consumer);
                }
                catch (Exception exp)
                {
                    _logger.LogCritical(exp, "Faild to connect to {Exchange}", exchange);
                }
            }

            return Task.CompletedTask;
        }
    }
}
