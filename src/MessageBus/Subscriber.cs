using MessageBus.Concretes;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Type of message, for instance IEvent or ICommand.</typeparam>
    public abstract class Subscriber<T>
        where T : IMessage
    {
        private readonly IChannelPool channelPool;
        private readonly HandlersStorage handlersStorage;
        private readonly MiddlewaresStorage middlewaresStorage;
        private readonly IServiceScopeFactory serviceScopeFactory;

        protected Subscriber(
            IChannelPool channelPool,
            HandlersStorage handlersStorage,
            MiddlewaresStorage middlewaresStorage,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.channelPool = channelPool;
            this.handlersStorage = handlersStorage;
            this.middlewaresStorage = middlewaresStorage;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected abstract void Subscribe(Type messageType, IModel channel);

        public Task Execute()
        {
            foreach (var item in GetMessageTypes())
                Subscribe(item, channelPool.Get());

            return Task.CompletedTask;
        }

        private IEnumerable<Type> GetMessageTypes() => handlersStorage.Pairs
            .Select(x => x.Message)
            .Distinct()
            .Where(x => x.GetInterfaces().Any(y => y == typeof(T)));

        protected async void HandleMessage(IMessage message)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var middlewareContext = ActivatorUtilities.CreateInstance<MiddlewareContext>(
                    scope.ServiceProvider,
                    new object[] {
                    middlewaresStorage.SubscriberMiddlewares
                    });

                await middlewareContext.Next(message);
            }
        }
    }
}
