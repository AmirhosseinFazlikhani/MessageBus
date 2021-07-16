using System;
using System.Collections.Generic;

namespace MessageBus.Concretes
{
    public class SubscriberMiddlewareStorage : IMiddlewareStorage
    {
        public IEnumerable<Type> Middlewares { get; init; }
    }
}
