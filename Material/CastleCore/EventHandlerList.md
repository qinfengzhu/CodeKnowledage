### 事件集合的写法

```
//样本取自Castle.MicroKernel
public partial class DefaultKernel : MarshalByRefObject, IKernel, IKernelEvents, IDeserializationCallback
{
	private static readonly object HandlerRegisteredEvent = new object(); //只读对象或者只读字符串
	private static readonly object ComponentRegisteredEvent = new object(); //只读对象或者只读字符串
	private static readonly object ComponentUnregisteredEvent = new object(); //只读对象或者只读字符串

	[NonSerialized]
    // 一个EventHandlerList初始只要4个字节,如果是一个Event接一个Event的话,则每一个Event会有4个字节
    //这样内存消耗过多
	private readonly EventHandlerList events = new EventHandlerList();
	public override object InitializeLifetimeService()
	{
		return null;
	}

	public event HandlerDelegate HandlerRegistered
	{
		add { events.AddHandler(HandlerRegisteredEvent, value); }
		remove { events.RemoveHandler(HandlerRegisteredEvent, value); }
	}

	public event ComponentDataDelegate ComponentRegistered
	{
		add { events.AddHandler(ComponentRegisteredEvent, value); }
		remove { events.RemoveHandler(ComponentRegisteredEvent, value); }
	}

	public event ComponentDataDelegate ComponentUnregistered
	{
		add { events.AddHandler(ComponentUnregisteredEvent, value); }
		remove { events.RemoveHandler(ComponentUnregisteredEvent, value); }
	}

	protected virtual void RaiseComponentRegistered(String key, IHandler handler)
	{
		ComponentDataDelegate eventDelegate = (ComponentDataDelegate) events[ComponentRegisteredEvent];
		if (eventDelegate != null) eventDelegate(key, handler);
	}

	protected virtual void RaiseComponentUnregistered(String key, IHandler handler)
	{
		ComponentDataDelegate eventDelegate = (ComponentDataDelegate) events[ComponentUnregisteredEvent];
		if (eventDelegate != null) eventDelegate(key, handler);
	}

	public virtual void RaiseHandlerRegistered(IHandler handler)
	{
		bool stateChanged = true;

		while(stateChanged)
		{
			stateChanged = false;
			HandlerDelegate eventDelegate = (HandlerDelegate) events[HandlerRegisteredEvent];
			if (eventDelegate != null) eventDelegate(handler, ref stateChanged);
		}
	}
}
```
### EventHandlerList的好处

如果你发现你的服务组件对外需要提供很多的事件，而这些事件一般情况下你认为很少有程序拦截。
使用EventHandlerList提供的功能将很适合你，如果使用.NET提供的默认事件机制，你可能在创建实例时消耗较多 的内存，
而使用EventHandlerList挂接事件将节约内存。
