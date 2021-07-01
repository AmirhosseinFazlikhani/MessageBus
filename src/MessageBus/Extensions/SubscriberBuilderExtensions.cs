using MessageBus.Concretes.Middlewars;

namespace MessageBus.Extensions
{
    public static class SubscriberBuilderExtensions
    {
        public static ISubscriberBuilder UseRouting(this ISubscriberBuilder builder)
        {
            builder.UseMiddleware<SubscriberRouterMiddleware>();
            return builder;
        }
    }
}
