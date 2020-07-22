#### Audit-Logging审计日志

> Introduction

维基百科：“审计跟踪（又称审计日志）是一个与安全有关的时间记录、记录集和/或目的地和记录来源，它们提供随时影响特定操作、程序或事件的活动序列的书面证据。”

ASP.NET Boilerplate 提供了一个基础，记录所有的交互的应用程序自动。它可以用调用者的信息和参数记录预定的方法调用。

基本上，保存的字段是：相关的租户ID、调用方用户ID、被调用的服务名称（被调用方法的类）、方法名、执行参数（序列化为JSON）、执行时间、执行时间（如毫秒）、客户机IP地址、客户端计算机名和异常（如果方法抛出异常）。

使用这些信息，我们不知道谁做的操作，也可以测量的应用性能和观察中引发的异常。更甚者，您可以获得有关应用程序使用情况的统计数据。

审计系统采用iabpsession获取当前用户名和tenantid。

应用服务，MVC控制器，Web API和ASP.NET的核心方法是自动审核，默认

__About IAuditingStore__

审计系统采用iauditingstore保存审计信息。虽然可以以自己的方式实现它，但它完全在模块为零的项目中实现。如果你不执行它，SimpleLogAuditingStore用它写审计信息的日志


> configuration

配置审计，你可以使用配置。在你的模块的属性审计分发方法。默认情况下启用审核。您可以禁用它，如下所示

```
public class MyModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Auditing.IsEnabled = false;
    }
}
```
IsEnabled: Used to enable/disable auditing system completely. Default: true.
IsEnabledForAnonymousUsers: If this is set to true, audit logs are saved also for users those are not logged in to the system. Default: false.
Selectors: Used to select other classes to save audit logs


选择器是一个谓词列表，用于选择其他类型以保存审计日志。选择器有唯一的名称和谓词。此列表中惟一的默认选择器用于选择应用程序服务类。它的定义如下所示

```
Configuration.Auditing.Selectors.Add(
    new NamedTypeSelector(
        "Abp.ApplicationServices",
        type => typeof (IApplicationService).IsAssignableFrom(type)
    )
);
```

> Enable/Disable by attributes

虽然你可以选择审计类的配置，您可以使用审核和disableauditing属性为一个类，一个单一的方法。一个例子

```
[Audited]
public class MyClass
{
    public void MyMethod1(int a)
    {
        //...
    }

    [DisableAuditing]
    public void MyMethod2(string b)
    {
        //...
    }

    public void MyMethod3(int a, int b)
    {
        //...
    }

```
