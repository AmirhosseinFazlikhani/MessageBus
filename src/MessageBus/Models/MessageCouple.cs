using System;

namespace MessageBus.Models
{
    public class MessageCouple
    {
        public Type Message { get; set; }

        public Type Handler { get; set; }
    }
}
