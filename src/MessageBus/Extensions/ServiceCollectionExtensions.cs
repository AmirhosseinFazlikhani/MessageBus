using MessageBus.Concretes;
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

            services.AddHostedService(p =>
            {
                return new SubscribersActivator(builderInstance.SubscriberBuilder.Subscribers, p);
            });

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

        public static IServiceCollection AddMessageHandler<TMessage, THandler>(this IServiceCollection services)
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            handlersStorage.Pairs.Add(typeof(TMessage), typeof(THandler));
            return services;
        }
    }
}
