### Abp与castle.windsor结合

1. Abp 引用了Castle的那些程序集: `Castle.Core`,`Castle.Windsor`,`Castle.Faccilities.Logging`

2. Abp 的启动点，主要实现模块之间的依赖以及核心的IOC容器初始化,`Startup/AbpBootstrapper` ，这里主要使用`IWindsorInstaller` 接口，`IWindsorContainer.Install()`

3. 下面进行具体代码的分析

3.1 `AbpBootstrapper` 入口初始化系统类
```
public class AbpBootstrapper : IDisposable
{
    private AbpApplicationManager _applicationManager;

    /// <summary>
    /// 初始化加载
    /// </summary>
    public virtual void Initialize()
    {
        //Installer 加载当前程序集中的Installer,这里有一个静态函数技巧【在创建第一个实例或引用任何静态成员之前,由.net底层自动调用】
        //这里有两个核心Installer: Startup/AbpStartupInstaller 与 Modules/AbpModuleSystemInstaller
        IocManager.Instance.IocContainer.Install(FromAssembly.This());
        //获取AbpApplicationManager的注册单例对象
        _applicationManager = IocHelper.Resolve<AbpApplicationManager>();
        //执行ApplicationManager进行初始化
        _applicationManager.Initialize();
    }

    /// <summary>
    /// 释放
    /// </summary>
    public virtual void Dispose()
    {
        _applicationManager.Dispose();
        IocManager.Instance.Dispose();
    }
}
```
3.2 这里有个隐藏点，就是`IocContainer`自己的实例化

```
public class IocManager : IDisposable
{

    public static IocManager Instance { get; private set; }

    public IWindsorContainer IocContainer { get; private set; }
    private readonly List<IConventionalRegisterer> _conventionalRegisterers;

    //使用静态函数,就是当IocManager.Instance被调用的时候，IocContainer 直接被初始化为WindsorContainer
    static IocManager()
    {
        Instance = new IocManager();
    }

    private IocManager()
    {
        IocContainer = new WindsorContainer();
        _conventionalRegisterers = new List<IConventionalRegisterer>();
    }
    public void Dispose()
    {
        IocContainer.Dispose();
    }
        //额外功能的注入器，默认的注入器 BasicConventionalRegisterer
        /// <summary>
        /// <summary>
       /// Adds a dependency registerer for conventional registration.
       /// </summary>
       /// <param name="registerer">dependency registerer</param>
       public void AddConventionalRegisterer(IConventionalRegisterer registerer)
       {
           _conventionalRegisterers.Add(registerer);
       }

       /// <summary>
       /// Registers types of given assembly by all conventional registerers. See <see cref="AddConventionalRegisterer"/> method.
       /// </summary>
       /// <param name="assembly">Assembly to register</param>
       public void RegisterAssemblyByConvention(Assembly assembly)
       {
           RegisterAssemblyByConvention(assembly, new ConventionalRegistrationConfig());
       }

       /// <summary>
       /// Registers types of given assembly by all conventional registerers. See <see cref="AddConventionalRegisterer"/> method.
       /// </summary>
       /// <param name="assembly">Assembly to register</param>
       /// <param name="config">Additional configuration</param>
       public void RegisterAssemblyByConvention(Assembly assembly, ConventionalRegistrationConfig config)
       {
           var context = new ConventionalRegistrationContext(assembly, IocContainer, config);

           foreach (var registerer in _conventionalRegisterers)
           {
               registerer.RegisterAssembly(context);
           }

           if (config.InstallInstallers)
           {
               IocContainer.Install(FromAssembly.Instance(assembly));                
           }
       }
}
```

3.3 `AbpStartupInstaller`在`Startup目录下面`,Abp启动的Installer,实现`IWindsorInstaller`接口

```
public class AbpStartupInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        //注册AbpApplicationManager 为单例对象
        container.Register(Component.For<AbpApplicationManager>().LifestyleSingleton());
    }
}
```

3.4 `AbpModuleSystemInstaller` 在`Modules目录下面`,Abp模块的Installer,实现`IWindsorInstaller`接口

```
public class AbpModuleSystemInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(
            Component.For<AbpModuleCollection>().LifestyleSingleton(), //单例模式
            Component.For<AbpModuleManager>().LifestyleSingleton(), //单例模式
            Component.For<AbpModuleLoader>().LifestyleTransient() //每次进行new 的模式注册
            );
    }
}
```

3.5  核心点现在来到了`ApplicationManager.Initialize()`的初始化调用方法

