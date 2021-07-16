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
        private readonly IServiceProvider serviceProvider;

        protected Subscriber(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected abstract void Subscribe(Type messageType, IModel channel);

        public Task Execute()
        {
            var channelPool = serviceProvider.GetRequiredService<IChannelPool>();

            foreach (var item in GetMessageTypes())
                Subscribe(item, channelPool.Get());

            return Task.CompletedTask;
        }

        private IEnumerable<Type> GetMessageTypes()
        {
            var storage = serviceProvider.GetRequiredService<HandlersStorage>();
            return storage.Pairs
                .Select(x => x.Message)
                .Distinct()
                .Where(x => x.GetInterfaces().Any(y => y == typeof(T)));
        }

        protected async void HandleMessage(IMessage message)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var pipelineFactory = scope.ServiceProvider.GetRequiredService<IPipelineFactory>();
                var pipeline = pipelineFactory.CreateSubscriberPipeline();
                await pipeline.Start(message);
            }
        }
    }
}
