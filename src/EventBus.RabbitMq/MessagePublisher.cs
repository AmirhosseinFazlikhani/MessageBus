using EventBus.RabbitMq.Abstractions;
using EventBus.RabbitMq.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace EventBus.RabbitMq
{
	public class MessagePublisher : IMessagePublisher
	{
		private readonly IChannelPool _channelPool;
		private readonly ILogger<MessagePublisher> _logger;

		public MessagePublisher(
			IChannelPool channelPool,
			ILogger<MessagePublisher> logger)
		{
			_logger = logger;
			_channelPool = channelPool;
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

				_logger.LogTrace("[EventBus] Event raised. Id: {Id}, Exchange: {Exchange}",
					@event.Id,
					exchange);
			}
			catch
			{
				_logger.LogError("[EventBus] Faild to raise event. Id: {Id}, Exchange: {Exchange}",
					@event.Id,
					exchange);
			}

			_channelPool.Release(channel);

			return Task.CompletedTask;
		}
	}
}
