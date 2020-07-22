#### ApplicationServices 应用服务

应用程序服务用于将域逻辑暴露到表示层中。应用服务是从表现层与DTO（数据传输对象）为参数，使用域对象执行一些具体的业务逻辑，并返回一个DTO返回给表现层。因此，表示层与域层完全隔离。在理想的分层应用程序中，表示层从不直接使用域对象。

> IApplicationService Interface (应用服务接口)

在ASP.NET Boilerplate框架中,一个应用服务应该继承自接口 `IApplicationService`,如下

```
public interface IPersonAppService : IApplicationService
{
    void CreatePerson(CreatePersonInput input);
}
```

接口 `IPersonAppService` 有且仅有一个方法,当需要创建一个人信息的时候，它会被表示层(UI)层调用。`CreatePersonInput` 是一个`DTO`对象,如下所示

```
public class CreatePersonInput
{
    [Required]
    public string Name { get; set; }
    public string EmailAddress { get; set; }
}
```

接口 `IPersionAppService`的具体实现如下:

```
public class PersonAppService : IPersonAppService
{
    private readonly IRepository<Person> _personRepository;

    public PersonAppService(IRepository<Person> personRepository)
    {
        _personRepository = personRepository;
    }

    public void CreatePerson(CreatePersonInput input)
    {
        var person = _personRepository.FirstOrDefault(p => p.EmailAddress == input.EmailAddress);
        if (person != null)
        {
            throw new UserFriendlyException("There is already a person with given email address");
        }

        person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };
        _personRepository.Insert(person);
    }
}
```

这里有几个重要点说明:

1. `PersonAppService` 使用了 `IRepository<Person>`来对数据库进行操作，使用构造方法注入.

2. `PersonAppService` 实现了 `IApplicationService`,它会自动的被加入到ASP.NET Boilerplate的依赖注入框架中，然后它就可以被其它类注入使用

3. `CreatePerson` 方法接受 `CreatePersonInput` 参数,`CreatePersonInput` 作为 `DTO` 会自动的被 ASP.NET Boilerplate 进行验证

> ApplicationService 类

应用服务应该实现`IApplicationService`接口以上的声明。另外，还可以实现应用服务的基类。因此，`IApplicationService`本质上是实现。同时，应用服务类的一些基本功能，使得日志，本地化等等…建议创建您的应用程序服务，扩展应用服务类特殊的基类。因此，您可以为所有应用程序服务添加一些公共功能。下面显示了一个示例应用程序服务类

```
public class TaskAppService : ApplicationService, ITaskAppService
{
    public TaskAppService()
    {
        LocalizationSourceName = "SimpleTaskSystem";
    }

    public void CreateTask(CreateTaskInput input)
    {
        //Write some logs (Logger is defined in ApplicationService class)
        Logger.Info("Creating a new task with description: " + input.Description);

        //Get a localized text (L is a shortcut for LocalizationHelper.GetString(...), defined in ApplicationService class)
        var text = L("SampleLocalizableTextKey");

        //TODO: Add new task to database...
    }
}
```

你可以在它的构造函数有一个基类定义了`LocalizationSourceName`。因此，不要为所有服务类重复它。有关此主题的更多信息，请参见日志和本地化文档。

> CrudAppService 与 AsyncCrudAppService 类

如果你需要创建一个应用程序服务，将创建，更新，删除，得到的，对于一个特定的实体获得的方法，你可以从`CrudAppService`（或`AsyncCrudAppService`如果你想创建异步方法）的类来创建更容易。`CrudAppService`基类是通用的，得到了相关的实体和DTO类型作为泛型参数是可扩展的，允许你覆盖的功能时，你需要定制。

1. 简单的 CRUD Application Service 实例 ,假设我们定义 `Task` 实体(`Entity`)类如下:

```
public class Task : Entity, IHasCreationTime
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreationTime { get; set; }
    public TaskState State { get; set; }
    public Person AssignedPerson { get; set; }
    public Guid? AssignedPersonId { get; set; }
    public Task()
    {
        CreationTime = Clock.Now;
        State = TaskState.Open;
    }
}
```
接着我们创建一个 `DTO` 对应上面的 `Entity`

```
[AutoMap(typeof(Task))]
public class TaskDto : EntityDto, IHasCreationTime
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreationTime { get; set; }
    public TaskState State { get; set; }
    public Guid? AssignedPersonId { get; set; }
    public string AssignedPersonName { get; set; }
}
```

`AutoMap` 标记,在`Entity`与`DTO`会自动添加一个映射配置,我们可以添加一个 `Application Service` 如下:

```
public class TaskAppService : AsyncCrudAppService<Task, TaskDto>
{
    public TaskAppService(IRepository<Task> repository)
        : base(repository)
    {
    }
}
```

我们注入仓库交给基类（我们可以从`CrudAppService`继承如果我们想创建同步方法代替异步方法）。这就是全部`TaskAppService`现在有简单的CRUD方法。如果您想为应用程序服务定义一个接口，您可以创建如下所示的接口：

```
public interface ITaskAppService : IAsyncCrudAppService<TaskDto>
{        
}
```

注意，`IAsyncCrudAppService`没有实体（任务）作为泛型参数。因为实体与实现有关，不应该包含在公共接口中。现在，我们可以实现对`TaskAppService`类`ITaskAppService`接口：

