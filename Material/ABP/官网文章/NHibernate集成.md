[TOC]

ASP.NET Boilerplate 可以与任何O/RM框架一起工作。它与NHibernate有内置的集成。本文档将解释如何将NHibernate与ASP.NET样板。假设你已经对NHibernate有了基本的了解。

## Nuget 包

 ASP.NET Boilerplate 与 NHibernate 集成的包为 Abp.NHibernate。你应该把它添加到你的应用程序中。最好在应用程序中的独立程序集（dll）中实现NHibernate，并依赖于该程序集中的那个包。

 ## 配置(Configuration)

 要开始使用NHibernate，您应该在模块的预初始化中配置它。

 ```
[DependsOn(typeof(AbpNHibernateModule))]
public class SimpleTaskSystemDataModule : AbpModule
{
    public override void PreInitialize()
    {
        var connStr = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        Configuration.Modules.AbpNHibernate().FluentConfiguration
            .Database(MsSqlConfiguration.MsSql2008.ConnectionString(connStr))
            .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()));
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
 ```
AbpNHibernateModule module provides base functionality and adapters to make NHibernate work with ASP.NET Boilerplate.
AbpNHibernateModule模块提供基本功能和适配器，使NHibernate能够工作ASP.NET Boilerplate。

### 实体映射(Entity Mapping)

在上面的这个示例配置中，我们已经使用当前程序集中的所有映射类顺利地进行了映射。映射类的示例如下所示：

```
public class TaskMap : EntityMap<Task>
{
    public TaskMap()
        : base("TeTasks")
    {
        References(x => x.AssignedUser).Column("AssignedUserId").LazyLoad();

        Map(x => x.Title).Not.Nullable();
        Map(x => x.Description).Nullable();
        Map(x => x.Priority).CustomType<TaskPriority>().Not.Nullable();
        Map(x => x.Privacy).CustomType<TaskPrivacy>().Not.Nullable();
        Map(x => x.State).CustomType<TaskState>().Not.Nullable();
    }
}
```

EntityMap是扩展自ClassMap<T>，自动映射Id属性并在构造函数中获取表名。因此，我从中派生并使用FluentNHibernate映射其他属性。当然，您可以直接从ClassMap派生，可以使用FluentNHibernate的完整API，也可以使用NHibernate的其他映射技术（比如映射XML文件）。

## 仓储

存储库用于从更高层抽象数据访问。有关详细信息，请参阅存储库文档。

### 默认实现

Abp.NHibernate 包为应用程序中的实体实现默认存储库。不必为实体创建存储库类，就可以使用预定义的存储库方法。例子：

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
PersonapService构造函数注入IRepository<Person>并使用Insert方法。通过这种方式，您可以轻松地注入IRepository<TEntity>（或IRepository<TEntity，TPrimaryKey>）并使用预定义的方法。有关所有预定义方法的列表，请参阅存储库文档。

### 自定义仓储

如果您想添加一些自定义方法，您应该首先将其添加到存储库接口（作为最佳实践），然后在存储库类中实现。ASP.NET Boilerplate 提供了一个基类NhRepositoryBase来轻松实现存储库。要实现IRepository接口，只需从该类派生存储库。

假设我们有一个任务实体可以分配给一个人（实体），任务有一个状态（new，assigned，completed。。。等等）。我们可能需要编写一个自定义方法来获取具有某些条件的任务列表，并且在单个数据库查询中预取了assignedPerson属性。参见示例代码：

```
public interface ITaskRepository : IRepository<Task, long>
{
    List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state);
}

public class TaskRepository : NhRepositoryBase<Task, long>, ITaskRepository
{
    public TaskRepository(ISessionProvider sessionProvider)
        : base(sessionProvider)
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
            .Fetch(task => task.AssignedPerson)
            .ToList();
    }
}
```
GetAll（）返回IQueryable<Task>，然后我们可以使用给定的参数添加一些Where过滤器。最后，我们可以调用ToList（）来获取任务列表。

您还可以在存储库方法中使用Session对象来使用NHibernate的完整API。

注意：在域/核心层定义自定义存储库接口，在分层应用的NHibernate项目中实现。因此，您可以从任何项目注入接口，而无需引用NH。

特定于应用程序的基本存储库类

虽然您可以从ASP.NET Boilerplate 的NhRepositoryBase派生存储库，最好创建自己的基类来扩展NhRepositoryBase。因此，您可以轻松地将共享/公共方法添加到存储库中。例子：

```
//Base class for all repositories in my application
public abstract class MyRepositoryBase<TEntity, TPrimaryKey> : NhRepositoryBase<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    protected MyRepositoryBase(ISessionProvider sessionProvider)
        : base(sessionProvider)
    {
    }

    //add common methods for all repositories
}

//A shortcut for entities those have integer Id.
public abstract class MyRepositoryBase<TEntity> : MyRepositoryBase<TEntity, int>
    where TEntity : class, IEntity<int>
{
    protected MyRepositoryBase(ISessionProvider sessionProvider)
        : base(sessionProvider)
    {
    }

    //do not add any method here, add the class above (since this inherits it)
}

public class TaskRepository : MyRepositoryBase<Task>, ITaskRepository
{
    public TaskRepository(ISessionProvider sessionProvider)
        : base(sessionProvider)
    {
    }

    //Specific methods for task repository
}
```
