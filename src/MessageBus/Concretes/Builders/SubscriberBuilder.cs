using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Concretes
{
    public class SubscriberBuilder : ISubscriberBuilder
    {
        private readonly List<Type> middlewares = new List<Type>();
        public IEnumerable<Type> Middlewares => middlewares;

        private readonly IDictionary<Type, Type> subscribers = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Subscribers => subscribers.ToDictionary(k => k.Key, v => v.Value);

        public ISubscriberBuilder UseMiddleware<T>() where T : IMiddleware
        {
            middlewares.Add(typeof(T));
            return this;
        }

        public ISubscriberBuilder UseSubscriber<TMessage, TSubscriber>()
            where TMessage : IMessage
            where TSubscriber : Subscriber<TMessage>
        {
            subscribers.Add(typeof(Subscriber<TMessage>), typeof(TSubscriber));
            return this;
        }
    }
}
