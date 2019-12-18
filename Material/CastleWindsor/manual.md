### Castle.Windsor 是一套IOC框架，下面主要讲解怎么去进行注册

1. 使用 `IWindsorInstaller`

```csharp
// application starts 
var container = new WindsorContainer();

//adds and configures all components using WindsorInstallers form executing assembly
container.Install(FromAssembly.This());

//instantiate and configure root component and all its dependencies and their dependencies and ...
var king = container.Resolve<IKing>();

// clean up, application exits
container.Dispose();

//installers  写法
public class RepositoriesInstaller : IWindsorInstaller
{
	public void Install(IWindsorContainer container, IConfigurationStore store)
	{
		container.Register(Classes.FromThisAssembly()
			                .Where(Component.IsInSameNamespaceAs<King>())
			                .WithService.DefaultInterfaces()
			                .LifestyleTransient());
	}
} 

//注册installers 
var container = new WindsorContainer();
container.Install(
   new RepositoriesInstaller(),
   // and all your other installers
);
//或者利用 FromAssembly  
container.Install(
   FromAssembly.This(),
   FromAssembly.Named("Acme.Crm.Bootstrap"),
   FromAssembly.Containing<ServicesInstaller>(),
   FromAssembly.InDirectory(new AssemblyFilter("Extensions")),
   FromAssembly.Instance(this.GetPluginAssembly())
);
//或者使用 Configuration 
container.Install(
   Configuration.FromAppConfig(),
   Configuration.FromXmlFile("settings.xml"),
   Configuration.FromXml(new AssemblyResource("assembly://Acme.Crm.Data/Configuration/services.xml"))
);
```

2. 直接单个单个注册

```csharp
//当不指定生命周期的时候，默认的都是使用 `Singleton` 模式

//把 MyServiceImpl 注册为一个单例服务
container.Register(
	Component.For<MyServiceImpl>()
);


//注册服务 IMyService 与组件 MyServiceImpl
container.Register(
    Component.For<IMyService>()
        .ImplementedBy<MyServiceImpl>()
);
或者以类型来注册
container.Register(
    Component.For(typeof(IMyService))
        .ImplementedBy(typeof(MyServiceImpl))
);


//注册泛类型
container.Register(
    Component.For<IRepository<Customer>>()
        .ImplementedBy<NHRepository<Customer>>(),
);
但更好的还是这个
container.Register(
    Component.For(typeof(IRepository<>)
        .ImplementedBy(typeof(NHRepository<>)
);

//配置服务的生命周期
container.Register(
   Component.For<IMyService>()
      .ImplementedBy<MyServiceImpl>()
      .LifeStyle.Transient
);

//同一种服务两个实现
这种情况默认取第一个
container.Register(
    Component.For<IMyService>().ImplementedBy<MyServiceImpl>(),
    Component.For<IMyService>().ImplementedBy<OtherServiceImpl>()
);
或者通过名字区分，指定默认组件
container.Register(
    Component.For<IMyService>().ImplementedBy<MyServiceImpl>(),
    Component.For<IMyService>().Named("OtherServiceImpl").ImplementedBy<OtherServiceImpl>().IsDefault()
);


//注册已经存在的服务与组件
var customer = new CustomerImpl();
container.Register(
    Component.For<ICustomer>().Instance(customer)
);


//使用委托来注册服务的组件
container
   .Register(
      Component.For<IMyService>()
         .UsingFactoryMethod(
            () => MyLegacyServiceFactory.CreateMyService())
);


//当服务的组件被创建的时候，你想改变一些内容
container.Register(
    Component.For<IService>()
        .ImplementedBy<MyService>()
        .OnCreate((kernel, instance) => instance.Name += "a")
);


//一个组件对应多个服务
container.Register(
    Component.For<IUserRepository, IRepository>()
        .ImplementedBy<MyRepository>()
);


//服务的继承的注册方式
container.Register(
    Component.For<IUserRepository>()
        .Forward<IRepository, IRepository<User>>()
            .ImplementedBy<MyRepository>()
);

```

3. 按照惯例注册服务与组件

您可以不用一个一个的注册组件，可以使用 `Classes`或`Types`输入类,Classes另一方面预先过滤类型只考虑非抽象类。Types允许您从给定程序集（即类，接口，结构，委托和枚举）注册所有（或者更确切地说，如果使用默认设置，所有公共）类型。

一般我们使用 `Classes`

1. 基本类型实现接口

```csharp
Classes.FromThisAssembly().BasedOn<IMessage>()
```

2. 一定都会

```csharp
Classes.FromAssemblyContaining<MyController>().Where( t=> Attribute.IsDefined(t, typeof(CacheAttribute)))
```

3. 没有限制

```csharp
Classes.FromAssemblyNamed("Acme.Crm.Services").Pick()
```