```
public class AbpApplicationManager
{
    private readonly AbpModuleManager _moduleManager;
    private readonly AbpModuleCollection _modules;

    //AbpModuleSystemInstaller 中已经注册了AbpModuleManager 与 AbpModuleCollection 所以这里可以用得到,
    //他们注册的时候都是单例模式
    public AbpApplicationManager(AbpModuleManager moduleManager, AbpModuleCollection modules)
    {
        _moduleManager = moduleManager;
        _modules = modules;
    }

    /// <summary>
    /// 初始化程序集
    /// </summary>
    public virtual void Initialize()
    {
        var initializationContext = new AbpInitializationContext(_modules); // 初始化一个Abp初始化上下文
        _moduleManager.Initialize(initializationContext);//Module管理器进行初始化
    }

    /// <summary>
    /// Disposes/shutdowns the application.
    /// </summary>
    public virtual void Dispose()
    {
        _moduleManager.Shutdown();
    }
}
```

3.6 `AbpInitializationContext` 仅起到一个承上启下的作用

```
internal class AbpInitializationContext : IAbpInitializationContext
{
    /// <summary>
    /// IocContainer的容器引用
    /// </summary>
    public IWindsorContainer IocContainer { get { return IocManager.Instance.IocContainer; } }

    private readonly AbpModuleCollection _modules;

    public AbpInitializationContext(AbpModuleCollection modules)
    {
        _modules = modules;
    }

    //获取指定Modoule
    public TModule GetModule<TModule>() where TModule : AbpModule
    {
        return _modules.GetModule<TModule>();
    }
}
```

3.7 `AbpModuleManager.Initialize` 核心点是加载所有引用过的`Assembly`中实现`IAbpModule`接口的所有具体类

```
public class AbpModuleManager
{
    private readonly AbpModuleCollection _modules;
    private readonly AbpModuleLoader _moduleLoader;

    public AbpModuleManager(AbpModuleCollection modules, AbpModuleLoader moduleLoader)
    {
        _moduleLoader = moduleLoader;
        _modules = modules;
    }

    public virtual void Initialize(IAbpInitializationContext initializationContext)
    {
        //这个方法最有意思,由AppDomain.CurrentDomain.GetAssemblies();
        //然后延伸为 assembly.GetReferencedAssemblies();由点到面的铺开，使用递归方式,加一个记录扫描过的程序集
        // scannedAssemlies 实现扫描所有的AbpModule
        _moduleLoader.LoadAll();

        //这个也有点意思，主要是对AbpModule模块之间依赖进行排序
        var sortedModules = _modules.GetSortedModuleListByDependency();
        sortedModules.ForEach(module => module.Instance.PreInitialize(initializationContext));
        sortedModules.ForEach(module => module.Instance.Initialize(initializationContext));
        sortedModules.ForEach(module => module.Instance.PostInitialize(initializationContext));
    }

    public virtual void Shutdown()
    {
        var sortedModules = _modules.GetSortedModuleListByDependency();
        sortedModules.Reverse();
        sortedModules.ForEach(sm => sm.Instance.Shutdown());
    }
}
```

3.8  `AbpModuleLoader` 由点到面的查找所有的 `IAbpModule` 实现无死角无遗漏

```
public class AbpModuleLoader
{
   public ILogger Logger { get; set; }

   private readonly AbpModuleCollection _modules;

   public AbpModuleLoader(AbpModuleCollection modules)
   {
       _modules = modules;
       Logger = NullLogger.Instance;
   }

   public void LoadAll()
   {
       Logger.Debug("Loading Abp modules...");

       _modules.Add(AbpModuleInfo.CreateForType(typeof(AbpStartupModule)));
       //这个以扫描记录的方式传入进去
       var scannedAssemlies = new List<Assembly>();

       var assemblies = AppDomain.CurrentDomain.GetAssemblies();
       foreach (var assembly in assemblies)
       {
           FillModules(assembly, scannedAssemlies);
       }
       //按程序集依赖,对Module进行初步排序
       SetDependencies();

       Logger.DebugFormat("{0} modules loaded.", _modules.Count);
   }

   private void FillModules(Assembly assembly, List<Assembly> scannedAssemblies)
   {
       if (scannedAssemblies.Contains(assembly))
       {
           return;
       }

       scannedAssemblies.Add(assembly);
       //这个有点意思
       var referencedAssemblyNames = assembly.GetReferencedAssemblies();
       foreach (var referencedAssemblyName in referencedAssemblyNames)
       {
           var referencedAssembly = Assembly.Load(referencedAssemblyName);
           FillModules(referencedAssembly, scannedAssemblies);
       }

       foreach (var type in assembly.GetTypes())
       {
           //Skip types those are not Abp Module
           if (!AbpModuleHelper.IsAbpModule(type))
           {
               continue;
           }

           //Prevent multiple adding same module
           var moduleInfo = _modules.FirstOrDefault(m => m.Type == type);
           if (moduleInfo == null)
           {
               moduleInfo = AbpModuleInfo.CreateForType(type);
               _modules.Add(moduleInfo);
           }

           //Check for depended modules
           var dependedModuleTypes = moduleInfo.Instance.GetDependedModules();
           foreach (var dependedModuleType in dependedModuleTypes)
           {
               FillModules(dependedModuleType.Assembly, scannedAssemblies);
           }

           Logger.Debug("Loaded module: " + moduleInfo);
       }
   }

   private void SetDependencies()
   {
       foreach (var moduleInfo in _modules)
       {
           //Set dependencies according to assembly dependency
           foreach (var referencedAssemblyName in moduleInfo.Assembly.GetReferencedAssemblies())
           {
               var referencedAssembly = Assembly.Load(referencedAssemblyName);
               var dependedModuleList = _modules.Where(m => m.Assembly == referencedAssembly).ToList();
               if (dependedModuleList.Count > 0)
               {
                   moduleInfo.Dependencies.AddRange(dependedModuleList);
               }
           }

           //Set dependencies according to explicit dependencies
           var dependedModuleTypes = moduleInfo.Instance.GetDependedModules();
           foreach (var dependedModuleType in dependedModuleTypes)
           {
               AbpModuleInfo dependedModule;
               if (((dependedModule = _modules.FirstOrDefault(m => m.Type == dependedModuleType)) != null)
                   && (moduleInfo.Dependencies.FirstOrDefault(dm => dm.Type == dependedModuleType) == null))
               {
                   moduleInfo.Dependencies.Add(dependedModule);
               }
           }
       }
   }
}
```

