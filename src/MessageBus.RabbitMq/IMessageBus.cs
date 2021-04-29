using MessageBus.RabbitMq.Messages;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T @event) where T : IntegrativeEvent;

        Task SendAsync<T>(T command) where T : Command;
    }
}
