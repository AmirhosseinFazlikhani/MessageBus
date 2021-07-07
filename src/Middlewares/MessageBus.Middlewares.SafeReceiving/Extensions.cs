namespace MessageBus.Middlewares.SafeReceiving
{
    public static class Extensions
    {
        public static ISubscriberBuilder UseSafeReceiving(this ISubscriberBuilder builder)
        {
            builder.UseMiddleware<SafeReceivingMiddleware>();
            MessagesStorage.Initialize();

            return builder;
        }
    }
}
