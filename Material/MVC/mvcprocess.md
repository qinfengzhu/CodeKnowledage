### MVC 执行流程

1. 请求之如何进入MVC框架

1.1 IIS正常处理.net web 请求的流程

client 请求 -> IIS的HTTP.SYS ->WAS(Web管理服务)->W3WP.exe 加载正确的ISAPI扩展(aspnet_isapi.dll)
-> 启动HTTPRuntime,HttpRuntime.ProcessRequest(HttpWorkerRequest wr)(入口点) -> 在HttpApplicationFactory类帮助下创建一个HttpApplication对象
-> 然后每一个请求都通过HttpModule来到达HttpHandler,这个模块列表由HttpApplication配置

2. Asp.net MVC 如何接管请求

2.1 `System.Web.Routing` 项目的目的是让一个请求与物理文件进行分离,原理是通过映射关系

```
public static void RegisterRoutes(RouteCollection routes)
{
    //MVC
    routes.MapRoute(
        name: "Default",
        url: "{controller}/{action}/{id}",
        defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
    );

    //WebForm
    routes.MapPageRoute(
        routeName: "WebForm",
        routeUrl: "Admin/{user}",
        physicalFile: "~/Admin/User.aspx"
    );
}
```
RouteCollection是一个Route集合,Route封装了名称、Url模式、约束条件、默认值等路由相关信息。
MapPageRoute是RouteCollection定义的方法,而MapRoute是MVC扩展的(在不修改原有代码的情况下添加所需功能),功能都是一样
创建一个Route对象添加到集合中。

2.2 配置文件中见

```
<system.webServer>
	<modules runAllManagedModulesForAllRequests="true">
		<add name="UrlRoutingModule" type="System.Web.Routing.UrlRoutingModule, System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
	</modules>
</system.webServer>
```
UrlRoutingModule订阅了PostResolveRequestCache 事件，实现url的映射。

```
protected virtual void Init(HttpApplication application)
{
	application.PostResolveRequestCache += new EventHandler(this.OnApplicationPostResolveRequestCache);
	application.PostMapRequestHandler += new EventHandler(this.OnApplicationPostMapRequestHandler);
}
private void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
{
	HttpContextBase context = new HttpContextWrapper(((HttpApplication)sender).Context);
	this.PostResolveRequestCache(context);
}
//核心流程,订阅缓存服务模块之后
public virtual void PostResolveRequestCache(HttpContextBase context)
{
	//1.获取RouteData
	RouteData routeData = this.RouteCollection.GetRouteData(context);
	if (routeData == null)
	{
		return;
	}
	//2.获取RouteHandler,在MVC下是 MvcRouteHandler
	IRouteHandler routeHandler = routeData.RouteHandler;
	if (routeHandler == null)
	{
		throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, RoutingResources.UrlRoutingModule_NoRouteHandler, new object[0]));
	}
	if (routeHandler is StopRoutingHandler)
	{
		return;
	}
	//保证HttpContext和RouteData在后续使用
	RequestContext requestContext = new RequestContext(context, routeData);
	//3.获取HttpHandler
	IHttpHandler httpHandler = routeHandler.GetHttpHandler(requestContext);
	if (httpHandler == null)
	{
		throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, RoutingResources.UrlRoutingModule_NoHttpHandler, new object[]
		{
			routeHandler.GetType()
		}));
	}
	context.Items[UrlRoutingModule._requestDataKey] = new UrlRoutingModule.RequestData
	{
		OriginalPath = context.Request.Path,
		HttpHandler = httpHandler
	};
	//后续的MapRequestHandler操作再写回来,context.RewritePath(requestData.OriginalPath);
	context.RewritePath("~/UrlRouting.axd");
}

//PostResolveRequestCache处理完RouteData之后，处理MapHandler
private void OnApplicationPostMapRequestHandler(object sender, EventArgs e)
{
	HttpContextBase context = new HttpContextWrapper(((HttpApplication)sender).Context);
	this.PostMapRequestHandler(context);
}
public virtual void PostMapRequestHandler(HttpContextBase context)
{
	UrlRoutingModule.RequestData requestData = (UrlRoutingModule.RequestData)context.Items[UrlRoutingModule._requestDataKey];
	if (requestData != null)
	{
		context.RewritePath(requestData.OriginalPath); //context.RewritePath("~/UrlRouting.axd"); 对应
		context.Handler = requestData.HttpHandler;
	}
}
```

