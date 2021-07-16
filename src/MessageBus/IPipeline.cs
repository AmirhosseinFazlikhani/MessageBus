using System.Threading.Tasks;

namespace MessageBus
{
    public interface IPipeline
    {
        Task Start(IMessage message);
    }
}
