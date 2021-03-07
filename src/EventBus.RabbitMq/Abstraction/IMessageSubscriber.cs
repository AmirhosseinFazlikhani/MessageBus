using System;

namespace EventBus.RabbitMq.Abstractions
{
	public interface IMessageSubscriber<T> where T : IntegrativeEvent
	{
		void Connect();

		void Disconnect();

		void Received(Action<T> action);
	}
}