```
public class TaskAppService : AsyncCrudAppService<Task, TaskDto>, ITaskAppService
{
    public TaskAppService(IRepository<Task> repository)
        : base(repository)
    {
    }
}
```

> Customize CRUD Application Services(定制CRUD应用服务)

1. Getting List (获取列表)

CRUD应用服务得到PagedAndSortedResultRequestDto为getAll方法作为默认参数，它提供了可选的排序和分页参数。但你可能要添加另一个参数的获得方法。例如，您可能需要添加一些自定义过滤器。在这种情况下，你可以创建一个DTO为getAll方法。例子:

```
public class GetAllTasksInput : PagedAndSortedResultRequestDto
{
    public TaskState? State { get; set; }
}
```
我们从PagedAndSortedResultRequestInput（这是不需要的，但要使用分页和排序的参数形式的基类），添加一个可选的状态属性的状态滤波任务。现在，我们应该改变`TaskAppService`为了应用自定义过滤器：

```
public class TaskAppService : AsyncCrudAppService<Task, TaskDto, int, GetAllTasksInput>
{
    public TaskAppService(IRepository<Task> repository)
        : base(repository)
    {
    }

    protected override IQueryable<Task> CreateFilteredQuery(GetAllTasksInput input)
    {
        return base.CreateFilteredQuery(input)
            .WhereIf(input.State.HasValue, t => t.State == input.State.Value);
    }
}
```

首先，我们增加了`GetAllTasksInput`为第四个泛型参数`AsyncCRUDAppService`类（第三个是PK型的实体）。然后超越`CreateFilteredQuery`方法应用自定义过滤器。该方法是`AsyncCRUDAppService`类扩展点（如果是一个扩展方法和简化条件过滤。但实际上我们做的是简单的过滤一个IQueryable）

注意：如果您已经创建了应用程序服务接口，那么您也需要向该接口添加相同的泛型参数

2. Create and Update (添加与更新)

注意，我们使用的是同样的DTO（TaskDTO）获取、创造和更新的任务，这可能不是现实生活中的应用很好。因此，我们可能需要自定义创建和更新DTOs。让我们开始创建`CreateTaskInput`类

```
[AutoMapTo(typeof(Task))]
public class CreateTaskInput
{
    [Required]
    [MaxLength(Task.MaxTitleLength)]
    public string Title { get; set; }
    [MaxLength(Task.MaxDescriptionLength)]
    public string Description { get; set; }
    public Guid? AssignedPersonId { get; set; }
}
```

添加一个 UpdateTaskInput DTO 定义

```
[AutoMapTo(typeof(Task))]
public class UpdateTaskInput : CreateTaskInput, IEntityDto
{
    public int Id { get; set; }
    public TaskState State { get; set; }
}
```
我想从`CreateTaskInput`包括更新操作的所有属性（但你可能需要不同的）。实现认同（or IEntity<PrimaryKey> for different PK than int）是必须的，因为我们需要知道哪些实体正在更新。最后，我添加了一个额外的属性，状态，他们不属于`CreateTaskInput`的属性。

现在，我们可以使用这些DTO类作为`AsyncCRUDAppService`类泛型参数，如下图所示：

```
public class TaskAppService : AsyncCrudAppService<Task, TaskDto, int, GetAllTasksInput, CreateTaskInput, UpdateTaskInput>
{
    public TaskAppService(IRepository<Task> repository)
        : base(repository)
    {
    }

    protected override IQueryable<Task> CreateFilteredQuery(GetAllTasksInput input)
    {
        return base.CreateFilteredQuery(input)
            .WhereIf(input.State.HasValue, t => t.State == input.State.Value);
    }
}
```

3. Other Method Arguments(其他方法的参数)

`AsyncCrudAppService`可以得到更一般的参数如果你想自定义输入DTOs和删除方法。此外，基类的所有方法都是虚拟的，因此您可以重写它们以自定义行为


4. CRUD Permissions（CRUD权限）

你可能需要授权你的CRUD方法。有预先定义的权限属性可以设置：`GetPermissionName`，`GetAllPermissionName`，`CreatePermissionName`，`UpdatePermissionName`和`DeletePermissionName`。基础的CRUD类自动检查权限，如果你让他们。您可以在构造函数中设置它，如下所示

```
public class TaskAppService : AsyncCrudAppService<Task, TaskDto>
{
    public TaskAppService(IRepository<Task> repository)
        : base(repository)
    {
        CreatePermissionName = "MyTaskCreationPermission";
    }
}
```

或者，你可以覆盖相应权限检查方法手动检查权限：`CheckGetPermission()`，`CheckGetAllPermission()`，`CheckCreatePermission()`，`CheckUpdatePermission()`，`CheckDeletePermission()`。在默认的情况下，他们的所有调用checkPermission（…）与相关权限名称简单地调用`IPermissionChecker`法。授权（…）方法


> Unit of Work (事务工作单元)

应用服务的方法是通过在 ASP.NET Boilerplate 默认工作单元。因此，任何应用程序服务方法都是事务性的，并在方法结束时自动保存所有数据库更改

> Lifetime of an Application Service (一个应用服务的生命周期)

所有应用程序服务实例都是暂时的。它意味着，每个使用都实例化它们。
