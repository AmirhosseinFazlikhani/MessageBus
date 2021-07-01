using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Concretes
{
    public class PublisherBuilder : IPublisherBuilder
    {
        private readonly IList<Type> middlewares = new List<Type>();
        public Type[] Middlewares => middlewares.ToArray();

        private readonly IDictionary<Type, Type> publishers = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Publishers => publishers.ToDictionary(k => k.Key, v => v.Value);

        public IPublisherBuilder UseMiddleware<T>() where T : IMiddleware
        {
            middlewares.Add(typeof(T));
            return this;
        }

        public IPublisherBuilder UsePublisher<TMessage, TPublisher>()
            where TMessage : IMessage
            where TPublisher : IPublisher<TMessage>
        {
            publishers.Add(typeof(IPublisher<TMessage>), typeof(TPublisher));
            return this;
        }
    }
}
