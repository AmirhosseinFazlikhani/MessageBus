using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.RabbitMq.UnitTests
{
	public class ChannelPoolTest
	{
		private readonly IConnection _connection;
		private readonly NullLogger<ChannelPool> _logger;

		public ChannelPoolTest()
		{
			_logger = new NullLogger<ChannelPool>();

			var mockedConnection = new Mock<IConnection>();
			mockedConnection.Setup(x => x.CreateModel()).Returns(new Mock<IModel>().Object);

			_connection = mockedConnection.Object;
		}

		[Fact]
		public void Get_WhenChannelsCountIsMaximum_DoNotCreateNewChannel()
		{
			var pool = new ChannelPool(_connection, _logger, new MessagingConfig { MaxChannels = 1 });

			var channel1 = pool.Get();
			IModel channel2 = null;
			Task.Run(() => channel2 = pool.Get());

			Assert.NotNull(channel1);
			Assert.Null(channel2);
		}

		[Fact]
		public void Get_WhenNotExistsFreeChannel_WaitUtilAChannelReleased()
		{
			var pool = new ChannelPool(_connection, _logger, new MessagingConfig { MaxChannels = 1 });

			var channel1 = pool.Get();
			IModel channel2 = null;

			Task.Run(() => channel2 = pool.Get());
			pool.Release(channel1);

			Thread.Sleep(10);
			Assert.NotNull(channel2);
		}

		[Fact]
		public void Get_WhenNotExistsFreeChannelAndMoreThanOneRequestCome_QueuedRequestsAndWaitUnitChannelsReleased()
		{
			var pool = new ChannelPool(_connection, _logger, new MessagingConfig { MaxChannels = 1 });

			var channel1 = pool.Get();
			IModel channel2 = null;
			IModel channel3 = null;
			IModel channel4 = null;

			Task.Run(() => channel2 = pool.Get());
			Thread.Sleep(100);
			Task.Run(() => channel3 = pool.Get());
			Thread.Sleep(100);
			Task.Run(() => channel4 = pool.Get());

			pool.Release(channel1);
			Thread.Sleep(50);
			Assert.NotNull(channel2);
			Assert.Null(channel3);
			Assert.Null(channel4);

			pool.Release(channel2);
			Thread.Sleep(50);
			Assert.NotNull(channel3);
			Assert.Null(channel4);

			pool.Release(channel3);
			Thread.Sleep(50);
			Assert.NotNull(channel4);
		}
	}
}
