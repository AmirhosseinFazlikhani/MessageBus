using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace MessageBus.Concretes
{
    internal class ChannelPool : IChannelPool
    {
        private readonly ConcurrentBag<IModel> channels;
        private readonly IConnection connection;
        private readonly ILogger<ChannelPool> logger;

        public ChannelPool(
            IConnection connection,
            ILogger<ChannelPool> logger)
        {
            channels = new ConcurrentBag<IModel>();
            this.connection = connection;
            this.logger = logger;
        }

        public IModel Get()
        {
            IModel channel;

            if (channels.TryTake(out channel))
            {
                logger.LogTrace(
                    "Channel {ChannelNumber} was took.", channel.ChannelNumber);

                return channel;
            }

            channel = connection.CreateModel();

            logger.LogTrace(
                "Channel {ChannelNumber} was created.", channel.ChannelNumber);

            return channel;
        }

        public void Release(IModel channel)
        {
            channels.Add(channel);
            
            logger.LogTrace(
                "Channel {ChannelNumber} was released.", channel.ChannelNumber);
        }
    }
}
