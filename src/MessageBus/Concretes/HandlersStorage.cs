using System;
using System.Collections.Generic;

namespace MessageBus.Concretes
{
    public class HandlersStorage
    {
        public IDictionary<Type, Type> Pairs { get; } = new Dictionary<Type, Type>();
    }
}
