using MessageBus.RabbitMq.Modules.Storage.Enums;
using MessageBus.RabbitMq.Modules.Storage.Models;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBus.RabbitMq.Modules.Storage.Concretes
{
    internal class ElasticStorage : IMessageStorage
    {
        private readonly IElasticClient _client;
        private readonly ILogger<ElasticStorage> _logger;
        private readonly MessageBusSettings _settings;

        public ElasticStorage(
            IElasticClient client,
            ILogger<ElasticStorage> logger,
            MessageBusSettings settings)
        {
            _client = client;
            _logger = logger;
            _settings = settings;
        }

        async Task IMessageStorage.SaveAsync<T>(
            T message,
            OperationType type,
            OperationStatus status)
        {
            var document = new MessageData
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = message.Id,
                DateTime = DateTime.UtcNow,
                Application = _settings.Application,
                ClrType = message.GetType().AssemblyQualifiedName,
                Host = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(),
                Status = status,
                Type = type,
                Body = JsonSerializer.Serialize(message),
            };

            var response = await _client.IndexAsync(
                document,
                idx => idx.Index(_settings.Elasticsearch.Index));

            if (response.Result == Result.Error)
            {
                _logger.LogError(response.OriginalException, "An error was eccured while indexing message {MessageId} on Elasticserach.", message.Id);
            }
        }

        public async Task<IReadOnlyCollection<MessageData>> GetAsync(
            int from = 0,
            int size = 50,
            OperationType? type = null,
            OperationStatus? status = null,
            Type message = null)
        {
            var search = new SearchDescriptor<MessageData>()
                .Index(_settings.Elasticsearch.Index)
                .From(from)
                .Size(size);

            if (type.HasValue)
            {
                search = search.Query(q => q
                      .Match(m => m
                          .Field(f => f.Type)
                          .Query(((int)type).ToString())
                          )
                    );
            }

            if (status.HasValue)
            {
                search = search.Query(q => q
                      .Match(m => m
                          .Field(f => f.Status)
                          .Query(((int)status).ToString())
                          )
                    );
            }

            if (message is not null)
            {
                search = search.Query(q => q
                      .Match(m => m
                          .Field(f => f.ClrType)
                          .Query(message.AssemblyQualifiedName)
                          )
                    );
            }

            var data = await _client.SearchAsync<MessageData>(search);
            return data.Documents;
        }

        public async Task<IReadOnlyCollection<MessageData>> FindAsync(Guid id)
        {
            var search = new SearchDescriptor<MessageData>()
                .Index(_settings.Elasticsearch.Index)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.MessageId)
                        .Query(id.ToString())
                        )
                    );

            var daat = await _client.SearchAsync<MessageData>(search);
            return daat.Documents;
        }
    }
}
