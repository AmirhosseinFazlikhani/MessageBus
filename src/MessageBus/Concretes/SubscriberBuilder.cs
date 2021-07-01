using System;
using System.Collections.Generic;

namespace MessageBus.Concretes
{
    public class SubscriberBuilder : ISubscriberBuilder
    {
        private readonly List<Type> middlewares = new List<Type>();
        public Type[] Middlewares => middlewares.ToArray();

        public ISubscriberBuilder UseMiddleware<T>() where T : IMiddleware
        {
            middlewares.Add(typeof(T));
            return this;
        }
    }
}
