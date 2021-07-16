using System.Threading.Tasks;

namespace MessageBus
{
    public delegate Task RequestDelegate(IMessage message);
}
