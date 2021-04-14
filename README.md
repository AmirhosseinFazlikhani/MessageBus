# EventBus
A simple event bus that use RabbitMq.

All events should be inherit from IntegrativeEvent. You can track events in logs with its unique id that is a Guid.
```
public class TestEvent : IntegrativeEvent
{
	public string Name { get; set; }
}
```
Use ***IMessagePublisher*** for publish messages:
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
Event can has unlimited handler. event handlers should inherit from ***BaseEventHandler*** and implement two method:
- **Initialize**: You can resolve services from a ***IServiceProvider***.
- **Handle**: When received an event which type is equal to event handler generic type, invoke this method.
```
public class TestEventHandler : IntegrativeEventHandler<TestEvent>
{
	public TestEventHandler(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

	public override void Initialize(IServiceProvider serviceProvider)
	{
		// ...
	}
    
	public override void Handle(TestEvent @event)
	{
		// ...
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
You can get all registered events and handlers from ***IServiceProvider***:
```
var events = services.GetEvents();
```
And get all handlers for an event:
```
var handlers = services.GetEventHandlers<TestEvent>();
// or
var handlers = services.GetEventHandlers(typeof(TestEvent));
```
