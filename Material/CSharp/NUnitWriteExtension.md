### 对NUnit的方法进行扩展,方便书写

```
public static clas UnitTestExtensions
{
    public static void ShouldEqual(this object actual,object expected)
    {
        Assert.AreEqual(expected,actual);
    }

    public static void ShouldBeTheSameAs(this object actual,object expected)
    {
        Assert.AreSame(expected,actual);
    }

    public static void ShouldBeNull(this object actual)
    {
        Assert.IsNull(actual);
    }

    public static void ShouldNotBeNull(this object actual)
    {
        Assert.IsNotNull(actual);
    }

    public static void ShouldBeFalse(this bool b)
    {
        Assert.IsFalse(b);
    }

    public static void ShouldBeTrue(this bool b)
    {
        Assert.IsTrue(b);
    }

    public static Exception ShouldBeThrownBy(this Type exceptionType,TestDelegate code)
    {
        return Assert.Throws(exceptionType,code);
    }

    public static T ShouldBe<T>(this object actual)
    {
        Assert.IsInstanceOf<T>(actual);
        return (T)actual;
    }
}
```
