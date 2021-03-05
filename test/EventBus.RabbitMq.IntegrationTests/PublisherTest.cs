using EventBus.RabbitMq.Abstractions;
using EventBus.RabbitMq.IntegrationTests.Base;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.RabbitMq.IntegrationTests
{
	public class PublisherTest:IClassFixture<RabbitMqFixture>
	{
		private readonly IMessagePublisher _publisher;
		private readonly IConnection _connection;

		public PublisherTest(RabbitMqFixture fixture)
		{
			_connection = fixture.Connection;

			var pool = new ChannelPool(_connection, 1);
			_publisher = new MessagePublisher(pool, new NullLogger<MessagePublisher>());
		}

		[Fact]
		public async void Publish_WhenPassAnEvent_PublishItWithRoutingKeyThatContainsItTypeName()
		{
			var @event = new TestIE();
			TestIE receivedEvent = null;

			#region subscribe
			var exchange = "eventbus";
			var routingKey = "TestIE";

			var channel = _connection.CreateModel();

			channel.ExchangeDeclare(
					exchange: exchange,
					type: ExchangeType.Fanout);

			var queue = channel.QueueDeclare().QueueName;

			channel.QueueBind(
				queue: queue,
				exchange: exchange,
				routingKey: routingKey);

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);
				var result = JsonSerializer.Deserialize<TestIE>(message);

				receivedEvent = result;
				channel.BasicAck(ea.DeliveryTag, false);
			};

			channel.BasicConsume(
				queue: queue,
				autoAck: false,
				consumer: consumer);
			#endregion

			await _publisher.PublishAsync(@event);

			await Task.Delay(50);
			Assert.Equal(@event.Id, receivedEvent.Id);
		}

		private class TestIE : IntegrativeEvent { }
	}
}
