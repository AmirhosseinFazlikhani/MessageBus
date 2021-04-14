namespace EventBus.RabbitMq
{
    public class EventBusSettings
    {
        public string HostName { get; set; }

        public int? Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int? MaxChannels { get; set; }
    }
}
