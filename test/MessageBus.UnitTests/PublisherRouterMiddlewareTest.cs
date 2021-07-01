using MessageBus.Concretes.Middlewars;
using MessageBus.Concretes.Publishers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.UnitTests
{
    public class PublisherRouterMiddlewareTest
    {
        [Fact]
        public async void Invoke_MessageIsEvent_PassIMessageToEventEndpoint()
        {
            Type endpointType = null;
            var middlewareContext = new Mock<IMiddlewareContext>();
            var channelPool = new Mock<IChannelPool>();
            var commandEndpoint = new Mock<CommandPublisher>(channelPool.Object);
            var eventEndpoint = new Mock<EventPublisher>(channelPool.Object);
            commandEndpoint.Setup(x => x.ProcessAsync(It.IsAny<IMessage>())).Returns(() => Task.FromResult(endpointType = typeof(CommandPublisher)));
            eventEndpoint.Setup(x => x.ProcessAsync(It.IsAny<IMessage>())).Returns(() => Task.FromResult(endpointType = typeof(EventPublisher)));

            var middleware = new PublisherRouterMiddleware(commandEndpoint.Object, eventEndpoint.Object);
            await middleware.InvokeAsync(new TestEvent(), middlewareContext.Object);

            Assert.Equal(typeof(EventPublisher), endpointType);
        }

        private class TestEvent : IEvent { }
    }
}
