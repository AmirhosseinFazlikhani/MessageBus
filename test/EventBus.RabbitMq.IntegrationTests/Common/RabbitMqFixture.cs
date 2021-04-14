using Docker.DotNet;
using Docker.DotNet.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace eShop.EventBus.IntegrationTests.Common
{
	public class RabbitMqFixture : IAsyncLifetime
	{
		private DockerClient DockerClient { get; set; }

		private string RabbitMqContainer { get; set; }

		public IConnection Connection { get; private set; }

		const string RabbitMqImage = "rabbitmq:3";

		public RabbitMqFixture()
		{
			RabbitMqContainer = "rabbitmq" + Guid.NewGuid().ToString("N");
		}

		public async Task InitializeAsync()
		{
			var address = Environment.OSVersion.Platform == PlatformID.Unix
				? new Uri("unix:///var/run/docker.sock")
				: new Uri("npipe://./pipe/docker_engine");

			var config = new DockerClientConfiguration(address);
			DockerClient = config.CreateClient();

			var imageParameters = new ImagesCreateParameters
			{
				FromImage = RabbitMqImage,
				Tag = "latest"
			};

			var images = await DockerClient.Images.ListImagesAsync(new ImagesListParameters { MatchName = RabbitMqImage });
			if (!images.Any())
			{
				await DockerClient.Images.CreateImageAsync(imageParameters, null, IgnoreProgress.Forever);
			}

			var hostConfig = new HostConfig
			{
				PortBindings = new Dictionary<string, IList<PortBinding>>
				{
					{
						"5672/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "7997"
							}
						}
					}
				}
			};

			await DockerClient.Containers.CreateContainerAsync(
				new CreateContainerParameters
				{
					Image = RabbitMqImage,
					Name = RabbitMqContainer,
					Tty = true,
					HostConfig = hostConfig
				});

			await DockerClient.Containers.StartContainerAsync(RabbitMqContainer, new ContainerStartParameters { });

			var factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest",
				Port = 7997
			};

			await Task.Delay(15000);

			Connection = factory.CreateConnection();
		}

		public async Task DisposeAsync()
		{
			if (DockerClient != null)
			{
				await DockerClient.Containers.StopContainerAsync(RabbitMqContainer, new ContainerStopParameters { });
				await DockerClient.Containers.RemoveContainerAsync(RabbitMqContainer, new ContainerRemoveParameters { Force = true });

				DockerClient.Dispose();
			}
		}

		private class IgnoreProgress : IProgress<JSONMessage>
		{
			public static readonly IProgress<JSONMessage> Forever = new IgnoreProgress();

			public void Report(JSONMessage value) { }
		}
	}
}
