using System.Threading.Tasks;

namespace MessageBus
{
    public interface ICommandHandler<T> where T : ICommand
    {
        Task HandleAsync(T command);
    }
}
