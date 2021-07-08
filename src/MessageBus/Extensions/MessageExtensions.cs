using System;
using System.Text.Json;

namespace MessageBus.Extensions
{
    internal static class MessageExtensions
    {
        public static string GetEventExchange(this Type type)
        {
            return type.Name;
        }

        public static string GetCommandQueue(this Type type)
        {
            return type.Name;
        }

        public static byte[] Serialize<T>(this T message) where T : IMessage
        {
            return JsonSerializer.SerializeToUtf8Bytes(message, message.GetType());
        }

        public static object Deserialize(this ReadOnlyMemory<byte> content, Type type)
        {
            return JsonSerializer.Deserialize(content.ToArray(), type);
        }
    }
}
