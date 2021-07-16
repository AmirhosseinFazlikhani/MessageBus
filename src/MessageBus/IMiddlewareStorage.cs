using System;
using System.Collections.Generic;

namespace MessageBus
{
    public interface IMiddlewareStorage
    {
        IEnumerable<Type> Middlewares { get; }
    }
}
