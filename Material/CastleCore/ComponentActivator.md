### 组件构建器ComponentActivator

1. 通常情况下，我们 `Activator.CreateInstance()`就可以了，但有时候我们更愿意把构建交给一些`factory`或者`builder`

```
public interface IComponentActivator
{
    object Create(CreationContext context); //根据构建上下文来构建对象
    void Destroy(object instance);
}
```

2. 抽象的`AbstractComponentActivator`组件创建器

```
public abstract class AbstractComponentActivator : IComponentActivator
{
	private IKernel kernel; //内核
	private ComponentModel model;//组件模型
	private ComponentInstanceDelegate onCreation;//组件构建后，触发构建后的事件
	private ComponentInstanceDelegate onDestruction;//组件销毁后，触发销毁事件

	/// <summary>
	/// 默认构造函数
	/// </summary>
	public AbstractComponentActivator(ComponentModel model, IKernel kernel,
		ComponentInstanceDelegate onCreation,
		ComponentInstanceDelegate onDestruction)
	{
		this.model = model;
		this.kernel = kernel;
		this.onCreation = onCreation;
		this.onDestruction = onDestruction;
	}

	public IKernel Kernel
	{
		get { return kernel; }
	}

	public ComponentModel Model
	{
		get { return model; }
	}

	public ComponentInstanceDelegate OnCreation
	{
		get { return onCreation; }
	}

	public ComponentInstanceDelegate OnDestruction
	{
		get { return onDestruction; }
	}

	#region IComponentActivator Members

	public virtual object Create(CreationContext context)
	{
		object instance = InternalCreate(context);

		onCreation(model, instance);

		return instance;
	}

	public virtual void Destroy(object instance)
	{
		InternalDestroy(instance);

		onDestruction(model, instance);
	}

	#endregion

	protected abstract object InternalCreate(CreationContext context);

	protected abstract void InternalDestroy(object instance);
}
```
所以如果自定义组件创建器,则必须实现 `InternalCreate(CreationContext context)`,`InternalDestroy(object instance)`

3. 自定义实现组件激活器`MyCustomerActivator`

代码实现,组件激活器
```
public class MyCustomerActivator : AbstractComponentActivator
{
    public MyCustomerActivator(ComponentModel model, IKernel kernel,
        ComponentInstanceDelegate onCreation,
        ComponentInstanceDelegate onDestruction)
        : base(model, kernel, onCreation, onDestruction)
    {
    }

    protected override object InternalCreate(CreationContext context)
    {
        CustomerImpl customer = new CustomerImpl();
        customer.Name = "James Bond";
        return customer;			
    }

    protected override void InternalDestroy(object instance)
    {
    }
}
```
代码实现，注册组件激活器
```
kernel.Register(
    Component.For<ICustomer>()
        .Named("customer")
        .ImplementedBy<CustomerImpl>()
        .Activator<MyCustomerActivator>()
        );

//IHandler handler = kernel.GetHandler("customer");
//Assert.AreEqual(typeof(MyCustomerActivator), handler.ComponentModel.CustomComponentActivator);

ICustomer customer = kernel.Resolve<ICustomer>();
```

4. `CreationContext` 更具体说明
