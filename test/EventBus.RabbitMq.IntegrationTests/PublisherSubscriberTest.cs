using EventBus.RabbitMq.Concrete;
using eShop.EventBus.IntegrationTests.Common;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using EventBus.RabbitMq;

namespace eShop.EventBus.IntegrationTests
{
    public class PublisherSubscriberTest : IClassFixture<RabbitMqFixture>
    {
        private readonly IEventPublisher _publisher;
        private readonly IEventSubscriber<TestEvent> _subscriber;

        public PublisherSubscriberTest(RabbitMqFixture fixture)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var connection = fixture.Connection;
            var channelPool = new ChannelPool(
                connection,
                new EventBusSettings { MaxChannels = 1 },
                loggerFactory.CreateLogger<ChannelPool>());

            _publisher = new EventPublisher(channelPool, loggerFactory.CreateLogger<EventPublisher>());
            _subscriber = new EventSubscriber<TestEvent>(connection, loggerFactory.CreateLogger<EventSubscriber<TestEvent>>());
        }

        [Fact]
        public async void Publish_WhenPassedEvent_PublishEventInExchangeThatNameIsEventType()
        {
            var received = false;

            _subscriber.Connect();
            _subscriber.Receive((TestEvent @event) => received = true);

            await _publisher.PublishAsync(new TestEvent());

            await Task.Delay(10);

            Assert.True(received);
        }
    }

    public class TestEvent : IntegrativeEvent { }
}
