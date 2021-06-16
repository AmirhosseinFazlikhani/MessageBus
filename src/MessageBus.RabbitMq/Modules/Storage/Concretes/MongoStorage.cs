using MessageBus.RabbitMq.Messages;
using MessageBus.RabbitMq.Modules.Storage.Enums;
using MessageBus.RabbitMq.Modules.Storage.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Modules.Storage.Concretes
{
    internal class MongoStorage : IMessageStorage
    {
        private readonly IMongoCollection<MessageData> _collection;
        private readonly MessageBusSettings _settings;

        public MongoStorage(
            IMongoCollection<MessageData> collection,
            MessageBusSettings settings)
        {
            _collection = collection;
            _settings = settings;
        }

        public async Task<IReadOnlyCollection<MessageData>> FindAsync(Guid id)
        {
            var cursor = await _collection.FindAsync(x => x.MessageId == id);
            return await cursor.ToListAsync();
        }

        public async Task<IReadOnlyCollection<MessageData>> GetAsync(
            int from = 0,
            int size = 50,
            OperationType? type = null,
            OperationStatus? status = null,
            Type message = null)
        {
            var query = _collection.AsQueryable();

            if (type.HasValue)
                query = query.Where(x => x.Type == type);

            if (status.HasValue)
                query = query.Where(x => x.Status == status);

            if (message is not null)
                query = query.Where(x => x.ClrType == message.AssemblyQualifiedName);

            return await query.Skip(from).Take(size).ToListAsync();
        }

        void IMessageStorage.Save<T>(
            T message,
            OperationType type,
            OperationStatus status,
            Type handler)
        {
            var document = new MessageData
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = message.Id,
                DateTime = DateTime.UtcNow,
                Application = _settings.Application,
                ClrType = message.GetType().AssemblyQualifiedName,
                Handler = handler?.FullName,
                Host = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(),
                Status = status,
                Type = type,
                Body = JsonSerializer.Serialize(message),
            };

            _collection.InsertOne(document);
        }
    }
}
