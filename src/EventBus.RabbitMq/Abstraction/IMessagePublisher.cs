using System.Threading.Tasks;

namespace EventBus.RabbitMq.Abstractions
{
	public interface IMessagePublisher
	{
		Task PublishAsync<T>(T @event) where T : IntegrativeEvent;
	}
}
