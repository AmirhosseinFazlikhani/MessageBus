using Microsoft.Extensions.DependencyInjection;
using System;

namespace MessageBus.Concretes
{
    public class MiddlewareFactory : IMiddlewareFactory
    {
        private readonly IServiceProvider serviceProvider;

        public MiddlewareFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IMiddleware Create(Type type)
        {
            return serviceProvider.GetRequiredService(type) as IMiddleware;
        }
    }
}
