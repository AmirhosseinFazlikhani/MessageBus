using RabbitMQ.Client;

namespace EventBus.RabbitMq.Abstractions
{
	public interface IChannelPool
	{
		void Release(IModel channel);

		IModel Get();
	}
}
