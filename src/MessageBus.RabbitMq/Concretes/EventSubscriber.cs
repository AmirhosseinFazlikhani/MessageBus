using MessageBus.RabbitMq.Extensions;
using MessageBus.RabbitMq.Modules.Storage;
using MessageBus.RabbitMq.Modules.Storage.Enums;
using MessageBus.RabbitMq.Modules.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Concretes
{
    internal class EventSubscriber : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly ILogger<EventSubscriber> _logger;
        private readonly IReadOnlyCollection<EventModule> _modules;
        private readonly IMessageStorage _storage;
        private readonly IMessageValidation _validation;

        public EventSubscriber(
            IServiceScopeFactory scopeFactory,
            IConnection connection,
            ILogger<EventSubscriber> logger,
            IReadOnlyCollection<EventModule> modules,
            IMessageStorage storage = null,
            IMessageValidation validation = null)
        {
            _scopeFactory = scopeFactory;
            _connection = connection;
            _logger = logger;
            _modules = modules;
            _storage = storage;
            _validation = validation;
        }

        private volatile object _lock = new object();

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
                        lock (_lock)
                        {
                            var @event = (dynamic)ea.Body.Deserialize(module.Event);

                            _logger.LogTrace("Event {Id} received from {Exchange}", (Guid)@event.Id, exchange);

                            try
                            {
                                _validation?.ThrowIfDuplicate(@event, module.Handler);

                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var service = (dynamic)ActivatorUtilities.CreateInstance(
                                        scope.ServiceProvider,
                                        module.Handler);

                                    service.HandleAsync(@event).Wait();
                                }

                                channel.BasicAck(ea.DeliveryTag, false);

                                _logger.LogTrace("Event {Id} handled", (Guid)@event.Id);
                                _storage?.Save(@event, OperationType.Receive, OperationStatus.Succeeded, module.Handler);
                            }
                            catch (Exception exp)
                            {
                                _logger.LogError(exp, "An exception was thrown while handling event {EventId}", (Guid)@event.Id);
                                _storage?.Save(@event, OperationType.Receive, OperationStatus.Failed, module.Handler);
                            }
                        }
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
