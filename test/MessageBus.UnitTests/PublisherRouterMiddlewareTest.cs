using MessageBus.Concretes.Middlewars;
using MessageBus.Concretes.Publishers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.UnitTests
{
    public class PublisherRouterMiddlewareTest
    {
        private readonly Mock<CommandPublisher> commandPublisher;
        private readonly Mock<EventPublisher> eventPublisher;
        private readonly Mock<IServiceProvider> serviceProvider;
        private readonly Mock<IMiddlewareContext> middlewareContext;

        public PublisherRouterMiddlewareTest()
        {
            var channelPool = new Mock<IChannelPool>();
            var eventLogger = new Mock<ILogger<EventPublisher>>();
            var commandLogger = new Mock<ILogger<CommandPublisher>>();

            commandPublisher = new Mock<CommandPublisher>(channelPool.Object, commandLogger.Object);
            eventPublisher = new Mock<EventPublisher>(channelPool.Object, eventLogger.Object);
            middlewareContext = new Mock<IMiddlewareContext>();
            serviceProvider = new Mock<IServiceProvider>();

            serviceProvider.Setup(x => x.GetService(typeof(IPublisher<IEvent>)))
                .Returns(eventPublisher.Object);

            serviceProvider.Setup(x => x.GetService(typeof(IPublisher<ICommand>)))
                .Returns(commandPublisher.Object);
        }

        [Fact]
        public async void Invoke_MessageIsEvent_PassIMessageToEventEndpoint()
        {
            Type endpointType = null;

            commandPublisher.Setup(x => x.PublishAsync(It.IsAny<ICommand>()))
                .Returns(() => Task.FromResult(endpointType = typeof(CommandPublisher)));

            eventPublisher.Setup(x => x.PublishAsync(It.IsAny<IEvent>()))
                .Returns(() => Task.FromResult(endpointType = typeof(EventPublisher)));

            var middleware = new PublisherRouterMiddleware(serviceProvider.Object);
            await middleware.InvokeAsync(new TestEvent(), middlewareContext.Object);

            Assert.Equal(typeof(EventPublisher), endpointType);
        }

        private class TestEvent : IEvent { }
    }
}
