using EventBus.RabbitMq.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace EventBus.RabbitMq
{
	public class ChannelPool : IChannelPool
	{
		private readonly IConnection _connection;
		private readonly ILogger<ChannelPool> _logger;
		private readonly int _maxCount;
		private readonly ConcurrentBag<IModel> _channels;

		public ChannelPool(
			IConnection connection,
			ILogger<ChannelPool> logger,
			int maxCount)
		{
			_logger = logger;
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

				if (_channels.TryTake(out channel))
				{
					_logger.LogTrace("[EventBus] Channel {Number} took.", channel.ChannelNumber);

					return channel;
				}

				if (_counter < _maxCount)
				{
					channel = _connection.CreateModel();
					_counter++;

					_logger.LogDebug("[EventBus] Channel {Number} created.", channel.ChannelNumber);
					_logger.LogTrace("[EventBus] Channel {Number} took.", channel.ChannelNumber);

					return channel;
				}

				var watch = new Stopwatch();
				watch.Start();

				while (!_channels.TryTake(out channel))
				{
					Thread.Sleep(5);
				}

				watch.Stop();

				_logger.LogWarning("[EventBus] Channel {Number} took after {TotalTime}", channel.ChannelNumber, watch.Elapsed);

				return channel;
			}
		}

		public void Release(IModel channel)
		{
			if (_channels.Count < _maxCount)
			{
				_channels.Add(channel);
				_counter++;

				_logger.LogTrace("[EventBus] Channel {Number} released.", channel.ChannelNumber);
			}
			else
			{
				_logger.LogWarning("[EventBus] An attempt was made to release channel {Number} while the number of free channels was maximum.", channel.ChannelNumber);
			}
		}
	}
}
