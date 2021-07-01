namespace MessageBus
{
    public interface ISubscriber<T> where T : IMessage
    {
        void Subscribe(T message);
    }
}
