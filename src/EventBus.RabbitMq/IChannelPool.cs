using RabbitMQ.Client;

namespace EventBus.RabbitMq
{
    public interface IChannelPool
    {
        void Release(IModel channel);
        IModel Get();
    }
}
