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
        private readonly IEnumerator<Type> subsribers;
        private readonly IServiceProvider serviceProvider;

        public SubscribersActivator(
            IEnumerator<Type> subsribers,
            IServiceProvider serviceProvider)
        {
            this.subsribers = subsribers;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (subsribers.MoveNext())
            {
                var instance = (dynamic)serviceProvider.GetRequiredService(subsribers.Current);
                await instance.Execute();
            }
        }
    }
}
