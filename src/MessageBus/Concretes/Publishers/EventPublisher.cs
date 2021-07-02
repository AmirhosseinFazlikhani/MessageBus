using MessageBus.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace MessageBus.Concretes.Publishers
{
    public class EventPublisher : IPublisher<IEvent>, IDisposable
    {
        private readonly IChannelPool channelPool;
        private readonly ILogger<EventPublisher> logger;
        private readonly IModel channel;

        public EventPublisher(IChannelPool channelPool, ILogger<EventPublisher> logger)
        {
            this.channelPool = channelPool;
            this.logger = logger;
            channel = channelPool.Get();
        }

        public virtual Task PublishAsync(IEvent message)
        {
            var exchange = message.GetType().GetEventExchange();
            var body = message.Serialize();

            channel.BasicPublish(
                exchange: exchange,
                routingKey: "",
                basicProperties: null,
                body: body);

            logger.LogTrace("Message {HashCode} published. Exchange: {Exchange}",
                message.GetHashCode(),
                exchange);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            channelPool.Release(channel);
        }
    }
}
