using System;

namespace EventBus.RabbitMq.Abstractions
{
	public interface IMessageSubscriber
	{
		void Connect<T>() where T : IntegrativeEvent;

		void Connect(Type type);

		void Disconnect();

		void Received<T>(Action<T> action) where T : IntegrativeEvent;
	}
}
