using MessageBus.RabbitMq.Modules.Storage.Enums;
using System;

namespace MessageBus.RabbitMq.Modules.Storage.Models
{
    public class MessageData
    {
        public string Id { get; set; }

        public Guid MessageId { get; set; }

        public DateTime DateTime { get; set; }

        public string Host { get; set; }

        public string Application { get; set; }

        public string ClrType { get; set; }

        public string Handler { get; set; }

        public string Body { get; set; }

        public OperationStatus Status { get; set; }

        public OperationType Type { get; set; }
    }
}
