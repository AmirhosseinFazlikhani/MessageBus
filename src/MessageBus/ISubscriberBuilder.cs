using System;

namespace MessageBus
{
    public interface ISubscriberBuilder
    {
        Type[] Middlewares { get; }

        ISubscriberBuilder UseMiddleware<T>() where T : IMiddleware;

        ISubscriberBuilder UseSubscriber<TMessage, TSubscriber>()
            where TMessage : IMessage
            where TSubscriber : Subscriber<TMessage>;
    }
}
