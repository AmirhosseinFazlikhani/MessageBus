using EventBus.RabbitMq.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus.RabbitMq.Extensions
{
	public static class RegisterServices
	{
		private static List<Item> _items;

		public static IServiceCollection AddEventBus(this IServiceCollection services, MessagingConfig config)
		{
			_items = new List<Item>();

			var connectionFactory = new ConnectionFactory
			{
				HostName = config.HostName,
				UserName = config.UserName,
				Password = config.Password
			};

			var connection = connectionFactory.CreateConnection();

			services.AddSingleton(connection);

			services.AddSingleton(new ChannelPool(connection, 10));
			services.AddTransient<IMessagePublisher, MessagePublisher>();
			services.AddTransient<IMessageSubscriber, MessageSubscriber>();

			return services;
		}

		public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
			where TEvent : IntegrativeEvent
			where THandler : BaseEventHandler<TEvent>
		{
			services.AddHostedService<THandler>();

			if (!_items.Any(x => x.Handler.Equals(typeof(THandler))))
			{
				_items.Add(new Item
				{
					Event = typeof(TEvent),
					Handler = typeof(THandler)
				});
			}

			return services;
		}

		public static IEnumerable<Type> GetEvents(this IServiceProvider provider)
		{
			return _items.Select(x => x.Event);
		}

		public static IEnumerable<Type> GetEventHandlers(this IServiceProvider provider, Type @event)
		{
			return GetEventHandlers(@event);
		}

		public static IEnumerable<Type> GetEventHandlers<TEvent>(this IServiceProvider provider) where TEvent : IntegrativeEvent
		{
			return GetEventHandlers(typeof(TEvent));
		}

		private static IEnumerable<Type> GetEventHandlers(Type @event)
		{
			return _items.Where(x => x.Event.Equals(@event)).Select(x => x.Handler);
		}

		private class Item
		{
			public Type Event { get; set; }

			public Type Handler { get; set; }
		}
	}
}
