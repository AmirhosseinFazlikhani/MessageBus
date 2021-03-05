using Docker.DotNet;
using Docker.DotNet.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.RabbitMq.IntegrationTests.Base
{
	public class RabbitMqFixture : IAsyncLifetime
	{
		private DockerClient DockerClient { get; set; }

		private string RabbitMqContainer { get; set; }

		public IConnection Connection { get; private set; }

		const string RabbitMqImage = "rabbitmq:3-management-alpine";

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
						"4369/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "4369"
							}
						}
					},
					{
						"5671/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "5671"
							}
						}
					},
					{
						"5672/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "5672"
							}
						}
					},
					{
						"25672/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "25672"
							}
						}
					},
					{
						"15671/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "15671"
							}
						}
					},
					{
						"15672/tcp",
						new List<PortBinding> {
							new PortBinding
							{
								HostPort = "15672"
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

			var connectionFactory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			await Task.Delay(5000);
			Connection = connectionFactory.CreateConnection();
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
