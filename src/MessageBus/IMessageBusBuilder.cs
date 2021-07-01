using MessageBus.Concretes;
using MessageBus.Settings;
using Microsoft.Extensions.Configuration;
using System;

namespace MessageBus
{
    public interface IMessageBusBuilder
    {
        MessageBusSettings Settings { get; }

        PublisherBuilder PublisherBuilder { get; }

        SubscriberBuilder SubscriberBuilder { get; }

        IMessageBusBuilder ConfigurePublisher(Action<IPublisherBuilder> action);

        IMessageBusBuilder ConfigureSubscriber(Action<ISubscriberBuilder> action);

        IMessageBusBuilder ReadSettings(IConfiguration configuration);
    }
}
