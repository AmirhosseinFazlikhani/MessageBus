using MessageBus.Concretes;
using MessageBus.Settings;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Linq;

namespace MessageBus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly HandlersStorage handlersStorage = new HandlersStorage();

        public static IServiceCollection AddMessageBus(this IServiceCollection services, Action<IMessageBusBuilder> builder)
        {
            var builderInstance = new MessageBusBuilder();
            builder(builderInstance);

            services.AddSingleton((provider) => CreateConnection(builderInstance.Settings));

            services.AddSingleton(new PublisherMiddlewareStorage { Middlewares = builderInstance.PublisherBuilder.Middlewares });
            services.AddSingleton(new SubscriberMiddlewareStorage { Middlewares = builderInstance.SubscriberBuilder.Middlewares });

            services.AddSingleton(handlersStorage);

            foreach (var item in builderInstance.PublisherBuilder.Publishers.Concat(builderInstance.SubscriberBuilder.Subscribers))
                services.AddTransient(item.Key, item.Value);

            foreach (var item in builderInstance.PublisherBuilder.Middlewares.Concat(builderInstance.SubscriberBuilder.Middlewares).Distinct())
                services.AddTransient(item);

            services.AddHostedService(p =>
            {
                return new SubscribersActivator(builderInstance.SubscriberBuilder.Subscribers.Select(x => x.Key).GetEnumerator(), p);
            });

            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddTransient<IMessagePublisher, MessagePublisher>();
            services.AddTransient<IMiddlewareFactory, MiddlewareFactory>();
            services.AddTransient<IPipelineFactory, PipelineFactory>();

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
            handlersStorage.Pairs.Add(new Pair
            {
                Message = typeof(TMessage),
                Handler = typeof(THandler)
            });

            return services;
        }
    }
}
