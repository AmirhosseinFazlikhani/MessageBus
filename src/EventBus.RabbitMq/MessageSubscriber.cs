using EventBus.RabbitMq.Abstractions;
using EventBus.RabbitMq.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace EventBus.RabbitMq
{
	public class MessageSubscriber : IMessageSubscriber
	{
		private readonly IConnection _connection;
		private readonly ILogger<MessageSubscriber> _logger;

		public MessageSubscriber(
			IConnection connection,
			ILogger<MessageSubscriber> logger)
		{
			_logger = logger;
			_connection = connection;
		}

		private IModel channel;

		private string queue;

		private const string Exchange = "eventbus";

		public void Connect<T>() where T : IntegrativeEvent => Connect(typeof(T).GetRoutingKey());

		public void Connect(Type type) => Connect(type.GetRoutingKey());

		private void Connect(string routingKey)
		{
			try
			{
				channel = _connection.CreateModel();

				channel.ExchangeDeclare(
					exchange: Exchange,
					type: ExchangeType.Fanout);

				queue = channel.QueueDeclare().QueueName;

				channel.QueueBind(
					queue: queue,
					exchange: Exchange,
					routingKey: routingKey);

				_logger.LogInformation("[EventBus] Start subscribing. Exchange: {Exchange}", Exchange);
			}
			catch
			{
				_logger.LogError("[EventBus] Faild to subcribe. Exchange: {Exchange}", Exchange);
			}
		}

		public void Disconnect()
		{
			channel.Close();
			channel.Dispose();
		}

		public void Received<T>(Action<T> action) where T : IntegrativeEvent
		{
			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				var @event = ea.Body.Deserialize<T>();

				_logger.LogTrace("[EventBus] Event received. Id: {Id}, Exchange: {Exchange}, CreatedAt: {CreatedAt}",
					@event.Id,
					@event.CreateDateTime,
					Exchange);

				action.Invoke(@event);

				var totalHandleTime = DateTime.Now - @event.CreateDateTime;
				_logger.LogTrace("[EventBus] Event handled. Id: {Id}, Exchange: {Exchange}, TotalTime: {TotalTime}",
					@event.Id,
					Exchange,
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
