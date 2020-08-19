[TOC]

Abp 可以使用任何O/RM框架。它与EntityFramework具有内置集成。本文档将解释如何将EntityFramework与Abp集成。假设您已经对EntityFramework有了基本的了解。

## Nuget包

Abp.EntityFramework

## 数据库上下文(DbContext)

如您所知，要使用EntityFramework，您应该为应用程序定义一个DbContext类。DbContext示例如下所示：
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

它是一个常规的DbContext类，但以下规则除外：

* 它是从AbpDbContext而不是DbContext派生的。
* 它应该具有与上面示例相同的构造函数（构造函数参数名也应该相同）。解释
1. Default constructor将“Default”作为连接字符串传递给base bass。因此，它期望在web.config/app.config文件。这个构造函数不被ABP使用，而是由EF命令行迁移工具命令（比如updatedatabase）使用。
2. 构造函数获取nameOrConnectionString被ABP用于在运行时传递连接名称或字符串。
3. 构造函数get existingConnection可用于单元测试，不能直接由ABP使用。
4. 构造函数获取existingConnection，当使用dbcontextextransactionstrategy时，ABP在单个数据库多个dbcontext场景中使用contextOwnsConnection来共享相同的连接和事务（）。

EntityFramework可以用传统的方式将类映射到数据库表。你甚至不需要做任何配置，除非你做一些定制的东西。在这个例子中，我们将实体映射到不同的表。默认情况下，任务实体映射到任务表。但我们把它改成了StsTasks表。我宁愿使用fluent配置，而不是用数据注释属性来配置它。你可以选择你喜欢的方式。

## 仓储

存储库用于从更高层抽象数据访问。有关详细信息，请参阅存储库文档。

### 默认仓储

Abp.EntityFramework为DbContext中定义的所有实体实现默认存储库。您不必创建存储库类来使用预定义的存储库方法。例子：

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
PersonapService构造函数注入IRepository<Person>并使用Insert方法。通过这种方式，您可以轻松地注入IRepository<TEntity>（或IRepository<TEntity，TPrimaryKey>）并使用预定义的方法。

### 自定义仓储

如果标准存储库方法还不够，可以为实体创建自定义存储库类。

特定于应用程序的基本存储库类

ASP.NET Boilerplate  提供了一个基类EfRepositoryBase来轻松实现存储库。要实现IRepository接口，只需从该类派生存储库。但是最好创建自己的基类来扩展EfRepositoryBase。因此，您可以将共享/通用方法添加到存储库中，或轻松覆盖现有方法。SimpleTaskSystem应用程序存储库的示例基类：
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
请注意，我们是从EfRepositoryBase<SimpleTaskSystemDbContext，tenty，TPrimaryKey>继承的。此声明ASP.NET在我们的存储库中使用SimpleTaskSystemDbContext的样板。

默认情况下，在给定的系统上下文中使用SimpleDatabase（DBEstoryAll）在SimpleDatabase中实现。通过向DbContext添加AutoRepositoryTypes属性，可以将其替换为自己的存储库基存储库类，如下所示：

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
自定义存储库示例

要实现自定义存储库，只需从上面创建的特定于应用程序的基本存储库类派生即可。

假设我们有一个任务实体可以分配给一个人（实体），任务有一个状态（new，assigned，completed。。。等等）。我们可能需要编写一个自定义方法来获取任务列表，其中包含一些条件，并且在单个数据库查询中预取（包含）了assignedPerson属性。参见示例代码：

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
我们首先定义了ITaskRepository，然后实现了它。GetAll（）返回IQueryable<Task>，然后我们可以使用给定的参数添加一些Where过滤器。最后，我们可以调用ToList（）来获取任务列表。

您还可以在存储库方法中使用Context对象来访问DbContext并直接使用实体框架api。

注意：在域/核心层定义自定义存储库接口，在分层应用的EntityFramework项目中实现。因此，您可以从任何项目注入接口，而无需引用EF。

### 仓库最佳实践

* 尽可能使用默认存储库。即使您有实体的自定义存储库，也可以使用默认存储库（如果您将使用标准存储库方法）。
* 始终为自定义存储库的应用程序创建存储库基类，如上所述。
* 定义域层中自定义存储库的接口（.startup template中的Core project），定义.EntityFramework项目中的自定义存储库类（如果要从域/应用程序中抽象EF）。

## 事务管理(Transaction Management)

ASP.NET Boilerplate有一个内置的工作单元系统来管理数据库连接和事务。实体框架有不同的事务管理方法。ASP.NET默认情况下，样板文件使用环境事务处理范围方法，但也有DbContext事务API的内置实现。如果要切换到DbContext事务API，可以在模块的PreInitialize方法中进行如下配置：

```
Configuration.ReplaceService<IEfTransactionStrategy, DbContextEfTransactionStrategy>(DependencyLifeStyle.Transient);
```
记住加上“使用Abp.Configuration.启动“；”以使您的代码文件能够使用ReplaceService泛型扩展方法。

此外，您的DbContext应该具有本文档的DbContext部分中描述的构造函数。

## 数据存储

因为ASP.NETBoilerplate与EntityFramework具有内置集成，它可以与EntityFramework支持的数据存储一起工作。我们的免费启动模板是为与SQLServer一起工作而设计的，但您可以修改它们以使用不同的数据存储。

例如，如果您想使用MySql，请参考本[文档](EF-MySql集成.md)
