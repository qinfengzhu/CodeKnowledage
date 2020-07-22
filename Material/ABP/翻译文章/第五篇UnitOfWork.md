#### UnitOfWork 工作单元

连接和事务管理是使用数据库的应用程序中最重要的概念之一。何时打开连接，何时启动事务，如何处理连接等等。ASP.NET Boilerplate的工作单元管理数据库连接和事务系统。


> Conventional Unit Of Work Methods

```
public class PersonAppService : IPersonAppService
{
    private readonly IPersonRepository _personRepository;
    private readonly IStatisticsRepository _statisticsRepository;

    public PersonAppService(IPersonRepository personRepository, IStatisticsRepository statisticsRepository)
    {
        _personRepository = personRepository;
        _statisticsRepository = statisticsRepository;
    }

    public void CreatePerson(CreatePersonInput input)
    {
        var person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };
        _personRepository.Insert(person);
        _statisticsRepository.IncrementPeopleCount();
    }
}
```
在createperson方法，我们把 person repository和增加总的人使用统计库统计。在本例中，两个存储库共享相同的连接和事务，因为默认情况下应用服务(IApplicationService)方法是工作单元。ASP.NET Boilerplate打开一个数据库连接和启动一个事务进入createperson方法和提交事务结束的方法如果没有异常被抛出，回滚如果发生任何异常。这样，在CreatePerson，所有的数据库操作方法成为原子（工作单位）。

除了默认的工作常规单元类，你可以添加你自己的惯例在分发你模块像下面的方法：

```
Configuration.UnitOfWork.ConventionalUowSelectors.Add(type => ...);
```

> Controlling Unit Of Work

工作单元隐式地工作于上面定义的方法。在大多数情况下，您不必手动控制Web应用程序的工作单元。如果想在其他地方控制工作单元，可以显式地使用它。它有两种方法

1. 使用 `UnitOfWorkAttribute`

```
[UnitOfWork]
public void CreatePerson(CreatePersonInput input)
{
    var person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };
    _personRepository.Insert(person);
    _statisticsRepository.IncrementPeopleCount();
}
```

2. 使用 `IUnitOfWorkManager`, 使用 `IUnitOfWorkManager.Begin(...) ` 方法

```
public class MyService
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IPersonRepository _personRepository;
    private readonly IStatisticsRepository _statisticsRepository;

    public MyService(IUnitOfWorkManager unitOfWorkManager, IPersonRepository personRepository, IStatisticsRepository statisticsRepository)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _personRepository = personRepository;
        _statisticsRepository = statisticsRepository;
    }

    public void CreatePerson(CreatePersonInput input)
    {
        var person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };

        using (var unitOfWork = _unitOfWorkManager.Begin())
        {
            _personRepository.Insert(person);
            _statisticsRepository.IncrementPeopleCount();

            unitOfWork.Complete();
        }
    }
}
```

> Unit Of Work in Detail

1. 使 UnitOfWork 不可用

```
[UnitOfWork(IsDisabled = true)]
public virtual void RemoveFriendship(RemoveFriendshipInput input)
{
    _friendshipRepository.Delete(input.Id);
}
```

2. Non-Transactional Unit Of Work(非事务性工作单元)

工作单元是事务性的缺省（根据其性质）。因此，ASP.NET Boilerplate 开始/提交/回滚文件显式数据库事务。在某些特殊情况下，事务可能导致问题，因为它可能会锁定数据库中的某些行或表。在这种情况下，您可能希望禁用数据库级事务。UnitOfWork属性可以作为非事务性的构造函数的布尔值。使用示例：

```
[UnitOfWork(isTransactional: false)]
public GetTasksOutput GetTasks(GetTasksInput input)
{
    var tasks = _taskRepository.GetAllWithPeople(input.AssignedPersonId, input.State);
    return new GetTasksOutput
            {
                Tasks = Mapper.Map<List<TaskDto>>(tasks)
            };
}
```

我建议使用此属性为[ UnitOfWork（istransactional：false）]。我认为它更易读，更明确。但是，你可以使用[ UnitOfWork（false）]。

3. Automatically Saving Changes（自动保存更改）

```
[UnitOfWork]
public void UpdateName(UpdateNameInput input)
{
    var person = _personRepository.Get(input.PersonId);
    person.Name = input.NewName;
}
```

就这样，名字被改变了！我们甚至没有调用_personrepository.update方法。O／RM框架跟踪工作单元中所有实体的变化，并反映对数据库的更改。

`IRepository.GetAll() Method`

当你调用getall()仓库的方法，必须有一个开放的数据库连接，因为它返回IQueryable。这是因为延期执行IQueryable。它不除非你调用tolist()方法或在foreach循环使用IQueryable执行数据库查询（或者访问查询项目）。所以，当你调用tolist()方法、数据库连接必须活着


```
[UnitOfWork]
public SearchPeopleOutput SearchPeople(SearchPeopleInput input)
{
    //Get IQueryable<Person>
    var query = _personRepository.GetAll();

    //Add some filters if selected
    if (!string.IsNullOrEmpty(input.SearchedName))
    {
        query = query.Where(person => person.Name.StartsWith(input.SearchedName));
    }

    if (input.IsActive.HasValue)
    {
        query = query.Where(person => person.IsActive == input.IsActive.Value);
    }

    //Get paged result list
    var people = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
    return new SearchPeopleOutput { People = Mapper.Map<List<PersonDto>>(people) };
}
```

> Options（选项）

```
public class SimpleTaskSystemCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsolationLevel = IsolationLevel.ReadCommitted;
        Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
    }

    //...other module methods
}
```

> Methods 调用方式

您可以通过以下两种方式之一访问当前工作单元：

1. 你可以直接使用CurrentUnitOfWork属性，如果你的类是来源特性（(ApplicationService, DomainService, AbpController, AbpApiController.…等）

2. 你可以注入IUnitOfWorkManager 到你的任何类，只用 IUnitOfWorkManager.Current 属性

> SaveChanges（保存修改）

ASP.NET Boilerplate  保存最后一个工作单元所有的改变，你不需要做任何事。但是，有时您可能希望在工作单元的中间操作中保存对数据库的更改。一个例子使用可保存更改到新插入的ID在EntityFramework的实体。

你可以使用调用SaveChanges或现在的工作单位savechangesasync方法。

注意：如果当前工作单元是事务性的，那么如果异常发生，事务中的所有更改都会回滚，甚至保存更改。

> Events(事件)

 工作单元完成、失败和处理事件。您可以注册这些事件并执行所需的操作。例如，你可能想运行一些代码，当前的单位工作顺利完成。例子:

 ```
public void CreateTask(CreateTaskInput input)
{
    var task = new Task { Description = input.Description };

    if (input.AssignedPersonId.HasValue)
    {
        task.AssignedPersonId = input.AssignedPersonId.Value;
        _unitOfWorkManager.Current.Completed += (sender, args) => { /* TODO: Send email to assigned person */ };
    }

    _taskRepository.Insert(task);
}
 ```
