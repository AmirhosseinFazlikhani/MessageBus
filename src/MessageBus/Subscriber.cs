using MessageBus.Concretes;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly IServiceProvider serviceProvider;

        protected Subscriber(
            IChannelPool channelPool,
            HandlersStorage handlersStorage,
            MiddlewaresStorage middlewaresStorage,
            IServiceProvider serviceProvider)
        {
            this.channelPool = channelPool;
            this.handlersStorage = handlersStorage;
            this.middlewaresStorage = middlewaresStorage;
            this.serviceProvider = serviceProvider;
        }

        protected abstract void Subscribe(Type messageType, IModel channel);

        public Task Execute()
        {
            foreach (var item in GetMessageTypes())
                Subscribe(item, channelPool.Get());

            return Task.CompletedTask;
        }

        private IEnumerable<Type> GetMessageTypes()
           => handlersStorage.EventCouples
               .ToList()
               .Select(x => x.Message)
               .Distinct()
               .Where(x => x.GetInterfaces().Any(y => y == typeof(T)));

        protected async void HandleMessage(IMessage message)
        {
            var middlewareContext = ActivatorUtilities.CreateInstance<MiddlewareContext>(
                    serviceProvider,
                    new object[] {
                        middlewaresStorage.SubscriberMiddlewares
                    });

            await middlewareContext.Next(message);
        }
    }
}
