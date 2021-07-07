using MessageBus.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace MessageBus.Concretes.Publishers
{
    public class EventPublisher : IPublisher<IEvent>
    {
        private readonly IChannelPool channelPool;
        private readonly ILogger<EventPublisher> logger;

        public EventPublisher(IChannelPool channelPool, ILogger<EventPublisher> logger)
        {
            this.channelPool = channelPool;
            this.logger = logger;
        }

        public virtual Task PublishAsync(IEvent message)
        {
            var exchange = message.GetType().GetEventExchange();
            var body = message.Serialize();
            var channel = channelPool.Get();

            channel.BasicPublish(
                exchange: exchange,
                routingKey: "",
                basicProperties: null,
                body: body);

            logger.LogTrace("Message {HashCode} published. Exchange: {Exchange}",
                message.GetHashCode(),
                exchange);

            channelPool.Release(channel);

            return Task.CompletedTask;
        }
    }
}
