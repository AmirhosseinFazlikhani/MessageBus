using MessageBus.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace MessageBus.Concretes.Publishers
{
    public class CommandPublisher : IPublisher<ICommand>, IDisposable
    {
        private readonly IChannelPool channelPool;
        private readonly ILogger<CommandPublisher> logger;
        private readonly IModel channel;

        public CommandPublisher(IChannelPool channelPool, ILogger<CommandPublisher> logger)
        {
            this.channelPool = channelPool;
            this.logger = logger;
            channel = channelPool.Get();
        }

        public virtual Task PublishAsync(ICommand message)
        {
            var queue = message.GetType().GetCommandQueue();
            var body = message.Serialize();

            channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: null,
                body: body);

            logger.LogTrace("Message {HashCode} published. Queue: {Queue}",
                message.GetHashCode(),
                queue);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            channelPool.Release(channel);
        }
    }
}
