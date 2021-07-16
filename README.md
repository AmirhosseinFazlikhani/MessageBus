# MessageBus
Implementation of command bus and event bus using RabbitMq.

## Nuget
```
Install-Package Berg.MessageBus.RabbitMq -Version <version>
```
# Usage
At first, you must register message bus in the DI container and enter the RabbitMq server details:
```cs
services.AddMessageBus(messagebus =>
{
	messagebus.ReadSettings(Configuration);

	messagebus.ConfigurePublisher(publisher =>
	{
		publisher.UseRouting();

		publisher.UseEventPublisher();
		publisher.UseCommandPublisher();
	});

	messagebus.ConfigureSubscriber(subscriber =>
	{
		subscriber.UseRouting();

		subscriber.UseEventSubscriber();
		subscriber.UseCommandSubscriber();
	});
});
```
MessageBus read settings from ```appsettings.json```. You must be pass ```IConfiguration``` to it.
``` json
"MessageBus": {
	"HostName": "localhost",
	"Port": 6572,
	"UserName": "guest",
	"Password": "guest",
	"AutomaticRecovery": true, // default value is true
	"RecoveryInterval": "00:00:05" // default value is 5s
}
```
If the connection is lost for any reason, application can try to recover it if *AutomaticRecovery* option is enabled.
MessageBus has an abstraction for publishing and subscribing message. You can implement these or use default implementation. In This instance of configuration, subscriber and publisher uses default implementaion for events and commands.
MessageBus supports middlewares. When a message is published, publishing middlewares are executed in order. ```RouterMiddleware``` must be executed last of all because it route message to its publisher, but don't call the next middleware. And in subscribing pipeline, ```RouterMiddleware``` route the received message to its handlers.

## Middlewares
A middleware is a class that implemented ```IMiddleware``` interface.
``` cs
public class LoggerMiddleware : IMiddleware
{
	private readonly ILogger<LoggerMiddleware> logger;

	public LoggerMiddleware(ILogger<LoggerMiddleware> logger)
	{
		this.logger = logger;
	}

	public async Task InvokeAsync(IMessage message, RequestDelegate next)
	{
		logger.LogInformation(message.GetHashCode().ToString());
		await next.Invoke(message);
	}
}
```
For add a middleware to publisher or subscriber pipeline, must use extension method ```UseMiddlware``` in configure pipelines section.
``` cs
messagebus.ConfigurePublisher(publisher =>
{
	publisher.UseMiddleware<LoggerMiddleware>();
});

messagebus.ConfigureSubscriber(subscriber =>
{
	subscriber.UseMiddleware<LoggerMiddleware>();
});
```
**Not:** A middleware can be added in both pipelines.

## Messages
All messages inherit from ```IMessage``` interface. But they are in two type: command & event. Commands after publishing placed in a specific queue and can has just one handler in the application. If the application is run in multiple instances, command handled in order by one of these. For example, we 2 instance of the application and ```SendEmailCommand``` published 3 times. The first time, command handled by instance 1, next time handled by instance 2, and the third time handled by instance 1. But events handled by all its handlers in all applications. For example, we have two microservice that proccess the new orders that the first microservice contains three handlers for the event and second microservice contains one handler. ```OrderCreatedEvent``` is published. This event received by four handlers, Three times in first microservice handlers and one time in second microservice.
Commands must be inherit from ```ICommand``` and events from ```IEvent```.

## Publishers
Each type of message has its own publisher. Publishers must be implement ```IPublisher<>```. For instance, a event publisher:
``` cs
public class EventPublisher : IPublisher<IEvent>
{
	public Task PublishAsync(IEvent message)
	{
	
	}
}
```
MessageBus has a default publisher for events and commands. But if you want to use your own implementation, must be register it in configure publisher section by extension method ```UsePublisher```.
``` cs
messagebus.ConfigurePublisher(publisher =>
{
	publisher.UsePublisher<IEvent, MyEventPublisher>();
	publisher.UsePublisher<ICommand, MyCommandPublisher>();
});
```
You have to consider that you can not create a connection to rabbitmq server. Application has a single connection in its lifetime that created in startup. For publish message, you can use ```IChannelPool``` for get a channel. At the end of publish process, the taken channel must be returned to the pool.
``` cs
public class EventPublisher : IPublisher<IEvent>
{
	private readonly IChannelPool channelPool;
	
	public EventPublisher(IChannelPool channelPool)
	{
		this.channelPool = channelPool;
	}

	public async Task PublishAsync(IEvent message)
	{
		var channel = channelPool.Get();
		
		// publish meesage
		
		channelPool.Release(channel);
	}
}
```

## Subscribers
Each type of message has its own subscriber. Subscibers must be implement ```Subscriber<>```. For instance, a event subscriber:
``` cs
public class EventSubscriber : Subscriber<IEvent>
{
	public EventSubscriber(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}

        protected override void Subscribe(Type messageType, IModel channel)
        {	
	}
}
```
Consider two event (```FirstTestEvent```, ```SecondTestEvent```) registered in application with its handlers. When the application runs, subscribe method is called two times. In the first time, ```messageType``` is type of ```FirstTestEvent``` and channel is a free channel that taked from channel pool. Subscribers is singleton and long-running process in application and no need to release channel. Because they need the channel while application is running. When a message was received, subscriber must be call ```HandleMessage(IMessage message)``` so that message is processed by middlewares.
Subscribers must be register in subscriber configuration section:
``` cs
messagebus.ConfigureSubscriber(subscriber =>
{
	subscriber.UseSubscriber<IEvent, MyEventSubscriber>();
	subscriber.UseSubscriber<ICommand, MyCommandSubscriber>();
});
```

## Publishing and handling message
```IMessagePublisher``` puts the message in pipeline.  You can use this module for publish any message.
``` cs
public class Test{
	private readonly IMessagePublisher publisher;

	public Test(IMessagePublisher publisher)
	{
	    this.publisher = publisher;
	}
	
	public async Task TestPublish()
	{
		await publisher.PublishAsync(new TestEvent());
		await publisher.PublishAsync(new TestCommand());
	}
}
```
For handling message, you must be implement ```IMessageHandler<>``` and register it.
``` cs
public class TestEventHandler : IMessageHandler<TestEvent>
{
	public Task HandleAsync(TestEvent message)
	{
		return Task.CompletedTask;
	}
}

public class TestCommandHandler : IMessageHandler<TestCommand>
{
	public Task HandleAsync(TestCommand message)
	{
		return Task.CompletedTask;
	}
}
```
``` cs
public void ConfigureServices(IServiceCollection services)
{
	services.AddMessageHandler<TestEvent, TestEventHandler>();
	services.AddMessageHandler<TestCommand, TestCommandHandler>();
}
```
