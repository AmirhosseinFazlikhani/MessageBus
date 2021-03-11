namespace EventBus.RabbitMq
{
	public class MessagingConfig
	{
		public string HostName { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public int MaxChannels { get; set; } = 10;
	}
}
