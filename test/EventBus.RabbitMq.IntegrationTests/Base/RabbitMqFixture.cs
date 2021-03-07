using RabbitMQ.Client;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.RabbitMq.IntegrationTests.Base
{
	public class RabbitMqFixture : IAsyncLifetime
	{
		public IConnection Connection { get; private set; }

		public Task InitializeAsync()
		{
			var connectionFactory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			Connection = connectionFactory.CreateConnection();

			return Task.CompletedTask;
		}

		public Task DisposeAsync()
		{
			Connection.Close();
			Connection.Dispose();

			return Task.CompletedTask;
		}
	}
}
