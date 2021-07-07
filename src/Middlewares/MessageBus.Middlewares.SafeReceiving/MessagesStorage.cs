using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Middlewares.SafeReceiving
{
    internal static class MessagesStorage
    {
        private static ConcurrentQueue<int> queue;

        internal static void Initialize()
        {
            queue = new ConcurrentQueue<int>();
        }

        public static bool TryAdd(IMessage message)
        {
            if (queue.Contains(message.GetHashCode()))
                return false;

            queue.Enqueue(message.GetHashCode());

            Task.Run(() =>
            {
                Thread.Sleep(3000);
                queue.TryDequeue(out _);
            });

            return true;
        }
    }
}
