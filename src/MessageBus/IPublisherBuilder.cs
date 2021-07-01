using System;

namespace MessageBus
{
    public interface IPublisherBuilder
    {
        Type[] Middlewares { get; }

        IPublisherBuilder UseMiddleware<T>() where T : IMiddleware;

        IPublisherBuilder UsePublisher<TMessage, TPublisher>()
            where TMessage : IMessage
            where TPublisher : IPublisher<TMessage>;
    }
}
