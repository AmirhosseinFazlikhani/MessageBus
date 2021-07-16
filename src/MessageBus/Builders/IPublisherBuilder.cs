using System;
using System.Collections.Generic;

namespace MessageBus
{
    public interface IPublisherBuilder
    {
        IEnumerable<Type> Middlewares { get; }

        IReadOnlyDictionary<Type, Type> Publishers { get; }

        IPublisherBuilder UseMiddleware<T>() where T : IMiddleware;

        IPublisherBuilder UsePublisher<TMessage, TPublisher>()
            where TMessage : IMessage
            where TPublisher : IPublisher<TMessage>;
    }
}
