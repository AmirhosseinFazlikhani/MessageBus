using MessageBus.RabbitMq.Messages;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq
{
    public interface IEventHandler<T> where T : IntegrativeEvent
    {
        Task HandleAsync(T @event);
    }
}
