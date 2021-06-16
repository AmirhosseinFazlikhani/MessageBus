using System;

namespace MessageBus.RabbitMq.Modules.Validation.Exceptions
{
    public class DuplicateMessageException : Exception
    {
        public Guid MessageId { get; set; }

        public DuplicateMessageException(Guid id)
        {
            MessageId = id;
        }
    }
}
