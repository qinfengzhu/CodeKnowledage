[TOC]

## 简介

ASP.NET Boilerplate 集成到ASP.NETWeb API控制器通过Abp.Web.Apinuget包。您可以创建常规ASP.NETWeb API控制器就像你一直做的那样。依赖注入对于常规的apicontroller是正确的。但是您应该从AbpApiController派生控制器，它提供了几个好处，并更好地集成到ASP.NET Boilerplate 。

## AbpApiController 基类

这是一个从AbpApiController派生的简单api控制器：

```
public class UsersController : AbpApiController
{
}
```

### 本地化(Localization)

AbpApiController定义了L方法，使定位更容易。例子：

```
public class UsersController : AbpApiController
{
    public UsersController()
    {
        LocalizationSourceName = "MySourceName";
    }

    public UserDto Get(long id)
    {
        var helloWorldText = L("HelloWorld");

        //...
    }
}
```
您应该设置LocalizationSourceName以使L方法工作。您可以在自己的基本api控制器类中设置它，以避免对每个api控制器重复。

### 其他

您还可以使用预注入AbpSession、EventBus、PermissionManager、PermissionChecker、SettingManager、FeatureManager、FeatureChecker、LocalizationManager、Logger、CurrentUnitOfWork基本属性等。

## 过滤(Filters)

ABP为aspnetwebapi定义了一些预构建的过滤器。默认情况下，所有这些都将添加到所有控制器的所有操作中。

### AuditLogging

AbpApiAuditFilter用于集成审计日志系统。默认情况下，它将所有请求记录到所有操作（如果未禁用审核）。您可以使用actions和controllers的Audited和DisableAuditing属性来控制审计日志记录。

### Authorization

您可以将AbpApiAuthorize属性用于api控制器或操作，以防止未经授权的用户使用您的控制器和操作。例子：

```
public class UsersController : AbpApiController
{
    [AbpApiAuthorize("MyPermissionName")]
    public UserDto Get(long id)
    {
        //...
    }
}
```
### Anti Forgery Filter

AbpAntiForgeryApiFilter用于自动保护ASP.NET针对Web API的动态删除和CSRF请求（包括Web API）。有关更多信息，请参阅CSRF文档。

### Unit Of Work (工作单元)

ABP过滤器用于集成到工作单元系统。它在操作执行之前自动开始一个新的工作单元，并在操作溢出之后完成工作单元（如果没有抛出异常）。

您可以使用UnitOfWork属性来控制操作的UOW行为。您还可以使用启动配置来更改所有操作的默认工作单位属性。

### 结果包装和异常处理(Result Wrapping & Exception Handling)

如果操作已成功执行，则默认情况下ASP.NET Boilerplate不会包装Web API操作。但它处理和包装异常。如果需要，可以将wraperslt/DontWrapResult添加到操作和控制器中。您可以从启动配置（使用Configuration.Modules.AbpWebApi()...). 有关结果包装的更多信息，请参见AJAX文档。

### Result Caching

ASP.NET Boilerplate 文件将缓存控制头（无缓存，无存储）添加到Web API请求的响应中。因此，它可以阻止浏览器缓存响应，即使对于GET请求也是如此。此行为可以通过配置禁用。

### Validation

AbpApiValidationFilter自动检查ModelState.IsValid如果操作无效，则阻止执行该操作。另外，实现验证文档中描述的输入数据到验证。

### Model Binders

AbpApiDateTimeBinder 被用于正常化 DateTime(Nullable<DateTime>) 输入,使用 Clock.Normalize 方法。