再看Web.config配置,尤其是 path="UrlRouting.axd"与 path="*.mvc"

```
<handlers>
    <add name="MvcHttpHandler" preCondition="integratedMode" verb="*" path="*.mvc" type="System.Web.Mvc.MvcHttpHandler, System.Web.Mvc, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL" />
    <add name="UrlRoutingHandler" preCondition="integratedMode" verb="*" path="UrlRouting.axd" type="System.Web.HttpForbiddenHandler, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
</handlers>
```

2.1 分析获取RouteData  `UrlRoutingModule` 中的 `RouteData routeData = this.RouteCollection.GetRouteData(context);`

```
public RouteData GetRouteData(HttpContextBase httpContext)
{
	if (httpContext == null)
	{
		throw new ArgumentNullException("httpContext");
	}
	if (httpContext.Request == null)
	{
		throw new ArgumentException(RoutingResources.RouteTable_ContextMissingRequest, "httpContext");
	}
	if (!this.RouteExistingFiles) //这个设置是针对静态直接地址访问的资源进行处理
	{
		string appRelativeCurrentExecutionFilePath = httpContext.Request.AppRelativeCurrentExecutionFilePath;
		if (appRelativeCurrentExecutionFilePath != "~/" && this._vpp != null && (this._vpp.FileExists(appRelativeCurrentExecutionFilePath) || this._vpp.DirectoryExists(appRelativeCurrentExecutionFilePath)))
		{
			return null;
		}
	}
	using (this.GetReadLock())
	{
		foreach (RouteBase current in this)
		{
			RouteData routeData = current.GetRouteData(httpContext);
			if (routeData != null)
			{
				return routeData;
			}
		}
	}
	return null;
}
//设置RouteExistingFiles=truede 例子

public static void RegisterRoutes(RouteCollection routes)
{
	routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
	routes.RouteExistingFiles=true; //设置这个属性，就是通过路由来处理请求的匹配静态资源

	routes.MapRoute{
		name:"pngfile",
		url:"Content/{file}.png",
		defaults:new{controller="Content",action="Png",file=UrlParameter.Optional}
	}
}
```

`routes.MapRoute` 的底层细节，已经如何添加`RouteData`到`RouteCollection`; `MapRoute` 是MVC对`RouteCollection`的扩展方法，主要是添加`RouteData`信息

```
public static Route MapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces) {
    if (routes == null) {
        throw new ArgumentNullException("routes");
    }
    if (url == null) {
        throw new ArgumentNullException("url");
    }
	//这里很重要，传递了MvcRouteHandler
    Route route = new Route(url, new MvcRouteHandler()) {
        Defaults = new RouteValueDictionary(defaults),
        Constraints = new RouteValueDictionary(constraints)
    };

    if ((namespaces != null) && (namespaces.Length > 0)) {
        route.DataTokens = new RouteValueDictionary();
        route.DataTokens["Namespaces"] = namespaces;
    }

    routes.Add(name, route);

    return route;
}
```

继续查看 `Route`对象的 `GetRouteData` 方法:

```
public override RouteData GetRouteData(HttpContextBase httpContext)
{
	string virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;
	RouteValueDictionary routeValueDictionary = this._parsedRoute.Match(virtualPath, this.Defaults);
	if (routeValueDictionary == null)
	{
		return null;
	}
	RouteData routeData = new RouteData(this, this.RouteHandler); //如果是MVC 这里的RouteHandler就是 MvcRouteHandler
	//路由的约束匹配是否OK
	if (!this.ProcessConstraints(httpContext, routeValueDictionary, RouteDirection.IncomingRequest))
	{
		return null;
	}
	foreach (KeyValuePair<string, object> current in routeValueDictionary)
	{
		routeData.Values.Add(current.Key, current.Value);
	}
	if (this.DataTokens != null)
	{
		foreach (KeyValuePair<string, object> current2 in this.DataTokens)
		{
			routeData.DataTokens[current2.Key] = current2.Value;
		}
	}
	return routeData;
}
```

2.3  通过 `RouteData` 中的 `RouteHandler` 来找 `IHttpHandler`, `IRouteHandler` 定义如下

```
public interface IRouteHandler
{
	IHttpHandler GetHttpHandler(RequestContext requestContext);
}
```
在 `MVC`框架中,具体类`MvcRouteHandler` 具体实现如下

