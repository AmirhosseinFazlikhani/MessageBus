using MessageBus.RabbitMq.Messages;
using MessageBus.RabbitMq.Modules.Storage;
using MessageBus.RabbitMq.Modules.Storage.Enums;
using MessageBus.RabbitMq.Modules.Validation.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;

namespace MessageBus.RabbitMq.Modules.Validation.Concretes
{
    internal class MessageValidation : IMessageValidation
    {
        private readonly IMessageStorage _storage;
        private readonly MessageBusSettings _settings;
        private readonly ILogger<MessageValidation> _logger;

        public MessageValidation(
            IMessageStorage storage,
            MessageBusSettings settings,
            ILogger<MessageValidation> logger)
        {
            _storage = storage;
            _settings = settings;
            _logger = logger;
        }

        public void ThrowIfDuplicate(Message message, Type handler)
        {
            var sameMessages = _storage.FindAsync(message.Id).Result;
            _logger.LogInformation(sameMessages.Count().ToString());
            bool isDuplicate = sameMessages.Where(x => x.MessageId == message.Id)
                .Where(x => x.Type == OperationType.Receive)
                .Where(x => x.Status == OperationStatus.Succeeded)
                .Where(x => x.Application == _settings.Application)
                .Where(x => x.Host == Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString())
                .Where(x => x.Handler == handler.FullName)
                .Any();

            if (isDuplicate)
                throw new DuplicateMessageException(message.Id);
        }
    }
}
