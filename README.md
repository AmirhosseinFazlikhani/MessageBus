# EventBus
A simple event bus that use RabbitMq.

All events should be inherit from IntegrativeEvent. You can track events in logs with its unique id that is a Guid.
```
 public class TestIE : IntegrativeEvent
 {
  public string Name { get; set; }
 }
```
Use ***IMessagePublisher*** for publish messages:
```
public class ExampleClass
{
  private readonly IMessagePublisher _publisher;

  public ExampleClass(IMessagePublisher publisher)
  {
    _publisher = publisher;
  }

  public async Task Publish()
  {
    var @event = new TestIE { Name = "test event" };
    await _publisher.PublishAsync(@event);
  }
}
```
Event can has unlimited handler. event handlers should inherit from ***BaseEventHandler*** and implement two method:
- **GetServices**: You can resolve scoped services from a ***IServiceProvider*** that passed to this method.
- **Handle**: When received an event which type is equal to event handler generic type, invoke this method.
```
public class TestEH : BaseEventHandler<TestIE>
{
	public TestEH(
		IMessageSubscriber subscriber,
		IServiceScopeFactory scopeFactory) : base(subscriber, scopeFactory) { }

	private IUnitOfWork _unitOfWork;

	public override void GetServices(IServiceProvider serviceProvider)
	{
		_unitOfWork = serviceProvider.GetService<IUnitOfWork>();
	}
    
	public override void Handle(TestIE @event)
	{
		// ...
	}
}
```
You should register event handlers:
```
services.AddEventHandler<TestIE, TestEH>();
```
For register EventBus you should pass RabbitMq server informations:
```
services.AddEventBus(new MessagingConfig
{
	HostName = "localhost",
	UserName = "guest",
	Password = "guest"
}
```
You can get all registered events and handlers from ***IServiceProvider***:
```
var events = services.GetEvents();
```
And get all handlers for an event:
```
var handlers = services.GetEventHandlers<TestIE>();
// or
var handlers = services.GetEventHandlers(typeof(TestIE));
```
