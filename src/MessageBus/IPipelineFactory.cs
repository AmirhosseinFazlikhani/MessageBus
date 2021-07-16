namespace MessageBus
{
    public interface IPipelineFactory
    {
        IPipeline CreatePublisherPipeline();

        IPipeline CreateSubscriberPipeline();
    }
}
