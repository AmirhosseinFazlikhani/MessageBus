using Microsoft.Extensions.DependencyInjection;
using System;

namespace MessageBus.Concretes
{
    public class PipelineFactory : IPipelineFactory
    {
        private readonly IServiceProvider serviceProvider;

        public PipelineFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IPipeline CreatePublisherPipeline()
        {
            var storage = serviceProvider.GetRequiredService<PublisherMiddlewareStorage>();
            return CreateInstance(storage);
        }

        public IPipeline CreateSubscriberPipeline()
        {
            var storage = serviceProvider.GetRequiredService<SubscriberMiddlewareStorage>();
            return CreateInstance(storage);
        }

        private IPipeline CreateInstance(IMiddlewareStorage storage)
        {
            var pipeline = ActivatorUtilities.CreateInstance(
                serviceProvider,
                typeof(Pipeline),
                storage.Middlewares.GetEnumerator());

            return pipeline as IPipeline;
        }
    }
}
