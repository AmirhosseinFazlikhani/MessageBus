using System;
using System.Collections.Generic;

namespace MessageBus.Concretes
{
    public class HandlersStorage
    {
        public IList<Pair> Pairs { get; } = new List<Pair>();
    }

    public class Pair
    {
        public Type Message { get; set; }

        public Type Handler { get; set; }
    }
}
