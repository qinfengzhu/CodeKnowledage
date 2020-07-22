#### Asp.net Core 核心

> Introducion

本文档介绍了ASP.NET Boilerplate framework 与 ASP.NET Core 集成.

> 配置

1. Startup Class ,为了集成 ABP 到 Asp.net Core 中，我们会在`Startup` 类中做如下调整:

```
public class Startup
{
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        //...

        //Configure Abp and Dependency Injection. Should be called last.
        return services.AddAbp<MyProjectWebModule>(options =>
        {
            //Configure Log4Net logging (optional)
            options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                f => f.UseLog4Net().WithConfig("log4net.config")
            );
        });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        //Initializes ABP framework and all modules. Should be called first.
        app.UseAbp();

        //...
    }
}   
```

2. Module Configuration(模块配置)

你可以在 `Startup` 类中配置 AspNet Core 模块,也可以在你自己的定义的 `AbpModule`方法`PreInitialize` 中使用 `Configuration.Modules.AbpAspNetCore()`

> Controllers (控制器)

控制器可以在ASP.NET核心的任何类型的类。它不限于从控制器类派生的类。默认情况下，一类以Controller结尾（如ProductController）作为MVC控制器。您还可以向任何类添加MVC的`[Controller]`属性标记，使之成为控制器。这是ASP.NET核心的MVC工作。更多的看到ASP.NET的核心文件。

如果你会使用Web层类（如HttpContext）或返回一个视图，这是更好地继承`AbpController`（这是从MVC的控制器）类。但是如果你正在创建一个API控制器只是工作的对象，你可以考虑创建一个POCO控制器类或您可以使用您的应用程序服务作为控制器，如下所述。

1. Application Services as Controllers (服务作为控制器)

ASP.NET Boilerplate 提供基础设施创建应用程序服务。如果你想让你的应用服务远程客户作为控制器（如先前使用动态Web API），你可以很容易做到的一个简单的配置,在你的模块(Module)PreInitialize方法中。例子

```
Configuration.Modules.AbpAspNetCore().CreateControllersForAppServices(typeof(MyApplicationModule).Assembly, moduleName: 'app', useConventionalHttpVerbs: true)
```
CreateControllersForAppServices 获取程序集中的(Application Service) 并且自动把这些服务转换为 `Mvc Controller`,可以使用 `RemoteServiceAttribute` 标记使方法生效或者不生效

当应用程序服务被转换为MVC控制器时，它的默认路由将类似于/api/services/<module-name>/<service-name>/<method-name>.例如：如果ProductAppService定义一个创建方法(Create Method)，它的URL将/api/services/app/product/create/创建（假设模块的名称是"app"）

如果`UseConventionalHttpVerbs`设置为true（这是默认值），然后通过命名约定确定HTTP动词使用方法

* Get: Used if method name starts with 'Get'.
* Put: Used if method name starts with 'Put' or 'Update'.
* Delete: Used if method name starts with 'Delete' or 'Remove'.
* Post: Used if method name starts with 'Post', 'Create' or 'Insert'.
* Patch: Used if method name starts with 'Patch'.
* Otherwise, Post is used as default HTTP verb.

你可以使用任何ASP.NET的核心标记(Attributes)改变HTTP方法或路由的行为（当然，这需要添加参考相关ASP.NET核心包）。

注意：以前，动态Web API系统要求为应用程序服务创建服务接口。但这并不是ASP.NET的核心集成所需。另外，MVC属性应该添加到服务类中，即使您有接口

> Filters 过滤器

ABP 为AspNet Core 定义了预建的过滤器 。所有这些都默认添加到所有控制器的所有操作中

1. Authorization Filter(授权过滤)

AbpAuthorizationFilter 被用来与授权系统以及特征系统集成

你可以定义为行动或控制器在执行检查所需的权限`AbpMvcAuthorizeAttribute` 标记。
你可以定义为行动或控制器在执行检查所需的功能`RequiresFeatureAttribute` 标记。
你可以定义`AllowAnonymous`（或`AbpAllowAnonymous`在应用层）的行动或控制器来抑制认证/授权标记

2. Audit Action Filter (审计行为过滤器)

AbpAuditActionFilter 用来整合审计日志系统.默认情况下，它将所有请求记录到所有操作（如果没有禁用审核）。你可以控制审计日志审计和DisableAuditing attributes在 Action 或者 Controller上。

3. Validation Action Filter (动作验证过滤器)

AbpValidationActionFilter 用于集成验证系统和自动验证所有输入的所有行动.此外，ABP的内置验证和标准化，它还检查model.isvalid财产将MVC的验证异常如果有任何动作输入的值无效

你可以控制使用动作和控制器 EnableValidation 、DisableValidation属性标记验证

4. Unit of Work Action Filter(工作单元动作过滤)

AbpUowActionFilter 是用来整合的工作系统。它会自动开始一个新的工作单位前一个动作的执行和完成行动后exucition工作单位（如果没有抛出异常）.

你可以使用属性标记来控制一个动作UnitOfWork UOW的行为

还可以使用启动配置更改所有操作的默认工作单元属性标记

5. Exception Filter (异常过滤)

AbpExceptionFilter 是用来处理从控制器动作抛出的异常。它处理和记录异常并返回对客户机的包装响应

It only handles object results, not view results. So, actions returns any object, JsonResult or ObjectResult will be handled. Action returns a view or any other result type implements IActionsResult are not handled. It's suggested to use built-in UseExceptionHandler extension method defined in Microsoft.AspNetCore.Diagonistics package to handle view exceptions.

Exception handling and logging behaviour can be changed using WrapResult and DontWrapResult attributes for methods and classes.

6. Result Filter (结果过滤)

AbpResultFilter 主要用于包装的action返回的result，如果行动是成功执行的结果

It only wraps results for JsonResult, ObjectResult and any object which does not implement IActionResult (and also their async versions). If your action is returning a view or any other type of result, it's not wrapped.

WrapResult and DontWrapResult attributes can be used for methods and classes to enable/disable wrapping.

You can use startup configuration to change default behaviour for result wrapping.

7. Result Caching For Ajax Requests (用于Ajax请求的结果缓存)

AbpResultFilter 添加缓存控制头（没有缓存，没有存储…）的响应Ajax请求。因此，即使GET请求，它也阻止浏览器缓存Ajax响应。可以通过配置或属性禁用此行为。你可以使用 NoClientCache attrbiute 防止缓存（默认）或AllowClientCache attrbiute 让浏览器缓存结果。或者你可以实现iclientcacheattribute为更好的控制创建您的特殊属性标记

> Model Binders (模型绑定)

AbpDateTimeModelBinder 用来规范DateTime（and Nullable<DateTime>）投入使用的时钟规范方法

> Views (视图)

MVC视图可以继承从AbpRazorPage到自动注入最常用的基础（LocalizationManager，PermissionChecker，SettingManager…等）。它也有快捷方法（如本地化文本的L（…））。启动模板默认继承它。

你可以继承您的Web部件代替ViewComponent 使用AbpViewComponent 利用基地的属性和方法

> Integration Testing (集成测试)

集成测试是相当容易的，这是ASP.NET核心的细节记录在自己的网站。公司遵循本指南和`Abp.AspNetCore.TestBase`包提供了`AbpAspNetCoreIntegratedTestBase`类。它使集成测试更加容易