他们会用 `BasedOn`, `Where` and `Pick`

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<IMessage>()
        .BasedOn(typeof(IMessageHandler<>)).WithService.Base()
        .Where(Component.IsInNamespace("Acme.Crm.MessageDTOs"))
);
```

4. 配置生命周期

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<SmartDispatcherController>()
        .Configure(c => c.Lifestyle.Transient)
);
```

5. Base()的含义

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn(typeof(ICommand<>)).WithService.Base(),
    Classes.FromThisAssembly()
        .BasedOn(typeof(IValidator<>)).WithService.Base()
);
```

6. DefaultInterfaces() 实施服务与组件名称匹配

如:ICustomerRepository/CustomerRepository, IMessageSender/SmsMessageSender

```csharp
container.Register(
    Classes.FromThisAssembly()
        .InNamespace("Acme.Crm.Services")
        .WithService.DefaultInterfaces()
);
```
7. ConfigureFor<T> 可以更细粒度的配置

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<ICommon>()
        .LifestyleTransient()
        .Configure(
            component => component.Named(component.Implementation.FullName + "XYZ")
        )
        .ConfigureFor<CommonImpl1>(
            component => component.DependsOn(Property.ForKey("key1").Eq(1))
        )
        .ConfigureFor<CommonImpl2>(
            component => component.DependsOn(Property.ForKey("key2").Eq(2))
        )
);
```

### 具体值的依赖，值来自常量、配置文件、嵌入式文件等

1. 普通值依赖

```csharp
var twitterApiKey = @"the key goes here";

container.Register(
    Component.For<ITwitterCaller>().ImplementedBy<MyTwitterCaller>()
        .DependsOn(Dependency.OnValue("APIKey", twitterApiKey))
);
```

2. 属性值指定

```csharp
var config = new TwitterApiConfiguration {
    // set all the properties here...
};

container.Register(
    Component.For<ITwitterCaller>().ImplementedBy<MyTwitterCaller>()
        .DependsOn(Dependency.OnValue<TwitterApiConfiguration>(config))
);
```

```csharp
container.Register(
    Component.For<ICustomer>().ImplementedBy<CustomerImpl>()
        .DependsOn(Property.ForKey("Name").Eq("Caption Hook"), Property.ForKey("Age").Eq(45)));
//或者 OnCreate 中调用委托
``` 

3. 指定类型依赖

```csharp
container.Register(
    Component.For<ITransactionProcessingEngine>().ImplementedBy<TransactionProcessingEngine>()
        .DependsOn(Dependency.OnComponent(typeof(ILogger), "secureLogger"))
);

//或者
container.Register(
    Component.For<ITransactionProcessingEngine>().ImplementedBy<TransactionProcessingEngine>()
        .DependsOn(Dependency.OnComponent<ILogger, SecureLogger>())
);
```

4. `appSettings`配置文件依赖

```csharp
container.Register(
    Component.For<ITwitterCaller>().ImplementedBy<MyTwitterCaller>()
        .DependsOn(Dependency.OnAppSettingsValue("timeout", "twitterApiTimeout"))
);
```

5. 嵌入式文件依赖

```csharp
container.Register(
    Component.For<MainViewModel>()
        .DependsOn(Dependency.OnResource<MyApp.Properties.Resources>("DisplayName", "MainWindowTitle"))
);
```

6. 实时运行的参数依赖

```csharp
container.Register(
    Component.For<ClassWithArguments>()
        .LifestyleTransient()
        .DynamicParameters((k, d) => d["createdTimestamp"] = DateTime.Now)
);
```

### 有条件的注册服务与组件

1. 组件在没有被注册的时候，才会被注册
```csharp
container.Register(
    Component.For(typeof(IRepository<>))
        .ImplementedBy(typeof(Repository<>))
        .OnlyNewServices()
);
```

2. 从多个组件中过滤一个组件

```csharp
// 这种-01
container.Register(
    Classes.Of<ICustomer>()
        .FromAssembly(Assembly.GetExecutingAssembly())
        .Unless(t => typeof(SpecificCustomer).IsAssignableFrom(t))
);

//这种-02
container.Register(
   AllTypes.Of<ICustomer>()
      .FromAssembly(Assembly.GetExecutingAssembly())
      .If(t => t.FullName.Contains("Chain"))
);

//这种-03
container.Register(
    Classes.Of<CustomerChain1>()
        .Pick(from type in Assembly.GetExecutingAssembly().GetExportedTypes()
              where type.IsDefined(typeof(SerializableAttribute), true)
              select type
        )
);
```
### 容器内部的子系统  `HandlerFactory`，`ReleasePolicy`，`DependencyResolver`

### 讲一讲自定义生命周期

### 讲一讲`Facility`