```
public class MvcRouteHandler : IRouteHandler {
    protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext) {
        return new MvcHandler(requestContext);
    }

    #region IRouteHandler Members
    IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext) {
        return GetHttpHandler(requestContext);
    }
    #endregion
}
```
到了这里，最后的处理就交给了 `MvcHandler`来进行处理了

2.4 `MvcHandler` 如何与 `Controller` 以及怎么调用 `Action`

2.4.1 首先 `MvcHandler` 实现了 `IHttpHandler` 接口,`void ProcessRequest(HttpContext context)`

```
void IHttpHandler.ProcessRequest(HttpContext httpContext)
{
    ProcessRequest(httpContext);
}
protected virtual void ProcessRequest(HttpContext httpContext)
{
	//封装为HttpContextBase 是为了方便单元测试Mock
    HttpContextBase iHttpContext = new HttpContextWrapper(httpContext);
    ProcessRequest(iHttpContext);
}

protected internal virtual void ProcessRequest(HttpContextBase httpContext)
{
    AddVersionHeader(httpContext);
    //1. 在路由信息中获得Controller的名称,后续从而得到Controller的Type
    string controllerName = RequestContext.RouteData.GetRequiredString("controller");

    //2. Controller的默认工厂,ControllerBuilder中设置Controller工厂为,DefaultControllerFactory
    IControllerFactory factory = ControllerBuilder.GetControllerFactory();

	//3. 根据请求上下文与Controller名称创建Controller实例
    IController controller = factory.CreateController(RequestContext, controllerName);
    if (controller == null) {
        throw new InvalidOperationException(
            String.Format(
                CultureInfo.CurrentUICulture,
                MvcResources.ControllerBuilder_FactoryReturnedNull,
                factory.GetType(),
                controllerName));
    }
    try {
	//4. Controller 根据请求上下文执行操作,实际执行的是Controller类的 ExecuteCore()方法
        controller.Execute(RequestContext);
    }
    finally {
        factory.ReleaseController(controller);
    }
}
```

2.4.2  `RouteData` 中获取Controller 名称的方法

```
public string GetRequiredString(string valueName)
{
	object obj;
	if (this.Values.TryGetValue(valueName, out obj))
	{
		string text = obj as string;
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
	}
	throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, RoutingResources.RouteData_RequiredValue, new object[]
	{
		valueName
	}));
}
```

2.4.3 `DefaultControllerFactory` 创建Controller

```
public virtual IController CreateController(RequestContext requestContext, string controllerName)
{
    if (requestContext == null) {
        throw new ArgumentNullException("requestContext");
    }
    if (String.IsNullOrEmpty(controllerName)) {
        throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
    }
    RequestContext = requestContext;
    Type controllerType = GetControllerType(controllerName);
    IController controller = GetControllerInstance(controllerType);
    return controller;
}
//根据Controller名称,获取Controller类的类型
protected internal virtual Type GetControllerType(string controllerName)
{
    if (String.IsNullOrEmpty(controllerName)) {
        throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
    }

    // first search in the current route's namespace collection
    object routeNamespacesObj;
    Type match;
    if (RequestContext != null && RequestContext.RouteData.DataTokens.TryGetValue("Namespaces", out routeNamespacesObj)) {
        IEnumerable<string> routeNamespaces = routeNamespacesObj as IEnumerable<string>;
        if (routeNamespaces != null) {
            HashSet<string> nsHash = new HashSet<string>(routeNamespaces, StringComparer.OrdinalIgnoreCase);
            match = GetControllerTypeWithinNamespaces(controllerName, nsHash);
            if (match != null) {
                return match;
            }
        }
    }

    // then search in the application's default namespace collection
    HashSet<string> nsDefaults = new HashSet<string>(ControllerBuilder.DefaultNamespaces, StringComparer.OrdinalIgnoreCase);
    match = GetControllerTypeWithinNamespaces(controllerName, nsDefaults);
    if (match != null) {
        return match;
    }

    // if all else fails, search every namespace
    return GetControllerTypeWithinNamespaces(controllerName, null /* namespaces */);
}
//根据Controller类型,创建该类型的Controller实例
protected internal virtual IController GetControllerInstance(Type controllerType)
{
    if (controllerType == null) {
        throw new HttpException(404,
            String.Format(
                CultureInfo.CurrentUICulture,
                MvcResources.DefaultControllerFactory_NoControllerFound,
                RequestContext.HttpContext.Request.Path));
    }
    if (!typeof(IController).IsAssignableFrom(controllerType)) {
        throw new ArgumentException(
            String.Format(
                CultureInfo.CurrentUICulture,
                MvcResources.DefaultControllerFactory_TypeDoesNotSubclassControllerBase,
                controllerType),
            "controllerType");
    }
    try {
        return (IController)Activator.CreateInstance(controllerType);
    }
    catch (Exception ex) {
        throw new InvalidOperationException(
            String.Format(
                CultureInfo.CurrentUICulture,
                MvcResources.DefaultControllerFactory_ErrorCreatingController,
                controllerType),
            ex);
    }
}
```

