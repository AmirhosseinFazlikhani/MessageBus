using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.RabbitMq
{
    public interface IMessageBusService
    {
        IServiceCollection Services { get; }

        MessageBusSettings Settings { get; }
    }
}
