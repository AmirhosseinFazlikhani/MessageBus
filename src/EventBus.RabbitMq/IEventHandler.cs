using System.Threading.Tasks;

namespace EventBus.RabbitMq
{
    public interface IEventHandler<T> where T : IntegrativeEvent
    {
        Task HandleAsync(T @event);
    }
}
