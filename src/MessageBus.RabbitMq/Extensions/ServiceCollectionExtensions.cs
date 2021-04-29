﻿using MessageBus.RabbitMq.Concrete;
using MessageBus.RabbitMq.Messages;
using Microsoft.Extensions.DependencyInjection;
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

        public static IServiceCollection AddMessageBus(this IServiceCollection services, MessageBusSettings settings)
        {
            _eventModules = new List<EventModule>();
            _commandModules = new List<CommandModule>();

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

            services.AddSingleton(EventModules);
            services.AddSingleton(CommandModules);

            services.AddHostedService<EventSubscriber>();
            services.AddHostedService<CommandReceiver>();

            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddTransient<IMessageBus, Concrete.MessageBus>();

            return services;
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

        public static IServiceCollection AddComandHandler<TCommand, THandler>(this IServiceCollection services)
            where TCommand : Command
            where THandler : ICommandHandler<TCommand>
        {
            services.AddTransient(typeof(ICommandHandler<TCommand>), typeof(THandler));
            _commandModules.Add(new CommandModule { Command = typeof(TCommand) });

            return services;
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