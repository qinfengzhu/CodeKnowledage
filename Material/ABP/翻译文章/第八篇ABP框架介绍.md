#### ABP框架介绍

> 介绍

我们正在根据不同的需求创建不同的应用程序。但是，至少在某种程度上一次又一次地执行通用和类似的结构。授权、验证、异常处理、日志记录、本地化、数据库连接管理、设置管理、审计日志记录是这些常见结构中的一些结构。同时，我们正在建设的建筑结构和最佳实践，如分层和模块化结构，领域驱动设计，依赖注入等。并尝试基于一些约定开发应用程序。

因为所有这些都是非常耗时和难以建立单独为每一个项目，许多公司创建私有的框架。他们正在开发新的应用程序，使用这些框架的bug更少。当然，并不是所有的公司都那么幸运。他们大多没有时间、预算和团队来开发这样的框架。即使他们有创建一个框架的可能性，也很难编写文档，培训开发人员并维护它。

ASP.NET Boilerplate（ABP）是一个开放源代码的，有据可查的应用框架开始发展为所有公司和开发商共同框架的主意！”它不仅仅是一个框架，而且还提供了一个基于领域驱动设计和最佳实践的强大的架构模型。


> 一个快速简单例子

让我们调查一个简单的类，看看ABP的好处：

```
public class TaskAppService : ApplicationService, ITaskAppService
{
    private readonly IRepository<Task> _taskRepository;

    public TaskAppService(IRepository<Task> taskRepository)
    {
        _taskRepository = taskRepository;
    }

    [AbpAuthorize(MyPermissions.UpdatingTasks)]
    public async Task UpdateTask(UpdateTaskInput input)
    {
        Logger.Info("Updating a task for input: " + input);

        var task = await _taskRepository.FirstOrDefaultAsync(input.TaskId);
        if (task == null)
        {
            throw new UserFriendlyException(L("CouldNotFoundTheTaskMessage"));
        }

        input.MapTo(task);
    }
}
```

这里，我们看到了一个示例应用程序服务方法。一个应用程序服务，在DDD中，由表示层直接使用来执行应用程序的用例。我们可以认为updatetask方法被JavaScript通过AJAX。让我们看看ABP在这里的一些好处：

依赖注入：ABP使用并提供强大且常规的DI基础设施。因为这个类是一个应用程序服务，所以它通常注册为DI容器作为临时（根据请求创建）。它可以简单地将所有的依赖性（如IRepository的<<task>>这样）。

仓储：ABP可以为每个实体的默认库（IRepository <task>这个例子）。默认存储库中有许多有用的方法，本例中使用FirstOrDefault。我们可以轻松地根据需要扩展默认存储库。文摘数据库和ORMs库简化了数据访问逻辑。

授权：ABP可以检查权限。它可以防止访问updatetask方法如果当前用户没有“更新任务”的权限或没有登录。它使用声明属性简化了授权，但也有额外的授权方式。

验证：ABP自动检查输入是否为空。它还基于标准数据注释属性和自定义验证规则验证输入的所有属性。如果请求无效，则抛出正确的验证异常。

审计日志：根据约定和配置，为每个请求自动保存用户、浏览器、IP地址、调用服务、方法、参数、调用时间、执行时间和其他信息。

工作单元(事务)：在ABP中，每个应用服务方法都假定为缺省工作单元。它会自动创建一个连接和交易的方法，在开始的开始。如果方法成功地完成了没有例外，那么交易的承诺和连接设置。即使此方法使用不同的存储库或方法，它们都是原子（事务）。在实体所有的改变都是自动保存在事务提交。因此，我们甚至不需要打电话_repository。更新（任务）的方法如下所示。

异常处理：我们几乎从不处理Web应用程序中ABP中的异常。默认情况下，所有异常都会自动处理。如果出现异常，ABP会自动记录并将正确的结果返回给客户机。例如，如果这是一个Ajax请求，它将一个JSON返回给客户机，表明发生了一个错误。如果隐藏从客户实际异常除非例外情况是userfriendlyexception用于此示例。它还理解并处理客户机上的错误，并向用户显示适当的消息。

日志：如您所见，我们可以使用基类中定义的日志记录器对象编写日志。log4net作为默认的但它的可变或可配置。

本地化：注意我们在抛出异常时使用了L方法。因此，它基于当前用户的文化自动本地化。当然，我们在某个地方定义couldnotfoundthetaskmessage（更多seelocalization文件）。

自动映射：在最后一行，我们使用ABP的信息以可拓方法地图输入属性的实体属性。它使用AutoMapper库执行映射。因此，我们可以根据命名规则轻松地将属性从一个对象映射到另一个对象。

动态Web API层：taskappservice实际上是一个简单的类（甚至不需要提供的应用服务）。我们通常编写一个包装Web API控制器来向JavaScript客户端公开方法。ABP在运行时自动执行。因此，我们可以直接从客户机中使用应用程序服务方法。

动态JavaScript Ajax代理：ABP创建JavaScript代理方法，这些方法使调用应用程序服务方法和调用客户机上的JavaScript方法一样简单。
在这样一个简单的类中，我们可以看到ABP的好处。所有这些任务通常需要显著的时间，但他们都是由总部自动处理。

> What Else(其他)

除了这个简单的示例之外，ABP提供了强大的基础设施和应用程序模型。这里，ABP的一些其他特性：

模块化：为构建可重用模块提供强大的基础设施。
数据过滤器：提供自动数据过滤来实现像软删除和多租户这样的模式。
多租户：它完全支持多租户，包括每个租户架构的单个数据库或数据库。
设置管理：提供一个强大的基础设施来获取/更改应用程序、租户和用户级设置。
单元和集成测试：它构建了可测试性。还提供基类来简化单元和集成测试。有关更多信息，请参见本文。

> Startup Templates (启动模板)

开始一个新的解决方案，创建层，安装NuGet包，创建一个简单的布局和菜单…所有这些都是耗费时间的东西。

ABP提供预构建的[启动模板](https://aspnetboilerplate.com/Templates)，使得启动一个新的解决方案变得容易得多。模板支持spa（单页应用程序）和MPA（多页MVC应用程序）体系结构。同时，我们可以用不同的形式。
