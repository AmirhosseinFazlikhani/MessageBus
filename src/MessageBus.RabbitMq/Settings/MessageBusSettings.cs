namespace MessageBus.RabbitMq
{
    public class MessageBusSettings
    {
        public string HostName { get; set; }

        public int Port { get; set; } = 5672;

        public string UserName { get; set; }

        public string Password { get; set; }

        public int MaxConcurrentChannels { get; set; } = 10;

        public string Application { get; set; }

        public ElasticserachSettings Elasticsearch { get; set; }
    }

    public class ElasticserachSettings
    {
        public string Node { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Index { get; set; } = "messagebus";
    }
}
