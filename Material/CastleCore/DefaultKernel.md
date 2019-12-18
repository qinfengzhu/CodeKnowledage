### `DefaultKernal` 构建基础

```
public DefaultKernel(IProxyFactory proxyFactory)
{
    this.proxyFactory = proxyFactory;

    childKernels = new ArrayList();
    facilities = new ArrayList();
    subsystems = new Hashtable();

    RegisterSubSystems(); //这里挂载了4个subSystems

    releaserPolicy = new LifecycledComponentsReleasePolicy(); //组件释放规则
    handlerFactory = new DefaultHandlerFactory(this); //组件处理器
    modelBuilder = new DefaultComponentModelBuilder(this);//组件Model构造器
    resolver = new DefaultDependencyResolver(this);//组件依赖解析器
    resolver.Initialize(new DependencyDelegate(RaiseDependencyResolving));//解析器初始化
}
protected virtual void RegisterSubSystems()
{
    AddSubSystem(SubSystemConstants.ConfigurationStoreKey,
                 new DefaultConfigurationStore());

    AddSubSystem(SubSystemConstants.ConversionManagerKey,
                 new DefaultConversionManager());

    AddSubSystem(SubSystemConstants.NamingKey,
                 new DefaultNamingSubSystem());

    AddSubSystem(SubSystemConstants.ResourceKey,
                 new DefaultResourceSubSystem());
}
public virtual void AddSubSystem(String key, ISubSystem subsystem)
{
    if (key == null) throw new ArgumentNullException("key");
    if (subsystem == null) throw new ArgumentNullException("subsystem");

    subsystem.Init(this);
    subsystems[key] = subsystem;
}
```
### 分析四个SubSytems

1. [DefaultConfigurationStore](DefaultConfigurationStore.md)

2. [DefaultConversionManager](DefaultConversionManager.md)

3. [DefaultNamingSubSystem](DefaultNamingSubSystem.md)

4. [DefaultResourceSubSystem](DefaultResourceSubSystem.md)

### 分析LifecycledComponentsReleasePolicy

### 分析DefaultHandlerFactory

### 分析DefaultComponentModelBuilder

### 分析DefaultDependencyResolver

### 分析AddComponent

1. 底层实际调用方法为 `public void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle,bool overwriteLifestyle)`

```
public void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle,
                         bool overwriteLifestyle)
{
    if (key == null) throw new ArgumentNullException("key");
    if (serviceType == null) throw new ArgumentNullException("serviceType");
    if (classType == null) throw new ArgumentNullException("classType");
    if (LifestyleType.Undefined == lifestyle)
        throw new ArgumentException("The specified lifestyle must be Thread, Transient, or Singleton.", "lifestyle");
    ComponentModel model = ComponentModelBuilder.BuildModel(key, serviceType, classType, null); //构建组件模型与配置

    if (overwriteLifestyle || LifestyleType.Undefined == model.LifestyleType)
    {
        model.LifestyleType = lifestyle;
    }

    RaiseComponentModelCreated(model); //触发组件构建模型事件
    IHandler handler = HandlerFactory.Create(model);//组件处理程序工厂，创建一个组件处理程序器
    RegisterHandler(key, handler);//
}
protected void RegisterHandler(String key, IHandler handler)
{
    RegisterHandler(key, handler, false);
}

protected void RegisterHandler(String key, IHandler handler, bool skipRegistration)
{
    if (!skipRegistration)
    {
        NamingSubSystem.Register(key, handler);
    }

    RaiseHandlerRegistered(handler); // 触发组件程序处理器注册事件
    RaiseComponentRegistered(key, handler);
}
```
2. 核心分析BuildModel业务逻辑[DefaultComponentModelBuilder](DefaultComponentModelBuilder.md)

3. 核心分析`HandlerFactory.Create` 来创建 `IHandler`，[DefaultHandlerFactory](DefaultHandlerFactory.md)
