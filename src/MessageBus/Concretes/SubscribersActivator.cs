using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Concretes
{
    public class SubscribersActivator : BackgroundService
    {
        private readonly Subscriber<IEvent> eventSubscriber;
        private readonly Subscriber<ICommand> commandSubscriber;

        public SubscribersActivator(
            Subscriber<IEvent> eventSubscriber,
            Subscriber<ICommand> commandSubscriber)
        {
            this.eventSubscriber = eventSubscriber;
            this.commandSubscriber = commandSubscriber;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await eventSubscriber.Execute();
            await commandSubscriber.Execute();
        }
    }
}
