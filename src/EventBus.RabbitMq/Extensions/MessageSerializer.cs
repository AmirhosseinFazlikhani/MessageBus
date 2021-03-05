using System;
using System.Text;
using System.Text.Json;

namespace EventBus.RabbitMq.Extensions
{
	internal static class MessageSerializer
	{
		public static byte[] Serialize<T>(this T message)
		{
			var json = JsonSerializer.Serialize(message);
			var bytes = Encoding.ASCII.GetBytes(json);

			return bytes;
		}

		public static T Deserialize<T>(this ReadOnlyMemory<byte> content)
		{
			var body = content.ToArray();
			var message = Encoding.UTF8.GetString(body);
			var result = JsonSerializer.Deserialize<T>(message);

			return result;
		}
	}
}
