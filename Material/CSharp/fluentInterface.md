### 为什么使用IFluentInterface接口

在现代流畅的API中，方法链是在配置某些底层对象时直观发现有效选项的关键技术。

在这些场景中，System.Object方法（Equals，GetHashCode，GetType和ToString）只会对Visual Studio intellisense造成混乱。每个人都知道那些成员总是在那里，但他们很少被明确使用。对于根据接口定义调用流程的流畅API而言，这非常令人讨厌，并且通常在方法链接语句的每个“步骤”中都有很少的成员。

例如，在下面的Moq设置中，在语句的特定步骤中，只有一个“真实”调用有意义（可验证）。但是，它被System.Object成员模糊：

![智能感知](images/full-intellisense.png)

通过简单地从IFluentInterface该项目提供的接口继承您的流畅API接口，可以实现更清晰的智能感知：

![屏蔽干扰](images/full-intellisense.png)

这个怎么运作
诀窍来自EditorBrowsableAttribute，它控制VS intellisense中成员的可见性。要隐藏智能感知中的成员，请对其应用以下属性：
`[EditorBrowsable(EditorBrowsableState.Never)]`

现在，您不希望仅为了应用属性而覆盖每种类型中的所有四个对象成员。存在一个非常优雅的解决方案，它涉及利用隐式接口实现。特别是，您可以定义一个重新定义所有对象成员的接口并应用该属性：

```
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFluentInterface
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type GetType();

    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetHashCode();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string ToString();

    [EditorBrowsable(EditorBrowsableState.Never)]
    bool Equals(object obj);
}
```

现在，您只需在要隐藏这些成员的所有类或接口中“实现”此接口，例如：

```
public interface IVerifies : IFluentInterface
```
