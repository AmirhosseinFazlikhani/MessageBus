using System;
using System.Collections.Generic;

namespace MessageBus.Concretes
{
    public class PublisherMiddlewareStorage : IMiddlewareStorage
    {
        public IEnumerable<Type> Middlewares { get; init; }
    }
}
