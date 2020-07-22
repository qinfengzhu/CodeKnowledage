#### Entity Framework Integration-实体框架集成

ASP.NET样板(ASP.NET Boilerplate)可以与任何ORM工作框架。它具有内置的集成EntityFramework。本文档将解释如何使用EntityFramework ASP.NET样板(ASP.NET Boilerplate)。假定你已经熟悉了基本的EntityFramework。

> DbContext

你知道的，和EntityFramework一起工作，你应该定义一个应用程序的DbContext类。例DbContext如下所示

```
public class SimpleTaskSystemDbContext : AbpDbContext
{
    public virtual IDbSet<Person> People { get; set; }
    public virtual IDbSet<Task> Tasks { get; set; }
    public SimpleTaskSystemDbContext()
        : base("Default")
    {
    }

    public SimpleTaskSystemDbContext(string nameOrConnectionString)
        : base(nameOrConnectionString)
    {
    }

    public SimpleTaskSystemDbContext(DbConnection existingConnection)
        : base(existingConnection, false)
    {
    }

    public SimpleTaskSystemDbContext(DbConnection existingConnection, bool contextOwnsConnection)
        : base(existingConnection, contextOwnsConnection)
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>().ToTable("StsPeople");
        modelBuilder.Entity<Task>().ToTable("StsTasks").HasOptional(t => t.AssignedPerson);
    }
}
```

这是一个常规的DbContext类除以下规则：

1. 这是得到abpdbcontext替代 DbContext
2. 它应该有构造函数作为上面的示例(构造函数形参名称也应该相同)
3. 默认构造函数通过“默认”基础低音的连接字符串。因此，预计“默认”的web.config/app.config文件命名连接字符串。这个构造函数不是由ABP使用的，而是由EF命令行迁移工具命令（如更新数据库）使用的。
构造函数获取nameorconnectionstring采用ABP在运行时通过连接字符串的名称。
让existingconnection构造函数可用于单元测试并不是直接使用ABP。
构造函数获取existingconnection和contextownsconnection的ABP单数据库的多个DbContext场景用来分享相同的连接和交易dbcontexteftransactionstrategy时使用（见交易管理部分）。
4. EF框架可以类映射到数据库表中的一个常规的方式。你甚至不需要做任何配置，除非你做一些定制的东西。在本例中，我们将实体映射到不同的表中。默认情况下，任务实体映射到任务表。但我们把它换成ststasks表。我不喜欢使用数据注释属性来配置它，而是倾向于使用流畅的配置。你可以选择你喜欢的方式。


> Repositories(仓库)

存储库用于从高层抽象数据访问。

__Default Repositories__

abp.entityframework实现在你定义的所有实体的默认库DbContext。您不必创建存储库类以使用预定义的存储库方法。例子:

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

ASP.NET的模板提供了一个基类库efrepositorybase实现容易。实施IRepository接口，你可以从这个类派生你的库。但它更好地创建自己的基地班extens EfRepositoryBase。因此，您可以将共享/公共方法添加到存储库中，或者轻松地覆盖现有的方法。例基类的所有simpletasksystem应用库：

```
//Base class for all repositories in my application
public class SimpleTaskSystemRepositoryBase<TEntity, TPrimaryKey> : EfRepositoryBase<SimpleTaskSystemDbContext, TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    public SimpleTaskSystemRepositoryBase(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    //add common methods for all repositories
}

//A shortcut for entities those have integer Id
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

注意，我们从EfrepositoryBase<SimpleTaskSystemDbcontext,TEntity,TPrimarykey>。这是ASP.NET的样板在我们的库使用SimpleTaskSystemDbcontext。

默认情况下，你指定的DbContext所有仓库（SimpleTaskSystemDbcontext这个例子）是用EfRepositoryBase。你可以换到你自己的库库类的属性你addingautorepositorytypes DbContext如下所示：

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

__Custom Repository Example__

要实现自定义存储库，只需从上面创建的应用程序特定的基础存储库类中获得。

 假设我们有一个任务实体可以分配给一个人（实体），任务有一个状态（新的，分配的，完成的）…等等）。我们需要写一个获得有条件的任务列表的自定义方法和属性assisgnedperson预取（包括）在一个单一的数据库查询。请参见示例代码：

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

 > Transaction Management(事务管理)

 ASP.NET的模板有一个内置的工作单元的系统来管理数据库连接和交易。实体框架有不同的事务管理方法。ASP.NET样板使用环境事务处理方法在默认情况下，但也有一个内置的DbContext API实现交易。如果你想切换到DbContext事务API，你可以配置它在你喜欢的方法分发模块：

```
 Configuration.ReplaceService<IEfTransactionStrategy, DbContextEfTransactionStrategy>(DependencyLifeStyle.Transient);
```

记得添加“using Abp.Configuration.Startup；“你的代码文件，可以使用replaceservice通用的扩展方法。
此外，你应该在DbContext构造函数DbContext部分描述本文件。
