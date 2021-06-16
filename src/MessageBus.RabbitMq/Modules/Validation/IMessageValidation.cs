using MessageBus.RabbitMq.Messages;
using System;

namespace MessageBus.RabbitMq.Modules.Validation
{
    internal interface IMessageValidation
    {
        void ThrowIfDuplicate(Message message, Type handler);
    }
}
