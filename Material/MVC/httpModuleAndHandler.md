### net的IHttpModule与IHttpHandler

1. `HttpApplication` 提供了基于事件的扩展机制,允许程序员借助于处理管道中的事件进行处理过程扩展。它是Asp.net基础架构来创建和维护。

提供两种方式方便程序员切入,`global.asax` 与 `IHttpModule` ，`HttpApplication` 的事件执行顺序如下,

`BeginRequest` : 当ASP.NET响应请求时，作为HTTP管道执行链中的第一个事件发生。BeginRequest事件表示创建任何给定的新请求。始终引发此事件，并始终是处理请求期间发生的第一个事件

`AuthenticateRequest` :  AuthenticateRequest事件表示配置的身份验证机制已对当前请求进行身份验证。订阅AuthenticateRequest事件可确保在处理附加模块或事件句柄之前对请求进行身份验证

`PostAuthenticateRequest` : 在AuthenticateRequest事件发生后引发PostAuthenticateRequest事件。可以在HttpContext的User属性中访问所有可用信息

`AuthorizeRequest` : AuthorizeRequest事件表示ASP.NET已授权当前请求。您可以订阅AuthorizeRequest事件以执行自定义授权

`PostAuthorizeRequest` : 在授权当前请求的用户时发生

`ResolveRequestCache` : 在ASP.NET完成授权事件以使缓存模块处理来自缓存的请求，绕过事件处理程序的执行并调用任何EndRequest处理程序时发生

`PostResolveRequestCache` : 到达此事件意味着无法从缓存提供请求，因此在此处创建HTTP处理程序。如果请求aspx页面，则会创建Page类

`PostMapRequestHandler` : ASP.NET基础结构使用MapRequestHandler事件根据请求的资源的文件扩展名确定当前请求的请求处理程序

`AcquireRequestState` : 在ASP.NET获取与当前请求关联的当前状态（例如，会话状态）时发生。必须存在有效​​的会话ID

`PostAcquireRequestState` : 在获取与当前请求关联的状态信息（例如，会话状态或应用程序状态）时发生

`PreRequestHandlerExecute` : 在ASP.NET开始执行事件处理程序之前发生

`PostRequestHandlerExecute` : 在ASP.NET事件处理程序完成生成输出时发生

`ReleaseRequestState` : 在ASP.NET完成所有请求事件处理程序的执行后发生。此事件指示ASP.NET状态模块保存当前请求状态

`PostReleaseRequestState` : 在ASP.NET完成所有请求事件处理程序的执行并且请求状态数据已被持久化时发生

`UpdateRequestCache` : 在ASP.NET完成执行事件处理程序时发生，以便让缓存模块存储将被重用以响应来自缓存的相同请求的响应

`PostUpdateRequestCache` : 引发PostUpdateRequestCache时，ASP.NET已完成处理代码，并且最终确定了缓存的内容

`LogRequest` : 在ASP.NET为当前请求执行任何日志记录之前发生。即使发生错误，也会引发LogRequest事件。您可以为LogRequest事件提供事件处理程序，以便为请求提供自定义日志记录

`PostLogRequest` : 在记录请求时发生

`EndRequest` : 当ASP.NET响应请求时，作为HTTP管道执行链中的最后一个事件发生。在这种情况下，您可以压缩或加密响应


2. `IHttpModule` 的定义,Asp.net内置了很多默认的配置 `C:\Windows\Microsoft.NET\Framework\v4.0.30319\Config\web.config`

```
<httpModules>
    <add name="OutputCache" type="System.Web.Caching.OutputCacheModule" />
    <add name="Session" type="System.Web.SessionState.SessionStateModule" />
    <add name="WindowsAuthentication" type="System.Web.Security.WindowsAuthenticationModule" />
    <add name="FormsAuthentication" type="System.Web.Security.FormsAuthenticationModule" />
    <add name="PassportAuthentication" type="System.Web.Security.PassportAuthenticationModule" />
    <add name="RoleManager" type="System.Web.Security.RoleManagerModule" />
    <add name="UrlAuthorization" type="System.Web.Security.UrlAuthorizationModule" />
    <add name="FileAuthorization" type="System.Web.Security.FileAuthorizationModule" />
    <add name="AnonymousIdentification" type="System.Web.Security.AnonymousIdentificationModule" />
    <add name="Profile" type="System.Web.Profile.ProfileModule" />
    <add name="ErrorHandlerModule" type="System.Web.Mobile.ErrorHandlerModule, System.Web.Mobile, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
    <add name="ServiceModel" type="System.ServiceModel.Activation.HttpModule, System.ServiceModel.Activation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" />
    <add name="UrlRoutingModule-4.0" type="System.Web.Routing.UrlRoutingModule" />
    <add name="ScriptModule-4.0" type="System.Web.Handlers.ScriptModule, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"/>
</httpModules>
```

