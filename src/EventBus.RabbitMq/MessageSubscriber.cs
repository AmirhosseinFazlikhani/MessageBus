using EventBus.RabbitMq.Abstractions;
using EventBus.RabbitMq.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace EventBus.RabbitMq
{
	public class MessageSubscriber<T> : IMessageSubscriber<T> where T : IntegrativeEvent
	{
		private readonly IConnection _connection;
		private readonly ILogger<MessageSubscriber<T>> _logger;

		public MessageSubscriber(
			IConnection connection,
			ILogger<MessageSubscriber<T>> logger)
		{
			_logger = logger;
			_connection = connection;
		}

		private IModel channel;

		private string queue;

		public void Connect()
		{
			string exchange = typeof(T).GetExchange();

			try
			{
				channel = _connection.CreateModel();

				channel.ExchangeDeclare(
					exchange: exchange,
					type: ExchangeType.Fanout);

				queue = channel.QueueDeclare().QueueName;

				channel.QueueBind(
					queue: queue,
					exchange: exchange,
					routingKey: "");

				_logger.LogInformation("[EventBus] Successfully connected. Exchange: {Exchange}", exchange);
			}
			catch
			{
				_logger.LogError("[EventBus] Faild to connect. Exchange: {Exchange}", exchange);
			}
		}

		public void Disconnect()
		{
			channel.Close();
			channel.Dispose();
		}

		public void Received(Action<T> action)
		{
			var exchange = typeof(T).GetExchange();

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				var @event = ea.Body.Deserialize<T>();

				_logger.LogTrace("[EventBus] Event received. Id: {Id}, Exchange: {Exchange}, CreatedAt: {CreatedAt}",
					@event.Id,
					exchange,
					@event.CreateDateTime);

				action.Invoke(@event);

				var totalHandleTime = DateTime.UtcNow - @event.CreateDateTime;
				_logger.LogTrace("[EventBus] Event handled. Id: {Id}, Exchange: {Exchange}, TotalTime: {TotalTime}",
					@event.Id,
					exchange,
					totalHandleTime);

				channel.BasicAck(ea.DeliveryTag, false);
			};

			channel.BasicConsume(
				queue: queue,
				autoAck: false,
				consumer: consumer);
		}
	}
}
