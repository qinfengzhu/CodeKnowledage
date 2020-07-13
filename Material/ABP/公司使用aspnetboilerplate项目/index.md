#### 基础骨架搭建

> 参考资料

1. `ABP` [项目源码](https://github.com/aspnetboilerplate/aspnetboilerplate)

2. `ABP` [使用文档](https://aspnetboilerplate.com/Pages/Documents)

3. `ABP` 使用的版本为 5.0.0 (5.0.0 已经可以满足项目的要求了)

> 具体使用的类库

1. `Database` ,使用 `EntityFramework`

2. `Mapper` ,使用 `AutoMapper`

3. `Validation` , 包 `Abp.FluentValidation`

4. `IOC`，使用自带的 `Castle.Core`

5. `AOP`，使用自带的 `Castle.Windsor`

6. `Log` ,使用 `log4net`

> 与 `asp.net mvc` 的集成操作,需要包  `Abp`,`Abp.Web.Mvc`

1. `asp.net mvc` 集成操作

```
//web 项目对应需要的 AbpModule
[DependsOn(
    typeof(AbpWebModule),
    typeof(AbpFluentValidationModule)
)]
public class RecruitmentWebModule:AbpModule
{
    public override void Initialize()
    {
        //Dependency Injection
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

        //Areas
        AreaRegistration.RegisterAllAreas();

        //Routes
        RouteConfig.RegisterRoutes(RouteTable.Routes);

        //Bundling
        BundleTable.Bundles.IgnoreList.Clear();
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    }
}

//全局过滤器注册
public class FilterConfig
{
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
        filters.Add(new HandleErrorAttribute());
    }
}

//全局路由注册
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

__重要的改造 Global.asax.cs__

```
public class MvcApplication : AbpWebApplication<RecruitmentWebModule>
{
    protected override void Application_Start(object sender, EventArgs e)
    {
        base.Application_Start(sender, e);
    }

    protected override void Application_BeginRequest(object sender, EventArgs e)
    {
        base.Application_BeginRequest(sender, e);
    }
}
```

2. `log4net` 集成操作,需要类库 `Abp.Castle.Log4Net`

```
// Web的放到 MvcApplication 的 Application_Start 方法中
IocManager.Instance.IocContainer.AddFacility<LoggingFacility>(f => f.UseAbpLog4Net());
```
