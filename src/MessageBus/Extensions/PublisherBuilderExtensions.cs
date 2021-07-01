using MessageBus.Concretes.Middlewars;
using MessageBus.Concretes.Publishers;

namespace MessageBus.Extensions
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder UseRouting(this IPublisherBuilder builder)
        {
            builder.UseMiddleware<PublisherRouterMiddleware>();
            return builder;
        }

        public static IPublisherBuilder UseEventPublisher(this IPublisherBuilder builder)
        {
            builder.UsePublisher<IEvent, EventPublisher>();
            return builder;
        }

        public static IPublisherBuilder UseCommandPublisher(this IPublisherBuilder builder)
        {
            builder.UsePublisher<ICommand, CommandPublisher>();
            return builder;
        }
    }
}
