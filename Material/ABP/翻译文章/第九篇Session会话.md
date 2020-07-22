#### ABP-Session会话

> 介绍

ASP.NET Boilerplate 提供iabpsession接口获取当前用户和租户不使用ASP.NET的会话。iabpsession也完全集成的ASP.NET Boilerplate 其他结构（设置系统和授权系统为例）。

> Injecting Session (会话注入)

`IAbpSession`一般属性注入需要Session信息才能工作的类。如果我们使用属性注入，我们可以用nullabpsession。例如默认值，如下图所示：

```
public class MyClass : ITransientDependency
{
    public IAbpSession AbpSession { get; set; }

    public MyClass()
    {
        AbpSession = NullAbpSession.Instance;
    }

    public void MyMethod()
    {
        var currentUserId = AbpSession.UserId;
        //...
    }
}
```

因为认证/授权是一个应用层的任务，这是建议使用`IAbpSession`在应用层和上层（我们不使用它在域层通常）。`ApplicationService`，`AbpController` `AbpApicontroller` and some other基础类，有`AbpSession`已经注入。所以，你可以直接使用`AbpSession Property`在应用服务的方法的实例中。

> Session Properties(会话属性)

ApbSession 定义了一些关键属性:

UserId: 如果没有当前用户，则当前用户ID或NULL。如果调用代码被授权，它不能为空

TenantId: 如果没有当前租户（在用户未登录或他是主机用户的情况下），则当前租户的ID或NULL

ImpersonatorUserId: 身份证模拟用户如果当前会话是由模拟另一个用户。如果这不是一个模拟登录它的空

ImpersonatorTenantId: 身份证模拟用户的租户，如果当前会话是由模拟另一个用户。如果这不是一个模拟登录它的空

MultiTenancySide: 可能是主机或租户

UserId 和TenantId可以是空。也有非空getuserid()和gettenantid()方法。
如果你确信有一个当前用户，你可以调用getuserid()。
如果当前用户为null，则此方法引发异常。gettenantid()也类似。

模拟的属性是不常见的其他属性，一般用于日志审计的目的。

`ClaimsAbpSession`是`IAbpSession`接口的默认实现。它获取会话属性（除multitenancyside，它计算）从当前的主要用户。对于基于cookie的表单身份验证，它来自cookie。因此，它很好地集成到ASP.NET的身份验证机制中。

> Overriding Current Session Values(重写当前会话值)

在某些特定的情况下，您可能需要更改/覆盖有限范围的会话值。在这种情况下，你可以使用`IAbpSession`。使用方法如下图所示：

```
public class MyService
{
    private readonly IAbpSession _session;

    public MyService(IAbpSession session)
    {
        _session = session;
    }

    public void Test()
    {
        using (_session.Use(42, null))
        {
            var tenantId = _session.TenantId; //42
            var userId = _session.UserId; //null
        }
    }
}
```
使用方法返回一个IDisposable，必须设置。一旦返回值被处理，会话值将自动恢复到以前的值。

> User Identifier(用户标识符)

你可以使用。`ToUserIdentifier()`扩展方法来创建一个对象从`IAbpSession` `UserIdentifier`。因为`UserIdentifier`用的最多的就是API，这将简化创建当前用户`UserIdentifier`对象。
