[TOC]

## 绑定动态 WEB API Controllers

> 下面文档是针对 Asp.net WEB Api ,如果你对 Asp.net Core 感兴趣,查看 [Asp.net core 文档](AspNetCore.md)

ASP.NET Boilerplate 自动为应用层(Application Layer) 生成 Asp.Net Web API 层(Asp.net Web Api Layer)。假设我们有一个应用程序服务，如下所示：

```
public interface ITaskAppService : IApplicationService
{
    GetTasksOutput GetTasks(GetTasksInput input);
    void UpdateTask(UpdateTaskInput input);
    void CreateTask(CreateTaskInput input);
}
```
我们希望将此服务公开为客户端的webapi控制器。ASP.NET Boilerplate可以自动和动态地为此应用程序服务创建一个Web API控制器，只需一行配置：

```
Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder.For<ITaskAppService>("tasksystem/task").Build();
```
就这些！在地址'/api/services/tasksystem/task'中创建了一个api控制器，所有方法现在都可供客户端使用。应该在模块的Initialize方法中进行此配置。

__ITaskAppService__ :  是我们希望用api控制器包装的应用程序服务。它不局限于应用程序服务，但这是传统的推荐方式。”tasksystem/task”是具有任意命名空间的api控制器的名称。您应该至少定义一个级别的命名空间，但可以定义更深层的命名空间，如“myCompany/myApplication/myNamespace1/myNamespace2/myServiceName”/api/services/'是所有动态web api控制器的前缀。所以，api控制器的地址将类似于'/api/services/tasksystem/task'，GetTasks方法的地址将是'/api/services/tasksystem/task/GetTasks'。方法名被转换成camelCase，因为在javascript世界中它是传统的。

### 所有方法(ForAll Method)

我们可能在一个应用程序中有许多应用程序服务，而一个接一个地构建api控制器可能是一项乏味而容易忘记的工作。DynamicApiControllerBuilper提供了一种方法，可以在一次调用中为所有应用程序服务生成web api控制器。例子：

```
Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
    .ForAll<IApplicationService>(Assembly.GetAssembly(typeof(SimpleTaskSystemApplicationModule)), "tasksystem")
    .Build();
```
ForAll方法是泛型的并接受接口。第一个参数是具有从给定接口派生的类的程序集。第二个是服务的名称空间前缀。假设给定程序集中有ITaskAppService和IPersonAppService。对于此配置，服务将是“/api/services/tasksystem/task”和“/api/services/tasksystem/person”。要计算服务名：服务和AppService后缀和I前缀被删除（对于接口）。此外，服务名称也转换为驼峰大小写。如果您不喜欢这个约定，有一个'WithServiceName'方法可以确定名称。另外，还有一个Where方法来过滤服务。如果您要为除少数应用程序服务之外的所有应用程序服务构建，那么这将非常有用。

### 重写查询所有方法(ForAll)

我们可以在ForAll方法之后重写配置。例子：

```
Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
    .ForAll<IApplicationService>(Assembly.GetAssembly(typeof(SimpleTaskSystemApplicationModule)), "tasksystem")
    .Build();

Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
    .For<ITaskAppService>("tasksystem/task")
    .ForMethod("CreateTask").DontCreateAction().Build();
```
在这段代码中，我们为程序集中的所有应用程序服务创建了动态webapi控制器。然后重写单个应用程序服务（ITaskAppService）的配置以忽略CreateTask方法。

ForMethods

我们可以用ForMethods方法在使用ForAll方法的同时更好地调整每个方法。例子：

```
Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
    .ForAll<IApplicationService>(Assembly.GetExecutingAssembly(), "app")
    .ForMethods(builder =>
    {
        if (builder.Method.IsDefined(typeof(MyIgnoreApiAttribute)))
        {
            builder.DontCreate = true;
        }
    })
    .Build();
```
在这个例子中，我使用了一个自定义属性（myignoreapitable）来检查所有方法，并且不为那些用该属性标记的方法创建动态webapi控制器操作。

### Http Verbs

默认情况下，所有方法都创建为POST。因此，客户机应该发送post请求，以便使用创建的webapi操作。我们可以用不同的方式改变这种行为。

