using MessageBus.Models;
using System.Collections.Generic;

namespace MessageBus.Concretes
{
    internal class HandlersStorage
    {
        public List<MessageCouple> EventCouples { get; } = new List<MessageCouple>();

        public List<MessageCouple> CommandCouples { get; }=new List<MessageCouple>();
    }
}
