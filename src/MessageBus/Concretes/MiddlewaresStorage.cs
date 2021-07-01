using System;

namespace MessageBus.Concretes
{
    internal class MiddlewaresStorage
    {
        public Type[] PublisherMiddlewares { get; }

        public Type[] SubscriberMiddlewares { get; }

        public MiddlewaresStorage(
            Type[] publisherMiddlewares,
            Type[] subscriberMiddlewares)
        {
            PublisherMiddlewares = publisherMiddlewares;
            SubscriberMiddlewares = subscriberMiddlewares;
        }
    }
}
