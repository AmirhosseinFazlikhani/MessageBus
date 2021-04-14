using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.RabbitMq
{
    public abstract class IntegrativeEventHandler<T> : BackgroundService where T : IntegrativeEvent
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventSubscriber<T> _subscriber;

        protected IntegrativeEventHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _subscriber = scopeFactory.CreateScope().ServiceProvider.GetService<IEventSubscriber<T>>();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriber.Connect();

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriber.Receive((T @event) =>
            {
                Task.Run(() =>
                {
                    var scope = _scopeFactory.CreateScope();
                    Initialize(scope.ServiceProvider);
                    Handle(@event);
                    scope.Dispose();
                });
            });

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _subscriber.Disconnect();

            return base.StopAsync(cancellationToken);
        }

        protected abstract void Initialize(IServiceProvider provider);

        public abstract void Handle(T @event);
    }
}
