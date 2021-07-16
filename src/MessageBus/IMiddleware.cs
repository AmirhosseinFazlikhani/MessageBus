using MessageBus.Concretes;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMiddleware
    {
        Task InvokeAsync(IMessage message, RequestDelegate next);
    }
}