3.9  `AbpModuleCollection` 进行依赖排序算法,主要看 `GetSortedModuleListByDependency()` 方法

```
public class AbpModuleCollection : List<AbpModuleInfo>
{
    /// <summary>
    /// Gets a reference to a module instance.
    /// </summary>
    /// <typeparam name="TModule">Module type</typeparam>
    /// <returns>Reference to the module instance</returns>
    public TModule GetModule<TModule>() where TModule : AbpModule
    {
        var module = this.FirstOrDefault(m => m.Type == typeof(TModule));
        if (module == null)
        {
            throw new AbpException("Can not find module for " + typeof(TModule).FullName);
        }

        return (TModule)module.Instance;
    }

    /// <summary>
    /// Sorts modules accorting to dependencies.
    /// If module A depends on mobule B, A comes after B in the returned List.
    /// </summary>
    /// <returns>Sorted list</returns>
    public List<AbpModuleInfo> GetSortedModuleListByDependency()
    {
        var orderedList = new List<AbpModuleInfo>();

        foreach (var moduleInfo in this)
        {
            var index = 0; //Order of this module (will be first module if there is no dependencies of it)

            //Check all modules and place this module after all it's dependencies
            if (!moduleInfo.Dependencies.IsNullOrEmpty())
            {
                for (var i = 0; i < orderedList.Count; i++)
                {
                    //Check for dependency
                    if (moduleInfo.Dependencies.Contains(orderedList[i]))
                    {
                        //If there is dependency, place after it
                        index = i + 1;
                    }
                }
            }

            //Insert module the right place in the list
            orderedList.Insert(index, moduleInfo);
        }

        return orderedList;
    }
}
```

4.0 `AbpStartupModule` 实现了抽象类`AbpModule`,里头主要加了一个通用的`Ioc注入器`

````
public class AbpStartupModule : AbpModule
{
    public override void PreInitialize(IAbpInitializationContext initializationContext)
    {
        base.PreInitialize(initializationContext);
        //IOC 管理器添加约定的基础注册器
        IocManager.Instance.AddConventionalRegisterer(new BasicConventionalRegisterer());
        //UnitWork--Aop方式
        UnitOfWorkRegistrer.Initialize(initializationContext);
    }

    public override void Initialize(IAbpInitializationContext initializationContext)
    {
        base.Initialize(initializationContext);
        //IOC 管理器按照程序集的上下文进行自动注册满足要求的类型
        IocManager.Instance.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly(),
            new ConventionalRegistrationConfig
            {
                InstallInstallers = false
            });
    }
}
````

4.1 `BasicConventionalRegisterer` 为通用的`Ioc注入器`,主要实现对通用类型的注入,他的执行默认在`AbpModule`的`Initialize`之后

```
//internal 类型可以说是Abp的内置Ioc注入器
internal class BasicConventionalRegisterer : IConventionalRegisterer
{
    public void RegisterAssembly(ConventionalRegistrationContext context)
    {
        //Transient
        context.IocContainer.Register(
            Classes.FromAssembly(context.Assembly)
                .IncludeNonPublicTypes()
                .BasedOn<ITransientDependency>()
                .WithService.Self()
                .WithService.DefaultInterfaces()
                .LifestyleTransient()
            );

        //Singleton
        context.IocContainer.Register(
            Classes.FromAssembly(context.Assembly)
                .IncludeNonPublicTypes()
                .BasedOn<ISingletonDependency>()
                .WithService.Self()
                .WithService.DefaultInterfaces()
                .LifestyleSingleton()
            );

        //Windsor Interceptors
        context.IocContainer.Register(
            Classes.FromAssembly(context.Assembly)
                .IncludeNonPublicTypes()
                .BasedOn<IInterceptor>()
                .WithService.Self()
                .LifestyleTransient()
            );
    }
}
```
