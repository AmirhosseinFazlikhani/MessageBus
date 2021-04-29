# MessageBus
Implementation of command bus and event bus using RabbitMq.

## Nuget
```
Install-Package Berg.MessageBus.RabbitMq -Version 5.0.0
```
#  Usage
At first, you must register message bus in the DI container and enter the rabbitMq server details:
```cs
services.AddMessageBus(new MessageBusSettings
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    Port = 5672, // Optional, default value is 5672
    MaxConcurrentChannels = 10 // Optional, default value is 10
});
```
Message bus opened a single connection in application lifetime. But can created several channel. RabbitMq channels are not thread safe and must not share in threads. For this reason message bus have a thread safe channel pool that has responsible for building, delivering and retrieving the channels. When application want to publish a message, if a free channel is exists, channel pool delivers it and after published message takes it back. But nothing free channel are exists, channel pool create a new channel. Channels are limited becuse creating channel almost a hard work and takes time and host resources, therefore channel pool creates a limited number of channels. If active channels is maximum, channel pool queues requests.

## Events
All event classes must be inherit from ***IntegrativeEvent***:
```cs
public class TestEvent : IntegrativeEvent
{
    public string Name { get; set; }
}
```
An event can have several handler. Event handler is a class that implement ***IEventHandler<>***:
```cs
public class TestEventHandler : IEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent @event)
    {
        return Task.CompletedTask;
    }
}
```
When an event published, are received by all its handlers. You can publish an event with ***PublishAsync()*** method of ***IMessageBus***:
```cs
public class Test
{
	private readonly IMessageBus _messageBus;

	public Test(IMessageBus messageBus)
	{
	        _messageBus = messageBus;
	}

	public async void Publish()
	{
	    await _messageBus.PublishAsync(new TestEvent());
	}
}
```
Event handlers must be register in application startup:
```cs
services.AddEventHandler<TestEvent, TestEventHandler>();
```

## Commands
All command classes must be inherit from ***Command***:
```cs
public class TestCommand: Command
{
    public string Name { get; set; }
}
```
An event can have several handler. Event handler is a class that implement ***IEventHandler<>***:
```cs
public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public Task HandleAsync(TestCommand command)
    {
        return Task.CompletedTask;
    }
}
```
If two or more handler registered for a command, it just handle by one of them, but other handlers will not be useless. Consider 4 command was sent and 2 handler registered in application, first command handled by handler1, second command handled by handler2, third command handled by handler1, and fourth command handled by handler2.
You can send a command with ***SendAsync()*** method of ***IMessageBus***:
```cs
public class Test
{
	private readonly IMessageBus _messageBus;

	public Test(IMessageBus messageBus)
	{
	        _messageBus = messageBus;
	}

	public async void Send()
	{
	    await _messageBus.SendAsync(new TestCommand());
	}
}
```
Command handlers must be register in application startup:
```cs
services.AddCommandHandler<TestCommand, TestCommandHandler>();
```
