using System;
using System.Text.Json.Serialization;

namespace EventBus.RabbitMq
{
    public class IntegrativeEvent
    {
        public Guid Id { get; set; }

        public DateTime CreateDateTime { get; set; }

        public IntegrativeEvent()
        {
            Id = Guid.NewGuid();
            CreateDateTime = DateTime.UtcNow;
        }

        [JsonConstructor]
        public IntegrativeEvent(Guid id, DateTime createDateTime)
        {
            Id = id;
            CreateDateTime = createDateTime;
        }
    }
}
