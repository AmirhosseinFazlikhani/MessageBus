using MessageBus.Settings;
using Microsoft.Extensions.Configuration;
using System;

namespace MessageBus
{
    public interface IMessageBusBuilder
    {
        MessageBusSettings Settings { get; }

        IPublisherBuilder PublisherBuilder { get; }

        ISubscriberBuilder SubscriberBuilder { get; }

        IMessageBusBuilder ConfigurePublisher(Action<IPublisherBuilder> action);

        IMessageBusBuilder ConfigureSubscriber(Action<ISubscriberBuilder> action);

        IMessageBusBuilder ReadSettings(IConfiguration configuration);
    }
}
