using EventBus.RabbitMq.Concrete;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus.RabbitMq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static List<Item> _items;

        public static IServiceCollection AddEventBus(this IServiceCollection services, EventBusSettings settings)
        {
            _items = new List<Item>();

            services.AddSingleton(settings);

            var connectionFactory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password
            };

            if (settings.Port.HasValue)
            {
                connectionFactory.Port = settings.Port.Value;
            }

            var connection = connectionFactory.CreateConnection();

            services.AddSingleton(connection);

            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddTransient<IEventPublisher, EventPublisher>();
            services.AddScoped(typeof(IEventSubscriber<>), typeof(EventSubscriber<>));

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IntegrativeEvent
            where THandler : IntegrativeEventHandler<TEvent>
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
