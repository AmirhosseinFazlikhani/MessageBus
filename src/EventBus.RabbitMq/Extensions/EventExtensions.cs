using System;
using System.Text.Json;

namespace EventBus.RabbitMq.Extensions
{
    internal static class EventExtensions
    {
        public static string GetExchange(this Type type)
        {
            return type.Name;
        }

		public static byte[] Serialize<T>(this T message)
		{
			return JsonSerializer.SerializeToUtf8Bytes(message);
		}

		public static object Deserialize(this ReadOnlyMemory<byte> content, Type type)
		{
			return JsonSerializer.Deserialize(content.ToArray(), type);
		}
	}
}
