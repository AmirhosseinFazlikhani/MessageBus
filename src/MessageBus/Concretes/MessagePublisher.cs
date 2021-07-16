using System.Threading.Tasks;

namespace MessageBus.Concretes
{
    internal class MessagePublisher : IMessagePublisher
    {
        private readonly IPipelineFactory pipelineFactory;

        public MessagePublisher(IPipelineFactory pipelineFactory)
        {
            this.pipelineFactory = pipelineFactory;
        }

        public async Task PublishAsync(IMessage message)
        {
            var pipeline = pipelineFactory.CreatePublisherPipeline();
            await pipeline.Start(message);
        }
    }
}
