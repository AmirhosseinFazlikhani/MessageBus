using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MessageBus.Concretes
{
    internal class MessagePublisher : IMessagePublisher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly MiddlewaresStorage middlewaresStorage;

        public MessagePublisher(
            IServiceProvider serviceProvider,
            MiddlewaresStorage middlewaresStorage)
        {
            this.serviceProvider = serviceProvider;
            this.middlewaresStorage = middlewaresStorage;
        }

        public async Task PublishAsync(IMessage message)
        {
            var middlewareContext = ActivatorUtilities.CreateInstance<MiddlewareContext>(serviceProvider, new object[] { middlewaresStorage.PublisherMiddlewares });
            await middlewareContext.Next(message);
        }
    }
}
