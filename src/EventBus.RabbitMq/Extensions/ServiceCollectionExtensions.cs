using EventBus.RabbitMq.Concrete;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace EventBus.RabbitMq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static List<RegisteredCouples> Couples { get; set; }

        public static IServiceCollection AddEventBus(this IServiceCollection services, EventBusSettings settings)
        {
            Couples = new List<RegisteredCouples>();

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

            services.AddSingleton(Couples);

            services.AddHostedService<EventSubscriber>();

            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddTransient<IEventPublisher, EventPublisher>();

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IntegrativeEvent
            where THandler : IEventHandler<TEvent>
        {
            Couples.Add(new RegisteredCouples
            {
                Event = typeof(TEvent),
                Handler = typeof(THandler)
            });

            return services;
        }

        internal class RegisteredCouples
        {
            public Type Event { get; set; }

            public Type Handler { get; set; }
        }
    }
}
