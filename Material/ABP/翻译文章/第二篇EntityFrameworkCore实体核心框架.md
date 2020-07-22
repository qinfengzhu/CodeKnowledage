#### EntityFramework Core 实体核心框架

abp.entityframeworkcore NuGet包是用来整合实体框架（EF）核心的ORM框架。安装此包后，我们还应该加上一个AbpEntityFrameworkCoreModule依赖属性。

> DbContext

EF核心需要定义来自DbContext类。在ABP，我们应该从abpdbcontext获得，如下所示：

```
public class MyDbContext : AbpDbContext
{
    public DbSet<Product> Products { get; set; }
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
}
```

构造函数必须得到 DbContextOptions<T> 如上面所示. 参数名称必须是选项。无法改变它，因为ABP将它作为匿名对象参数提供。

> Configuration(配置)

1. In Startup Class(启动类)

```
services.AddAbpDbContext<MyDbContext>(options =>
{
    options.DbContextOptions.UseSqlServer(options.ConnectionString);
});
```

对于非Web项目，我们将不会有一个启动类。在这种情况下，我们可以使用配置模块。abpefcore().adddbcontext方法在我们的模块配置DbContext类，如下图所示：

```
Configuration.Modules.AbpEfCore().AddDbContext<MyDbContext>(options =>
{
    options.DbContextOptions.UseSqlServer(options.ConnectionString);
});
```

我们使用给定的连接字符串，并使用SQL Server作为数据库提供程序。options.connectionstring是默认的连接字符串（见下一节）正常。但ABP采用iconnectionstringresolver去确定。因此，可以改变这种行为，并可以动态地确定连接字符串。行动通过adddbcontext时调用DbContext的实例将被创建。因此，您还可以有条件地返回不同的连接字符串。

2. In Module PreInitialize

```
public class MyEfCoreAppModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = GetConnectionString("Default");
        ...
    }
}
```
所以，你可以定义getconnectionstring方法只返回一个配置文件中的连接字符串（一般从appSettings.JSON）

> Repositories(仓库)

存储库用于从高层抽象数据访问。更多信息请参见库文档。

__Default Repositories(默认的仓库)__

abp.entityframeworkcore实现在你定义的所有实体的默认库DbContext。您不必创建存储库类以使用预定义的存储库方法。
当有默认的实现不能满足的时候，就得自己写实现IResponsitory接口 (默认继承自 EfCoreRepositoryBase泛型类)

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
        person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };
        _personRepository.Insert(person);
    }
}
```

__Custom Repositories（自定义的仓库）__

如果标准存储库方法还不够，您可以为实体创建自定义存储库类。

ASP.NET boilerplate 提供了基本的类`EfCoreRepositoryBase` 库很容易实现。实现`IRepository`接口，你可以获得这个类的任何库。但它是更好的创建你自己的类，扩展`EfRepositoryBase`。因此，你可以添加到你的库共享/普通的方法很容易。在所有的库类的例子：一个simpletasksystem应用

```
 public class SimpleTaskSystemRepositoryBase<TEntity, TPrimaryKey> :EfCoreRepositoryBase<SimpleTaskSystemDbContext, TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    public SimpleTaskSystemRepositoryBase(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    //add common methods for all repositories
}

public class SimpleTaskSystemRepositoryBase<TEntity> : SimpleTaskSystemRepositoryBase<TEntity, int>
    where TEntity : class, IEntity<int>
{
    public SimpleTaskSystemRepositoryBase(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    //do not add any method here, add to the class above (because this class inherits it)
}
```

注意，我们从efcorerepositorybase<simpletasksystemdbcontext，TEntity，tprimarykey>继承。这表明ASP.NET  Boilerplate 在我们的库中使用simpletasksystemdbcontext。

默认情况下，你指定的DbContext所有仓库（simpletasksystemdbcontext这个例子）是用efcorerepositorybase。你可以换到你自己的库库的类中添加属性到你autorepositorytypes DbContext如下所示：

```
[AutoRepositoryTypes(
    typeof(IRepository<>),
    typeof(IRepository<,>),
    typeof(SimpleTaskSystemEfRepositoryBase<>),
    typeof(SimpleTaskSystemEfRepositoryBase<,>)
)]
public class SimpleTaskSystemDbContext : AbpDbContext
{
    ...
}
```

__Custom Repository Example例子__

要实现自定义存储库，只需从上面创建的应用程序特定的基础存储库类中获得。

假设我们有一个任务实体可以分配给person（实体），任务有一个状态（新的，分配的，完成的）…等等）。我们需要写一个获得有条件的任务列表的自定义方法和属性assisgnedperson预取（包括）在一个单一的数据库查询。请参见示例代码：

```
public interface ITaskRepository : IRepository<Task, long>
{
    List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state);
}

public class TaskRepository : SimpleTaskSystemRepositoryBase<Task, long>, ITaskRepository
{
    public TaskRepository(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state)
    {
        var query = GetAll();

        if (assignedPersonId.HasValue)
        {
            query = query.Where(task => task.AssignedPerson.Id == assignedPersonId.Value);
        }

        if (state.HasValue)
        {
            query = query.Where(task => task.State == state);
        }

        return query
            .OrderByDescending(task => task.CreationTime)
            .Include(task => task.AssignedPerson)
            .ToList();
    }
}
```

我们首先定义itaskrepository然后实现它。getall()返回IQueryable <task>，然后我们可以添加一些过滤器使用给定的参数。最后，我们可以称tolist()得到任务列表。

你也可以在库的方法使用上下文对象到你的DbContext和直接使用实体框架的API。

注：定义域/核心层的自定义库的接口，实现其在EntityFrameworkCore的项目分层中的应用。因此，您可以在不引用EF内核的情况下从任何项目注入接口。

__Replacing Default Repositories__

即使你已经创建了一个taskrepository如上所示，任何类仍然可以注入IRepository <task,long>，并使用它。在大多数情况下这都不是问题。但是，如果你在你的自定义库基础之上的方法吗？说你已经超过了删除自定义库的方法添加一个自定义的行为上删除。如果一个类注入IRepository <task,long>，并使用默认的存储库中删除任务，自定义的行为将不工作。为了克服这个问题，您可以用如下所示的默认选项替换您的自定义存储库实现：

```
Configuration.ReplaceService<IRepository<Task, Guid>>(() =>
{
    IocManager.IocContainer.Register(
        Component.For<IRepository<Task, Guid>, ITaskRepository, TaskRepository>()
            .ImplementedBy<TaskRepository>()
            .LifestyleTransient()
    );
});
```

__Repository Best Practices(仓库最佳实践)__

在可能的地方使用默认存储库。您可以使用默认存储库，即使您有实体的自定义存储库（如果您使用标准存储库方法）。
始终为您的自定义存储库应用程序创建存储库基类，如上所述。
定义自定义库在领域层的接口（在启动模板核心项目），自定义库类的。如果你想从你的摘要EF核心域/应用项目entityframeworkcore。
