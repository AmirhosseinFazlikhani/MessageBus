using System;
using System.Text.Json.Serialization;

namespace MessageBus.RabbitMq.Messages
{
    public abstract class Message
    {
        public Guid Id { get; private set; }

        public DateTime CreateDateTime { get; private set; }

        public Message()
        {
            Id = Guid.NewGuid();
            CreateDateTime = DateTime.UtcNow;
        }

        [JsonConstructor]
        public Message(Guid id, DateTime createDateTime)
        {
            Id = id;
            CreateDateTime = createDateTime;
        }
    }
}
