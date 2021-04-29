namespace MessageBus.RabbitMq
{
    public class MessageBusSettings
    {
        public string HostName { get; set; }

        public int Port { get; set; } = 5672;

        public string UserName { get; set; }

        public string Password { get; set; }

        public int MaxConcurrentChannels { get; set; } = 10;
    }
}
