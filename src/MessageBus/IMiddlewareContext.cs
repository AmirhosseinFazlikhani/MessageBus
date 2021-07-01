using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMiddlewareContext
    {
        Task Next(IMessage message);
    }
}
