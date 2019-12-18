### 为什么反射会很慢,怎么解决

1. 参考文章

[原文:Why is Reflection slow](https://mattwarren.org/2016/12/14/Why-is-Reflection-slow/)

[参考:透过委托让反射飞快](https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/)

[参考:表达式树快速完成反射功能](http://geekswithblogs.net/Madman/archive/2008/06/27/faster-reflection-using-expression-trees.aspx)

[参考:BenchmarkDotNet Documentation](https://fransbouma.github.io/BenchmarkDotNet/GettingStarted.htm)

[怎么写基准测试](HowtoWriteBenchmark.md)

2. 参考功能类库

[Emit便捷操作类库Sigil](https://github.com/kevin-montrose/Sigil)

[快速访问成员类库FastMember](https://github.com/christopherwithers/FastMember)

#### 使用基准测试来进行试验

`Mean` 的意思是 Arithmetic mean of all measurements 所有测量的算术平均值

`StdErr` 的意思是 Half of 99.9% confidence interval 99.9% 一半的置信度区间

`StdDev` 是所有测量的标准偏差

具体实施代码,引用了类库 `FastMember`,`Sigil`

```
/// <summary>
/// 用于测试的类
/// </summary>
public class User
{
    public User(string userName)
    {
        this.UserName = userName;
    }
    public string UserName { get; set; }
}

//核心测试类
public class PerformanceTest
{
    //reflection
    Type type;
    PropertyInfo property;
    User user = new User("xiaoming");

    //fast member
    TypeAccessor accessor;

    //Delegate
    Func<User, string> getUserNameDelegate;

    //Expression
    Func<object, object> getExpressionDelegateFunc;

    //Emit
    Func<object, string> getUserNameEmitFunc;
    [Setup]
    public void InitCode()
    {
        //reflection
        type = user.GetType();
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        property = type.GetProperty("UserName", bindingFlags);

        //fast member
        accessor = TypeAccessor.Create(type, true);

        //delegate
        getUserNameDelegate = (Func<User, string>)Delegate.CreateDelegate(
         typeof(Func<User, string>),
         property.GetGetMethod(true)
        );

        //Expression
        ParameterExpression instance = Expression.Parameter(typeof(object), "instance");

        UnaryExpression instanceCast = !property.DeclaringType.IsValueType ?
            Expression.TypeAs(instance, property.DeclaringType) :
            Expression.Convert(instance, property.DeclaringType);

        getExpressionDelegateFunc = Expression.Lambda<Func<object, object>>(
            Expression.TypeAs(
                Expression.Call(instanceCast, property.GetGetMethod(true)),
                typeof(object)),
            instance)
            .Compile();

        //Emit
        var getterEmitter = Emit<Func<object, string>>
            .NewDynamicMethod("GetUserNameProperty")
            .LoadArgument(0)
            .CastClass(type)
            .Call(property.GetGetMethod(true))
            .Return();
        getUserNameEmitFunc = getterEmitter.CreateDelegate();
    }

    /// <summary>
    /// Direct Value
    /// </summary>
    [Benchmark]
    public string GetValueByDirectProperty()
    {
        return user.UserName;
    }

    /// <summary>
    /// PropertyInfo ->Reflection
    /// </summary>
    [Benchmark]
    public string GetValueByReflection()
    {
        return (string)property.GetValue(user);
    }

    /// <summary>
    /// FastMember
    /// </summary>
    [Benchmark]
    public string GetValueByFastMember()
    {
        return (string)accessor[user, "UserName"];
    }

    /// <summary>
    /// Delegate
    /// 缺点: 必须为 Func《User, string》 否则异常，就是说第一个User与后面string 入参与出参必须明确
    /// [有趣深入文章见](https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/)
    /// </summary>
    [Benchmark]
    public string GetValueByDelegate()
    {
        return (string)getUserNameDelegate(user);
    }

    /// <summary>
    /// Expression->Compile->Delegate
    /// 优点:弥补了直接Delegate 的入参与出参必须明确的形式,使用Expression动态的Compile编译生成Func《object,object》
    /// [表达式树反射](http://geekswithblogs.net/Madman/archive/2008/06/27/faster-reflection-using-expression-trees.aspx)
    /// </summary>
    [Benchmark]
    public string GetValueByExpression()
    {
        return (string)getExpressionDelegateFunc(user);
    }

    /// <summary>
    /// IL->Emit->Delegate
    /// </summary>
    [Benchmark]
    public string GetValueByILEmit()
    {
        return getUserNameEmitFunc(user);
    }
}

//运行性能测试报告
[Test]
public void ReflectionGetProfrenceTest()
{
    var summary = BenchmarkRunner.Run<PerformanceTest>();

    Assert.AreEqual(string.Empty, summary);
}
```
