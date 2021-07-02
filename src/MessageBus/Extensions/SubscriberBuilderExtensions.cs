using MessageBus.Concretes.Middlewars;
using MessageBus.Concretes.Subscribers;

namespace MessageBus.Extensions
{
    public static class SubscriberBuilderExtensions
    {
        public static ISubscriberBuilder UseRouting(this ISubscriberBuilder builder)
        {
            builder.UseMiddleware<SubscriberRouterMiddleware>();
            return builder;
        }

        public static ISubscriberBuilder UseEventSubscriber(this ISubscriberBuilder builder)
        {
            builder.UseSubscriber<IEvent, EventSubscriber>();
            return builder;
        }

        public static ISubscriberBuilder UseCommandSubscriber(this ISubscriberBuilder builder)
        {
            builder.UseSubscriber<ICommand, CommandSubscriber>();
            return builder;
        }
    }
}
