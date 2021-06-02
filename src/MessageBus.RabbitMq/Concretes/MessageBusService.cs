using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.RabbitMq.Concretes
{
    internal class MessageBusService : IMessageBusService
    {
        public MessageBusSettings Settings { get; }

        public IServiceCollection Services { get; }

        public MessageBusService(MessageBusSettings settings, IServiceCollection services)
        {
            Settings = settings;
            Services = services;
        }
    }
}
