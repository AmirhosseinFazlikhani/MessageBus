using MessageBus.RabbitMq.Messages;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq
{
    public interface ICommandHandler<T> where T : Command
    {
        Task HandleAsync(T command);
    }
}
