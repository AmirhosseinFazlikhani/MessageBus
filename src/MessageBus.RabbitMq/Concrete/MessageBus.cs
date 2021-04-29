using MessageBus.RabbitMq.Extensions;
using MessageBus.RabbitMq.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Concrete
{
    internal class MessageBus : IMessageBus
    {
        private readonly IChannelPool _channelPool;
        private readonly ILogger<MessageBus> _logger;

        public MessageBus(
            IChannelPool channelPool,
            ILogger<MessageBus> logger)
        {
            _channelPool = channelPool;
            _logger = logger;
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
            }
            catch
            {
                _logger.LogError("Faild to publish event {Id} on {Exchange}",
                    @event.Id,
                    exchange);
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
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Faild to send command {Id} on {Queue}",
                    command.Id,
                    queue);
            }

            _channelPool.Release(channel);

            return Task.CompletedTask;
        }
    }
}
