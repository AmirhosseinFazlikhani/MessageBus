using RabbitMQ.Client;

namespace EventBus.RabbitMq
{
    internal interface IChannelPool
    {
        void Release(IModel channel);
        IModel Get();
    }
}
