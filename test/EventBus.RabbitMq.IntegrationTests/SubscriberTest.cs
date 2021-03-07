using EventBus.RabbitMq.Abstractions;
using EventBus.RabbitMq.IntegrationTests.Base;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.RabbitMq.IntegrationTests
{
	public class SubscriberTest : IClassFixture<RabbitMqFixture>
	{
		private readonly IMessageSubscriber<TestIE> _subscriber;
		private readonly IConnection _connection;

		public SubscriberTest(RabbitMqFixture fixture)
		{
			_connection = fixture.Connection;
			_subscriber = new MessageSubscriber<TestIE>(_connection, new NullLogger<MessageSubscriber<TestIE>>());
		}

		[Fact]
		public async void Subscribe_WhenReceivedEvent_InvokePassedAction()
		{
			var @event = new TestIE();
			TestIE receivedEvent = null;

			_subscriber.Connect();
			_subscriber.Received((TestIE @event) => receivedEvent = @event);

			#region Publish
			var exchange = "TestIE";

			var json = JsonSerializer.Serialize(@event);
			var bytes = Encoding.ASCII.GetBytes(json);

			var channel = _connection.CreateModel();

			channel.BasicPublish(
					exchange: exchange,
					routingKey: "",
					basicProperties: null,
					body: bytes);

			channel.Close();
			channel.Dispose();
			#endregion

			await Task.Delay(50);
			_subscriber.Disconnect();

			Assert.Equal(@event.Id, receivedEvent.Id);
		}

		private class TestIE : IntegrativeEvent { }
	}
}
