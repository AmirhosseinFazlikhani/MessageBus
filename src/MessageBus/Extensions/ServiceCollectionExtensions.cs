using MessageBus.Concretes;
using MessageBus.Models;
using MessageBus.Settings;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;

namespace MessageBus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly HandlersStorage handlersStorage = new HandlersStorage();

        public static IServiceCollection AddMessageBus(this IServiceCollection services, Action<IMessageBusBuilder> builder)
        {
            var builderInstance = new MessageBusBuilder();
            builder(builderInstance);

            var connection = CreateConnection(builderInstance.Settings);
            services.AddSingleton(connection);
            services.AddSingleton(new MiddlewaresStorage(builderInstance.PublisherBuilder.Middlewares, builderInstance.SubscriberBuilder.Middlewares));
            services.AddSingleton(handlersStorage);

            foreach (var item in builderInstance.PublisherBuilder.Publishers)
                services.AddTransient(item.Key, item.Value);

            foreach (var item in builderInstance.SubscriberBuilder.Subscribers)
                services.AddTransient(item.Key, item.Value);

            services.AddHostedService<SubscribersActivator>();
            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddScoped<IMessagePublisher, MessagePublisher>();

            return services;
        }

        private static IConnection CreateConnection(MessageBusSettings settings)
        {
            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password,
                Port = settings.Port,
                AutomaticRecoveryEnabled = settings.AutomaticRecovery,
                TopologyRecoveryEnabled = settings.AutomaticRecovery,
                NetworkRecoveryInterval = settings.RecoveryInterval
            };

            return factory.CreateConnection();
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            handlersStorage.EventCouples.Add(new MessageCouple
            {
                Message = typeof(TEvent),
                Handler = typeof(THandler)
            });

            return services;
        }

        public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
            where TCommand : ICommand
            where THandler : ICommandHandler<TCommand>
        {
            handlersStorage.CommandCouples.Add(new MessageCouple
            {
                Message = typeof(TCommand),
                Handler = typeof(THandler)
            });

            return services;
        }
    }
}
