using System;

namespace MessageBus.Models
{
    internal class MessageCouple
    {
        public Type Message { get; set; }

        public Type Handler { get; set; }
    }
}
