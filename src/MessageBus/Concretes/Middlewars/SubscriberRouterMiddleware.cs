using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus.Concretes.Middlewars
{
    public class SubscriberRouterMiddleware : IMiddleware
    {
        private readonly HandlersStorage storage;
        private readonly IServiceProvider provider;
        private readonly ILogger<SubscriberRouterMiddleware> logger;

        public SubscriberRouterMiddleware(
            HandlersStorage storage,
            IServiceProvider provider,
            ILogger<SubscriberRouterMiddleware> logger)
        {
            this.storage = storage;
            this.provider = provider;
            this.logger = logger;
        }

        public async Task InvokeAsync(IMessage message, RequestDelegate next)
        {
            var handlers = SelectHandlers(message);

            foreach (var item in handlers)
            {
                var handler = (dynamic)ActivatorUtilities.CreateInstance(provider, item);
                await handler.HandleAsync((dynamic)message);

                logger.LogTrace(
                    "Message {HashCode} handled by {Handler}",
                    message.GetHashCode(),
                    item.FullName);
            }
        }

        private IEnumerable<Type> SelectHandlers(IMessage message)
            => storage.Pairs.Where(x => x.Message == message.GetType()).Select(x => x.Handler);
    }
}
