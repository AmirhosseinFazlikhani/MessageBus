using MessageBus.RabbitMq.Concretes;
using MessageBus.RabbitMq.Messages;
using MessageBus.RabbitMq.Modules.Storage;
using MessageBus.RabbitMq.Modules.Storage.Concretes;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace MessageBus.RabbitMq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static List<EventModule> _eventModules;

        private static IReadOnlyCollection<EventModule> EventModules => _eventModules;

        private static List<CommandModule> _commandModules;

        private static IReadOnlyCollection<CommandModule> CommandModules => _commandModules;

        public static IMessageBusService AddMessageBus(this IServiceCollection services, MessageBusSettings settings)
        {
            _eventModules = new List<EventModule>();
            _commandModules = new List<CommandModule>();

            services.AddSingleton(settings);

            var connectionFactory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password,
                Port = settings.Port
            };

            var connection = connectionFactory.CreateConnection();

            services.AddSingleton(connection);

            services.AddSingleton(EventModules);
            services.AddSingleton(CommandModules);

            services.AddHostedService<EventSubscriber>();
            services.AddHostedService<CommandReceiver>();

            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddTransient<IMessageBus, Concretes.MessageBus>();

            return new MessageBusService(settings, services);
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IntegrativeEvent
            where THandler : IEventHandler<TEvent>
        {
            _eventModules.Add(new EventModule
            {
                Event = typeof(TEvent),
                Handler = typeof(THandler)
            });

            return services;
        }

        public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
            where TCommand : Command
            where THandler : ICommandHandler<TCommand>
        {
            services.AddTransient(typeof(ICommandHandler<TCommand>), typeof(THandler));
            _commandModules.Add(new CommandModule { Command = typeof(TCommand) });

            return services;
        }

        public static IMessageBusService AddElasticsearch(this IMessageBusService messageBus)
        {
            var node = new Uri(messageBus.Settings.Elasticsearch.Node);
            var connection = new ConnectionSettings(node).BasicAuthentication(
                messageBus.Settings.Elasticsearch.User,
                messageBus.Settings.Elasticsearch.Password);

            var client = new ElasticClient(connection);

            messageBus.Services.AddSingleton<IElasticClient>(client);
            messageBus.Services.AddTransient<IMessageStorage, ElasticStorage>();

            return messageBus;
        }
    }

    internal class EventModule
    {
        public Type Event { get; set; }

        public Type Handler { get; set; }
    }

    internal class CommandModule
    {
        public Type Command { get; set; }
    }
}
