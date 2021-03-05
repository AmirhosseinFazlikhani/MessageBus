using EventBus.RabbitMq.Abstractions;
using System;

namespace EventBus.RabbitMq.Extensions
{
	internal static class EventExtensions
	{
		public static string GetRoutingKey<T>(this T @event) where T : IntegrativeEvent
		{
			return typeof(T).Name;
		}

		public static string GetRoutingKey(this Type type)
		{
			return type.Name;
		}
	}
}