WithVerb Method
我们可以用with动词来表示这样的方法：

```
Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
    .For<ITaskAppService>("tasksystem/task")
    .ForMethod("GetTasks").WithVerb(HttpVerb.Get)
    .Build();
```
HTTP Attributes
我们可以添加HttpGet，HttpPost。。。服务接口中方法的属性：

```
public interface ITaskAppService : IApplicationService
{
    [HttpGet]
    GetTasksOutput GetTasks(GetTasksInput input);

    [HttpPut]
    void UpdateTask(UpdateTaskInput input);

    [HttpPost]
    void CreateTask(CreateTaskInput input);
}
```
为了使用这些属性，我们应该添加对Microsoft.AspNet.WebApi.Core nuget包。

命名规则

您可以使用WithConventionalVerbs方法，而不是为每个方法声明HTTP，如下所示：

```
Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
    .ForAll<IApplicationService>(Assembly.GetAssembly(typeof(SimpleTaskSystemApplicationModule)), "tasksystem")
    .WithConventionalVerbs()
    .Build();
```
在这种情况下，HTTP动词由方法名前缀确定：

* __Get__ : 方法名以 'Get' 开头
* __Put__ : 方法名以 'Put' 或者 'Update' 开头
* __Delete__ : 方法名以 'Delete' 或者 'Remove' 开头
* __Post__ : 方法名以 'Post' 或  'Create' 或 'Insert' 开头
* __Patch__ : 方法名以 'Patch' 开头
* __Otherwise__ : Post 为默认的 HTTP verb.

我们可以为前面描述的特定方法重写它。

### API Explorer

默认情况下，所有动态webapi控制器对apiexplorer可见（例如，它们在Swagger中可用）。您可以使用fluent DynamicApiControllerBuilder API或使用下面定义的RemoteService属性来控制此行为。

### 远程服务标记(RemoteService Attribute)

您还可以使用任何接口或方法定义的RemoteService属性来启用/禁用（IsEnabled）动态API或API资源管理器设置（IsMetadataEnabled）。

## 动态Javascript 代理

您可以在javascript中通过ajax使用动态创建的webapi控制器。ASP.NET Boilerplate 还通过为动态webapi控制器创建动态javascript代理简化了这一点。因此，您可以像调用函数一样从javascript调用动态web api控制器的操作：

```
abp.services.tasksystem.task.getTasks({
    state: 1
}).done(function (result) {
    //use result.tasks here...
});
```

## Enable/Disable

如果使用了上面定义的ForAll方法，则可以使用RemoteService属性为服务或方法禁用它。在服务接口中而不是在服务类中使用此属性。

## 包装结果(Wrapping Results)

ASP.NET Boilerplate 通过AjaxResponse对象包装动态webapi操作的返回值。有关这种包装的更多信息，请参阅ajax文档。您可以为每个方法或每个应用程序服务启用/禁用包装。请参阅以下示例应用程序服务：

```
public interface ITestAppService : IApplicationService
{
    [DontWrapResult]
    DoItOutput DoIt(DoItInput input);
}
```
我们禁用了DoIt方法的包装。应该为接口而不是实现类声明此属性。



如果您想对客户端的确切返回值进行更多控制，则展开非常有用。尤其是，在使用无法使用的第三方客户端库时，可能需要禁用它ASP.NET样板的标准Ajax响应。在这种情况下，您还应该自己处理异常，因为异常处理将被禁用（DontWrapResult属性具有WrapOnError属性，可用于启用异常的处理和包装）。

## 关于参数绑定 (Parameter Binding)

ASP.NET Boilerplate在运行时创建Api控制器。所以，ASP.NETwebapi的模型和参数绑定用于绑定模型和参数。你可以阅读它的文档了解更多信息。

### FormUri 和 FormBody Attributes

FromUri和FromBody属性可以在服务接口中用于绑定的高级控制。

### DTO与基元类型

我们强烈建议使用dto作为应用程序服务和webapi控制器的方法参数。但是你也可以使用原始类型（比如string，int，bool。。。或者像int这样可以为null的类型？，布尔？…）作为服务参数。可以使用多个参数，但这些参数中只允许使用一个复杂类型参数（因为ASP.NET WebAPI）。