```
public interface IHttpModule
{
  void Dispose(); //Dispose 方法基本就是回收Module所使用的非托管资源，托管资源 .net 垃圾回收会自己做
  void Init(HttpApplication context);
}
```


6. `IHttpHandler` 的定义，Asp.net内置了很多默认的配置

```
<httpHandlers>
    <add path="eurl.axd" verb="*" type="System.Web.HttpNotFoundHandler" validate="True" />
    <add path="trace.axd" verb="*" type="System.Web.Handlers.TraceHandler" validate="True" />
    <add path="WebResource.axd" verb="GET" type="System.Web.Handlers.AssemblyResourceLoader" validate="True" />
    <add verb="*" path="*_AppService.axd" type="System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" validate="False" />
    <add verb="GET,HEAD" path="ScriptResource.axd" type="System.Web.Handlers.ScriptResourceHandler, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" validate="False"/>
    <add path="*.axd" verb="*" type="System.Web.HttpNotFoundHandler" validate="True" />
    <add path="*.aspx" verb="*" type="System.Web.UI.PageHandlerFactory" validate="True" />
    <add path="*.ashx" verb="*" type="System.Web.UI.SimpleHandlerFactory" validate="True" />
    <add path="*.asmx" verb="*" type="System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" validate="False" />
    <add path="*.rem" verb="*" type="System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" validate="False" />
    <add path="*.soap" verb="*" type="System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" validate="False" />
    <add path="*.asax" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.ascx" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.master" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.skin" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.browser" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.sitemap" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.dll.config" verb="GET,HEAD" type="System.Web.StaticFileHandler" validate="True" />
    <add path="*.exe.config" verb="GET,HEAD" type="System.Web.StaticFileHandler" validate="True" />
    <add path="*.config" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.cs" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.csproj" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.vb" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.vbproj" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.webinfo" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.licx" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.resx" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.resources" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.mdb" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.vjsproj" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.java" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.jsl" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.ldb" verb="*" type="System.Web.HttpForbiddenHandler"  validate="True" />
    <add path="*.ad" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.dd" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.ldd" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.sd" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.cd" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.adprototype" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.lddprototype" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.sdm" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.sdmDocument" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.mdf" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.ldf" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.exclude" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.refresh" verb="*" type="System.Web.HttpForbiddenHandler" validate="True" />
    <add path="*.svc" verb="*" type="System.ServiceModel.Activation.HttpHandler, System.ServiceModel.Activation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" validate="False"/>
    <add path="*.rules" verb="*" type="System.Web.HttpForbiddenHandler" validate="True"/>
    <add path="*.xoml" verb="*" type="System.ServiceModel.Activation.HttpHandler, System.ServiceModel.Activation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" validate="False"/>
    <add path="*.xamlx" verb="*" type="System.Xaml.Hosting.XamlHttpHandlerFactory, System.Xaml.Hosting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" validate="False"/>
    <add path="*.aspq" verb="*" type="System.Web.HttpForbiddenHandler" validate="True"/>
    <add path="*.cshtm" verb="*" type="System.Web.HttpForbiddenHandler" validate="True"/>
    <add path="*.cshtml" verb="*" type="System.Web.HttpForbiddenHandler" validate="True"/>
    <add path="*.vbhtm" verb="*" type="System.Web.HttpForbiddenHandler" validate="True"/>
    <add path="*.vbhtml" verb="*" type="System.Web.HttpForbiddenHandler" validate="True"/>
    <add path="*" verb="GET,HEAD,POST" type="System.Web.DefaultHttpHandler" validate="True" />
    <add path="*" verb="*" type="System.Web.HttpMethodNotAllowedHandler" validate="True" />
</httpHandlers>
```

7. [网友讲的比较详细的](https://www.cnblogs.com/fish-li/archive/2012/01/29/2331477.html)
