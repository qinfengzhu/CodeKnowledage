#### ABP框架系列之一(Entity-实体)

实体是DDD（领域驱动设计）的核心概念之一。Eric Evans把它描述为“一个没有被它的属性基本定义的对象，而是一个连续性和标识性的线程”。因此，实体拥有id并存储在数据库中。实体通常映射到关系数据库的表。

> Entity Class (实体类)

实体是从实体基类派生
```
public class Person:Entity
{
  public virtural string Name{ get; set; }
  public virtural DateTime CreationTime{ get; set; }
  public Person()
  {
    CreationTime=DateTime.Now;
  }
}
```

人员类定义为一个实体。它有两个属性。此外，实体类定义id属性。它是实体的主键。因此，所有实体的主键名称相同，它是id。

id（主键）的类型可以更改。这是int（Int32）默认。如果要将另一类型定义为id，则应显式声明如下所示：
```
public class Person : Entity<long>
{
    public virtual string Name { get; set; }
    public virtual DateTime CreationTime { get; set; }
    public Person()
    {
        CreationTime = DateTime.Now;
    }
}
```
另外，可以将其设置为字符串、GUID或其他内容。
实体类重写相等运算符（=）以轻松检查两个实体是否相等（它们的ID是相等的）。它还定义了istransient()方法检查它是否有一个ID或不。

> AggregateRoot Class (聚合根类)

“聚合是领域驱动设计中的一种模式。DDD聚合是一组域对象，可以作为单个单元处理。一个例子可能是一个订单和它的行项目，它们将是单独的对象，但是将订单（连同它的行项目）作为单个集合处理是有用的。”（Martin Fowler -参见完整描述）

虽然ABP不强制您使用聚合，但您可能希望在应用程序中创建聚合和聚合根。 ABP定义聚合根类，为了类扩展的实体创建一个聚集根实体

聚合根定义领域事件集合，通过聚合根类自动生成领域事件。这些事件是在当前工作单元完成之前自动触发的。事实上，任何实体可以通过继承 `IgeneratesDomainevents`接口自动生成领域的事件，但它是常见的（最佳实践），用聚合根自动生成领域事件。这就是为什么它的默认聚合根而不是实体类。

> Conventional Interfaces (接口约定)

在许多应用中，类似的实体属性（和数据库表的字段）是用于 CreationTime表明这个实体的创建。ASP.NET的模板提供了一些有用的接口，使这个共同的属性的明确和有表达力。此外，这为实现这些接口的实体提供了一种编码公共代码的方法。

1. Auditing (审计)
ihascreationtime使得它可以使用一个共同的属性的一个实体的创建时间信息。ASP.NET样板自动设置的创建时间为当前时间当实体插入到数据库中，只要实现这个接口

```
public interface IHasCreationTime
{
    DateTime CreationTime { get; set; }
}

//人员类能被重写如下通过实现 IHasCreationTime 接口
public class Person : Entity<long>, IHasCreationTime
{
    public virtual string Name { get; set; }

    public virtual DateTime CreationTime { get; set; }

    public Person()
    {
        CreationTime = DateTime.Now;
    }
}

//ICreationAudited扩展IHasCreationTime，增加 CreatorUserId属性
public interface ICreationAudited : IHasCreationTime
{
    long? CreatorUserId { get; set; }
}
```

2. Soft Delete(软删除)

软删除是一种常用模式，用于将实体标记为已删除，而不是实际从数据库中删除它。如，你可能不想从数据库中删除用户因为它有许多关系到其他tables.isoftdelete接口用于此目的的：

```
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
```

ASP.NET样板实现软删除模式开箱。当软删除实体被删除，ASP.NET样板检测，防止删除、设置isDeleted作为真正的更新数据库实体。而且，它没有获得（选择）软删除实体数据库，自动过滤。

如果你使用软删除，则可能希望在删除实体和谁删除它时记录信息。你可以实现ideletionaudited接口，如下图所示：

```
public interface IDeletionAudited : ISoftDelete
{
    long? DeleterUserId { get; set; }

    DateTime? DeletionTime { get; set; }
}
```

当你注意到 ideletionaudited扩展isoftdelete 。ASP.NET样板自动设置这些属性，当一个实体被删除。

如果你想实现所有审计接口（创建，修改和删除）一个实体，可以直接实现ifullaudited因为它继承了所有：

```
public interface IFullAudited : IAudited, IDeletionAudited
{
}
```

作为一种快捷方式，你可以从fullauditedentity得到你的实体类，实现了所有。

注1：所有审计接口和类都定义导航属性用户实体的通用版本（如 ICreationAudited<TUser> and FullAuditedEntity<TPrimaryKey, TUser>）。
注2：同时，他们都有一个aggregateroot版，如AuditedAggregateRoot。

> Active ,Passive Entities (激活、闲置实体)

一些实体需要标记为激活或闲置。然后你可以对实体的激活或闲置状态采取行动。你可以实现ipassivable接口就是这个原因了。它定义了IsActive属性。

如果你的实体将第一次创作是激活的，你可以设置 IsActive = true在构造函数。

这是不同于软删除（isDeleted）。如果一个实体被软删除，它不能从数据库中检索（ABP防止它作为默认）。但是，对于激活/闲置实体，完全由您来控制获取实体。

> Entity Change Events(实体改变事件)

ASP.NET样板自动触发某些事件时，一个实体的插入、更新或删除。因此，您可以注册这些事件并执行您需要的任何逻辑。有关详细信息，请参阅事件总线文档中的预定义事件部分。

> IEntity Interfaces(IEntity 接口)

实际上，实体类实现的接口（and Entity<TPrimaryKey> implements IEntity<TPrimaryKey>）。如果不希望从实体类派生，则可以直接实现这些接口。其他实体类也有相应的接口。但这不是建议的方法，除非您有很好的理由不从实体类派生出来。

> IExtendableObject Interface(IExtendableObject接口)

ASP.NET的模板提供了一个简单的接口，iextendableobject，很容易联想到一个实体任意name-value数据。考虑这个简单的实体：

```
public class Person : Entity, IExtendableObject
{
    public string Name { get; set; }
    public string ExtensionData { get; set; }
    public Person(string name)
    {
        Name = name;
    }
}
```

IExtendableObject 定义ExtensionData  string 属性用来存储json格式的name-value objects,如：

```
var person = new Person("John");
person.SetData("RandomValue", RandomHelper.GetRandom(1, 1000));
person.SetData("CustomData", new MyCustomObject { Value1 = 42, Value2 = "forty-two" });
```

我们可以使用任何类型的对象作为值SetData方法。当我们使用上面的代码，extensiondata如下：

```
{"CustomData":{"Value1":42,"Value2":"forty-two"},"RandomValue":178}
```

用GetData取得任意的值

```
var randomValue = person.GetData<int>("RandomValue");
var customData = person.GetData<MyCustomObject>("CustomData");
```

虽然这种技术在某些情况下非常有用（当您需要提供向实体动态添加额外数据的能力时），但您通常应该使用常规属性。这种动态用法不是类型安全的和显式的。
