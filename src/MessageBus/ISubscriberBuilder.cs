using System;

namespace MessageBus
{
    public interface ISubscriberBuilder
    {
        Type[] Middlewares { get; }

        ISubscriberBuilder UseMiddleware<T>() where T : IMiddleware;
    }
}
