using EventBus.RabbitMq.Abstractions;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Threading;

namespace EventBus.RabbitMq
{
	public class ChannelPool : IChannelPool
	{
		private readonly IConnection _connection;
		private readonly int _maxCount;
		private readonly ConcurrentBag<IModel> _channels;

		public ChannelPool(
			IConnection connection,
			int maxCount)
		{
			_channels = new ConcurrentBag<IModel>();
			_connection = connection;
			_maxCount = maxCount;
		}

		private int _counter = 0;

		private readonly object _lock = new object();

		public IModel Get()
		{
			lock (_lock)
			{
				IModel channel;
				if (_channels.TryTake(out channel)) return channel;

				if (_counter < _maxCount)
				{
					channel = _connection.CreateModel();
					_counter++;

					return channel;
				}

				while (!_channels.TryTake(out channel))
				{
					Thread.Sleep(10);
				}

				return channel;
			}
		}

		public void Release(IModel channel)
		{
			if (_channels.Count < _maxCount)
			{
				_channels.Add(channel);
				_counter++;
			}
		}
	}
}
