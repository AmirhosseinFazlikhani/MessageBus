using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessageHandler<T> where T : IMessage
    {
        Task HandleAsync(T message);
    }
}
