using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessagePublisher
    {
        Task PublishAsync(IMessage message);
    }
}
