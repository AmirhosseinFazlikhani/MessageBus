using System;

namespace MessageBus.Settings
{
    public class MessageBusSettings
    {
        public string HostName { get; set; }

        public int Port { get; set; } = 5672;

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool AutomaticRecovery { get; set; } = true;

        public TimeSpan RecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    }
}
