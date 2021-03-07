using EventBus.RabbitMq.Abstractions;
using System;

namespace EventBus.RabbitMq.Extensions
{
	internal static class EventExtensions
	{
		public static string GetExchange(this Type type)
		{
			return type.Name;
		}
	}
}
