### 有点类似递归类的代码如下

```csharp
public abstract class StringSigned<T> where T:StringSigned<T>,new()
{
    private static T m_instance;
    private static object m_syncobject= new object();
    
    protected virtual string Splitor
    {
        get{return "_";}
    }

    public static T _
    {
        get
        {
            if(m_instance==null)
            {
                lock(m_syncobject)
                {                    
                    T t = new T();
                    string typeName = typeof(T).Name;
                    var properties = from prop in typeof(T).GetProperties()
                                     where prop.CanWrite && prop.CanRead
                                     let value = typeName.Replace(".", t.Splitor) + t.Splitor + prop.Name
                                     select new { prop, value };
                    properties.ToList().ForEach(p => p.prop.SetValue(t, p.value, null));
                    m_instance = t;
                }
            }
            return m_instance;
        }
    }
}
```

咋一看，什么鬼？怎么泛型类 `StringSigned<T>` 中的类型居然是它自己的实现类，看着上面的抽象类 `StringSigned<T>` 不禁仍不住写了下面的类看看，一探到底是怎么回事

```csharp
//这个就是按照泛型定义来着，与抽象类的继承来着
public class ComputerRoles:StringSigned<ComputerRoles>
{
    //超级管理员
    public string Administrator{get;set;}
    //匿名用户
    public string AnonymousUser{get;set;}
    //Mysql数据库管理员
    public string MySqlManager{get;set;}
    //IIS管理员
    public string IISManager{get;set;}
    //AspNet用户
    public string AspNet{get;set;}
}
```
具体使用呢? `ComputerRoles` 计算机用户类，我们已经定义好了，怎么调用呢？见下面单元测试

```csharp
[TestFixture]
public class ComputerRolesTest
{
    [Test]
    public void AdministratorTest()
    {
        string administrator = ComputerRoles._.Administrator;

        Assert.AreEqual("ComputerRoles_Administrator", administrator);
    }
}
```
### 分析这个类的原理

1. 抽象泛型类

泛型是一种开放类型，也就是在没有指定明确`T`类型的时候，编译器也不知道怎么给他分配内存，更不用说去调用了。
抽象类一般我们了解的是，它不能够被实例化。

2. 抽象泛型类的静态函数

泛型的类型被指定前是不能调用泛型内部的静态方法的。

3. 反射

从上面的类来看，就是获取类的可读可写属性，然后赋值，如果要提高效率，可以考虑用`Emit`或者`委托`或者`Expression构建委托`来做。

4. linq

这个就不多讲了,底层机制原理就是 `Expression` .