using MessageBus.Settings;
using Microsoft.Extensions.Configuration;
using System;

namespace MessageBus.Concretes
{
    internal class MessageBusBuilder : IMessageBusBuilder
    {
        public MessageBusSettings Settings { get; private set; }

        public PublisherBuilder PublisherBuilder { get; } = new PublisherBuilder();

        public SubscriberBuilder SubscriberBuilder { get; } = new SubscriberBuilder();

        public IMessageBusBuilder ReadSettings(IConfiguration configuration)
        {
            Settings = configuration.GetSection("MessageBus").Get<MessageBusSettings>();
            return this;
        }

        public IMessageBusBuilder ConfigurePublisher(Action<IPublisherBuilder> builder)
        {
            builder(PublisherBuilder);
            return this;
        }

        public IMessageBusBuilder ConfigureSubscriber(Action<ISubscriberBuilder> builder)
        {
            builder(SubscriberBuilder);
            return this;
        }
    }
}
