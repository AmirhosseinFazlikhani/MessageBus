using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus.Concretes.Middlewars
{
    public class PublisherRouterMiddleware : IMiddleware
    {
        private readonly IServiceProvider serviceProvider;

        public PublisherRouterMiddleware(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(IMessage message, IMiddlewareContext context)
        {
            var @interface = message.GetType()
                .GetInterfaces()
                .Where(x => x != typeof(IMessage))
                .Single();

            var type = typeof(IPublisher<>);
            var typeArgs = new Type[] { @interface };
            var publisherType = type.MakeGenericType(typeArgs);
            var publisher = (dynamic)serviceProvider.GetService(publisherType);

            if (publisher is null)
                throw new NotImplementedException("Publisher for this message type was not implemented.");

            await publisher.PublishAsync((dynamic)message);
        }
    }
}
