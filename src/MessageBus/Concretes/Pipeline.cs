using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Concretes
{
    internal class Pipeline : IPipeline
    {
        private readonly IMiddlewareFactory middlewareFactory;
        private readonly ILogger<Pipeline> logger;
        private readonly IEnumerator<Type> middlewares;

        public Pipeline(
            IMiddlewareFactory middlewareFactory,
            ILogger<Pipeline> logger,
            IEnumerator<Type> middlewares)
        {
            this.middlewareFactory = middlewareFactory;
            this.logger = logger;
            this.middlewares = middlewares;
        }

        public async Task Start(IMessage message)
        {
            await Next().Invoke(message);
        }

        private RequestDelegate Next() => async (message) =>
        {
            if (middlewares.MoveNext())
            {
                logger.LogTrace("Message {HashCode} is processing by {Middleware}.",
                    message.GetHashCode(),
                    middlewares.Current.FullName);

                var middleware = middlewareFactory.Create(middlewares.Current);
                await middleware.InvokeAsync(message, Next());
            }
        };
    }
}
