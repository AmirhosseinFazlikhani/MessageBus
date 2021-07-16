using System;
using System.Collections.Generic;

namespace MessageBus
{
    public interface ISubscriberBuilder
    {
        IEnumerable<Type> Middlewares { get; }

        IReadOnlyDictionary<Type, Type> Subscribers { get; }

        ISubscriberBuilder UseMiddleware<T>() where T : IMiddleware;

        ISubscriberBuilder UseSubscriber<TMessage, TSubscriber>()
            where TMessage : IMessage
            where TSubscriber : Subscriber<TMessage>;
    }
}
