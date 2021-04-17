# EventBus
A simple event bus that use RabbitMq.

All events should be inherit from IntegrativeEvent. You can track events in logs with its unique identifier that's a Guid.
```
public class TestEvent : IntegrativeEvent
{
	public string Name { get; set; }
}
```
Use ***IEventPublisher*** for publish messages:
```
public class ExampleClass
{
	private readonly IEventPublisher _publisher;

	public ExampleClass(IEventPublisher publisher)
	{
		_publisher = publisher;
	}

	public async Task Publish()
	{
		var @event = new TestEvent { Name = "test event" };
		await _publisher.PublishAsync(@event);
	}
}
```
Event can has unlimited handlers. event handlers should be implement ***IEventHandler<>***.
```
public class TestEventHandler : IEventHandler<TestEvent>
{
	public Task HandleAsync(TestEvent @event)
	{
		
	}
}
```
You should register event handlers:
```
services.AddEventHandler<TestEvent, TestEventHandler>();
```
For register EventBus you should pass RabbitMq server informations:
```
services.AddEventBus(new EventBusSettings
{
	HostName = "localhost",
	UserName = "guest",
	Password = "guest",
	Port = 5672 // Optional
}
```
