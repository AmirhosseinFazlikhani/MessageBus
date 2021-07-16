using System;

namespace MessageBus
{
    public interface IMiddlewareFactory
    {
        public IMiddleware Create(Type type);
    }
}
