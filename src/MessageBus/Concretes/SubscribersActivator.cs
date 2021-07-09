using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Concretes
{
    public class SubscribersActivator : BackgroundService
    {
        private readonly IReadOnlyDictionary<Type, Type> subsribers;
        private readonly IServiceProvider serviceProvider;

        public SubscribersActivator(
            IReadOnlyDictionary<Type, Type> subsribers,
            IServiceProvider serviceProvider)
        {
            this.subsribers = subsribers;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var item in subsribers)
            {
                var instance = (dynamic)ActivatorUtilities.CreateInstance(serviceProvider, item.Value);
                await instance.Execute();
            }
        }
    }
}
