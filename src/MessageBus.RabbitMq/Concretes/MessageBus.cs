using MessageBus.RabbitMq.Extensions;
using MessageBus.RabbitMq.Messages;
using MessageBus.RabbitMq.Modules.Storage;
using MessageBus.RabbitMq.Modules.Storage.Enums;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Concretes
{
    internal class MessageBus : IMessageBus
    {
        private readonly IChannelPool _channelPool;
        private readonly ILogger<MessageBus> _logger;
        private readonly IMessageStorage _storage;

        public MessageBus(
            IChannelPool channelPool,
            ILogger<MessageBus> logger,
            IMessageStorage storage = null)
        {
            _channelPool = channelPool;
            _logger = logger;
            _storage = storage;
        }

        public Task PublishAsync<T>(T @event) where T : IntegrativeEvent
        {
            var channel = _channelPool.Get();
            var exchange = typeof(T).GetEventExchange();

            try
            {
                channel.BasicPublish(
                    exchange: exchange,
                    routingKey: "",
                    basicProperties: null,
                    body: @event.Serialize());

                _logger.LogTrace("Event {Id} published on {Exchange}",
                    @event.Id,
                    exchange);

                Task.Run(() => _storage.Save(@event, OperationType.Send, OperationStatus.Succeeded));
            }
            catch
            {
                _logger.LogError("Faild to publish event {Id} on {Exchange}",
                    @event.Id,
                    exchange);

                Task.Run(() => _storage.Save(@event, OperationType.Send, OperationStatus.Failed));
            }

            _channelPool.Release(channel);

            return Task.CompletedTask;
        }

        public Task SendAsync<T>(T command) where T : Command
        {
            var queue = typeof(T).GetCommandQueue();
            var channel = _channelPool.Get();

            try
            {
                channel.QueueDeclare(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: queue,
                    basicProperties: null,
                    body: command.Serialize());

                _logger.LogTrace("Command {Id} sent on {Queue}",
                    command.Id,
                    queue);

                Task.Run(() => _storage.Save(command, OperationType.Send, OperationStatus.Succeeded));
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Faild to send command {Id} on {Queue}",
                    command.Id,
                    queue);

                Task.Run(() => _storage.Save(command, OperationType.Send, OperationStatus.Failed));
            }

            _channelPool.Release(channel);

            return Task.CompletedTask;
        }
    }
}
