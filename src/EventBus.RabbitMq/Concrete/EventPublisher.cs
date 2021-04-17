using EventBus.RabbitMq.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace EventBus.RabbitMq.Concrete
{
    internal class EventPublisher : IEventPublisher
    {
        private readonly IChannelPool _channelPool;
        private readonly ILogger<EventPublisher> _logger;

        public EventPublisher(
            IChannelPool channelPool,
            ILogger<EventPublisher> logger)
        {
            _channelPool = channelPool;
            _logger = logger;
        }

        public Task PublishAsync<T>(T @event) where T : IntegrativeEvent
        {
            var channel = _channelPool.Get();
            var exchange = typeof(T).GetExchange();

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
    }
}
