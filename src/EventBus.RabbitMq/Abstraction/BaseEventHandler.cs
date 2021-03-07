using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.RabbitMq.Abstractions
{
	public abstract class BaseEventHandler<T> : BackgroundService where T : IntegrativeEvent
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IMessageSubscriber<T> _subscriber;

		protected BaseEventHandler(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
			_subscriber = scopeFactory.CreateScope().ServiceProvider.GetService<IMessageSubscriber<T>>();
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_subscriber.Connect();

			return Task.CompletedTask;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_subscriber.Received((T @event) =>
			{
				GetServices(_scopeFactory.CreateScope().ServiceProvider);
				Handle(@event);
			});

			return Task.CompletedTask;
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_subscriber.Disconnect();

			return Task.CompletedTask;
		}

		protected abstract void GetServices(IServiceProvider provider);

		public abstract void Handle(T @event);
	}
}
