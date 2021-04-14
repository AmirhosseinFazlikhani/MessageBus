using System.Threading.Tasks;

namespace EventBus.RabbitMq
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : IntegrativeEvent;
    }
}
