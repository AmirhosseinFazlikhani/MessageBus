using System.Threading.Tasks;

namespace MessageBus.Middlewares.SafeReceiving
{
    public class SafeReceivingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(IMessage message, RequestDelegate next)
        {
            if (MessagesStorage.TryAdd(message))
                await next.Invoke(message);
        }
    }
}
