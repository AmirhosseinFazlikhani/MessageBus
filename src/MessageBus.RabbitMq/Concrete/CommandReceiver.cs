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
    internal class CommandReceiver : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly ILogger<CommandReceiver> _logger;
        private readonly IReadOnlyCollection<CommandModule> _modules;

        public CommandReceiver(
            IServiceScopeFactory scopeFactory,
            IConnection connection,
            ILogger<CommandReceiver> logger,
            IReadOnlyCollection<CommandModule> modules)
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

                        Task.Run(() =>
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var type = typeof(ICommandHandler<>);
                                var typeArgs = new Type[] { command.GetType() };
                                var serviceType = type.MakeGenericType(typeArgs);
                                dynamic service = scope.ServiceProvider.GetService(serviceType);
                                service.HandleAsync(command).Wait();
                            }

                            _logger.LogTrace("Command {Id} handled", (Guid)command.Id);
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        });
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
