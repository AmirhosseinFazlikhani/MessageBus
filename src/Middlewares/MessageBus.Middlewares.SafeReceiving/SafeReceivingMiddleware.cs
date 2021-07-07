using System.Threading.Tasks;

namespace MessageBus.Middlewares.SafeReceiving
{
    public class SafeReceivingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(IMessage message, IMiddlewareContext context)
        {
            if (MessagesStorage.TryAdd(message))
                await context.Next(message);
        }
    }
}
