using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MessageBus.Concretes
{
    internal class MiddlewareContext : IMiddlewareContext
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<MiddlewareContext> logger;
        private readonly Type[] middlewares;

        public MiddlewareContext(
            IServiceProvider serviceProvider,
            ILogger<MiddlewareContext> logger,
            Type[] middlewares)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.middlewares = middlewares;
        }

        private int index = 0;

        public async Task Next(IMessage message)
        {
            logger.LogTrace("Message {HashCode} is processing by {Middleware}.",
                message.GetHashCode(),
                middlewares[index].FullName);

            var next = (IMiddleware)ActivatorUtilities.CreateInstance(serviceProvider, middlewares[index]);
            index += 1;
            await next.InvokeAsync(message, this);
            index = 0;
        }
    }
}
