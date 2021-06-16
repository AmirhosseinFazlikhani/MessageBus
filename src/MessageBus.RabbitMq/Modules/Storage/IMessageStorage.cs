using MessageBus.RabbitMq.Messages;
using MessageBus.RabbitMq.Modules.Storage.Enums;
using MessageBus.RabbitMq.Modules.Storage.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Modules.Storage
{
    public interface IMessageStorage
    {
        internal void Save<T>(T message, OperationType type, OperationStatus status, Type handler = null) where T : Message;

        Task<IReadOnlyCollection<MessageData>> GetAsync(int from = 0, int size = 50, OperationType? type = null, OperationStatus? status = null, Type message = null);

        Task<IReadOnlyCollection<MessageData>> FindAsync(Guid id);
    }
}
