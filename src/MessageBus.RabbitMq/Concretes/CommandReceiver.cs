using MessageBus.RabbitMq.Extensions;
using MessageBus.RabbitMq.Modules.Storage;
using MessageBus.RabbitMq.Modules.Storage.Enums;
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
    internal class CommandReceiver : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly ILogger<CommandReceiver> _logger;
        private readonly IReadOnlyCollection<CommandModule> _modules;
        private readonly IMessageStorage _storage;

        public CommandReceiver(
            IServiceScopeFactory scopeFactory,
            IConnection connection,
            ILogger<CommandReceiver> logger,
            IReadOnlyCollection<CommandModule> modules,
            IMessageStorage storage = null)
        {
            _scopeFactory = scopeFactory;
            _connection = connection;
            _logger = logger;
            _modules = modules;
            _storage = storage;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var module in _modules)
            {
                var queue = module.Command.GetCommandQueue();

                try
                {
                    var channel = _connection.CreateModel();
                    channel.QueueDeclare(
                        queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    _logger.LogInformation("Successfully connected to {Queue}", queue);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var command = (dynamic)ea.Body.Deserialize(module.Command);

                        _logger.LogTrace("Command {Id} received from {Queue}", (Guid)command.Id, queue);

                        try
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var type = typeof(ICommandHandler<>);
                                var typeArgs = new Type[] { command.GetType() };
                                var serviceType = type.MakeGenericType(typeArgs);
                                dynamic service = scope.ServiceProvider.GetService(serviceType);
                                service.HandleAsync(command).Wait();
                            }

                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                            _logger.LogTrace("Command {Id} handled", (Guid)command.Id);
                            _storage.SaveAsync(command, OperationType.Receive, OperationStatus.Succeeded).Wait();
                        }
                        catch (Exception exp)
                        {
                            _logger.LogError(exp, "An exception was thrown while handling command {CommandId}", (Guid)command.Id);
                            _storage.SaveAsync(command, OperationType.Receive, OperationStatus.Failed).Wait();
                        }
                    };

                    channel.BasicConsume(
                        queue: queue,
                        autoAck: false,
                        consumer: consumer);
                }
                catch (Exception exp)
                {
                    _logger.LogCritical(exp, "Faild to connect to {Queue}", queue);
                }
            }

            return Task.CompletedTask;
        }
    }
}
