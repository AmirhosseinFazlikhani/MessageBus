using System;

namespace EventBus.RabbitMq
{
    public interface IEventSubscriber<T> where T : IntegrativeEvent
    {
        void Connect();

        void Receive(Action<T> action);

        void Disconnect();
    }
}
