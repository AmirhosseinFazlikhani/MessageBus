using RabbitMQ.Client;

namespace MessageBus.RabbitMq
{
    internal interface IChannelPool
    {
        void Release(IModel channel);
        IModel Get();
    }
}
