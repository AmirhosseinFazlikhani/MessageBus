using RabbitMQ.Client;

namespace MessageBus
{
    public interface IChannelPool
    {
        IModel Get();

        void Release(IModel channel);
    }
}