2.4.4  接`MvcHandler`中`ProcessRequest`中`IController` 执行`Excute` 方法,其实是调用`ControllerBase`的`Excute`

```
public abstract class ControllerBase : MarshalByRefObject, IController
{
    void IController.Execute(RequestContext requestContext)
	{
        Execute(requestContext);
    }
	protected virtual void Execute(RequestContext requestContext)
	{
        if (requestContext == null) {
            throw new ArgumentNullException("requestContext");
        }
		//初始化控制器上下文,把请求与自身传递给控制器上下文
        Initialize(requestContext);
		//该方法是一个抽象方法,具体的交给了Controller自己去实现
        ExecuteCore();
    }
	protected virtual void Initialize(RequestContext requestContext)
	{
        ControllerContext = new ControllerContext(requestContext, this);
    }
}
```

2.4.5 `Controller` 执行 `ExcuteCore`

```
public abstract class Controller
{
	  protected override void ExecuteCore()
	  {
			//加载临时数据(Session中数据)
            TempData.Load(ControllerContext, TempDataProvider);

            try {
				//获取Action名称
                string actionName = RouteData.GetRequiredString("action");
				//ControllerActionInvoker 根据Action名称与Controller上下文调用Action方法
                if (!ActionInvoker.InvokeAction(ControllerContext, actionName)) {
                    HandleUnknownAction(actionName);
                }
            }
            finally {
                TempData.Save(ControllerContext, TempDataProvider);
            }
      }
}
```

2.4.6 `ControllerActionInvoker` 调用 `Controller` 下的 `Action`方法

```
//根据控制器上下文与Action方法名称,来执行Action方法
public virtual bool InvokeAction(ControllerContext controllerContext, string actionName)
{
    if (controllerContext == null) {
        throw new ArgumentNullException("controllerContext");
    }
    if (String.IsNullOrEmpty(actionName)) {
        throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
    }

    ControllerDescriptor controllerDescriptor = GetControllerDescriptor(controllerContext);
    ActionDescriptor actionDescriptor = FindAction(controllerContext, controllerDescriptor, actionName);
    if (actionDescriptor != null) {
        FilterInfo filterInfo = GetFilters(controllerContext, actionDescriptor);

        try {
            AuthorizationContext authContext = InvokeAuthorizationFilters(controllerContext, filterInfo.AuthorizationFilters, actionDescriptor);
            if (authContext.Result != null) {
                // the auth filter signaled that we should let it short-circuit the request
                InvokeActionResult(controllerContext, authContext.Result);
            }
            else {
                if (controllerContext.Controller.ValidateRequest) {
                    ValidateRequest(controllerContext.HttpContext.Request);
                }

                IDictionary<string, object> parameters = GetParameterValues(controllerContext, actionDescriptor);
                ActionExecutedContext postActionContext = InvokeActionMethodWithFilters(controllerContext, filterInfo.ActionFilters, actionDescriptor, parameters);
                InvokeActionResultWithFilters(controllerContext, filterInfo.ResultFilters, postActionContext.Result);
            }
        }
        catch (ThreadAbortException) {
            // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
            // the filters don't see this as an error.
            throw;
        }
        catch (Exception ex) {
            // something blew up, so execute the exception filters
            ExceptionContext exceptionContext = InvokeExceptionFilters(controllerContext, filterInfo.ExceptionFilters, ex);
            if (!exceptionContext.ExceptionHandled) {
                throw;
            }
            InvokeActionResult(controllerContext, exceptionContext.Result);
        }

        return true;
    }

    // notify controller that no method matched
    return false;
}
```
