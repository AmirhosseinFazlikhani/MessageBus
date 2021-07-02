using System.Threading.Tasks;

namespace MessageBus
{
    public interface IPublisher<T>
    {
        Task PublishAsync(T message);
    }
}
