using System.Threading.Tasks;

namespace MessageBus
{
    public interface IEventHandler<T> where T : IEvent
    {
        Task HandleAsync(T @event);
    }
}
