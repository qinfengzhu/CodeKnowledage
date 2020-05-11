### 创建最简单的Oath2 授权服务器，客户端，API

本次演练的目的是创建尽可能简单的 `IdendityServer` 充当 `OAuth2授权服务器`.这将实力开始了解一些基本功能与
配置项([完整的源代码到](https://github.com/IdentityServer/IdentityServer3.Samples/tree/master/source/Simplest%20OAuth2%20Walkthrough)).文档中还有其他更高级的演练,您可以在之后进行，本教程包括:

* 创建自承载的 IdentityServer

* 为应用程序设置客户端

* 注册 API

* 请求访问令牌

* 调用API

* 验证访问令牌

#### 设置 IdentityServer

首先我们创建一个 控制台主机(console host) ,然后配置 IdentityServer

```
控制台程序，先通过 nuget 安装 IdentityServer3
install-package IdentityServer3
```

__注册API__

API 被建模为作用域,您需要注册所有希望通过请求访问令牌的API,为此我们创建一个返回域列表

```
using IdentityServer3.Core.Models;

internal static class Scopes
{
  public static List<Scope> Get()
  {
    return new List<Scope>()
    {
       new Scope
       {
          Name="api1"
       }
    };
  }
}
```

__注册客户端__

现在我们只想注册一个客户端，这个客户端能够请求 `api1`作用域的令牌.对于我们第一次迭代，不会有人参与，
客户端只会代表自己请求令牌。

对于客户机我们配置如下：

* 客户端名称与客户端ID(唯一)

* 客户端秘钥(用于根据令牌终端节点对客户端进行身份验证)

* 流(客户端凭据流在这次实例中)

* 使用所谓的引用Token(Reference Token),引用令牌不需要签名证书

* 访问 `api1` 的 scope (范围)

```
using IdentityServer3.Core.Models;

internal static class Clients
{
  public static List<Client> Get()
  {
    return new List<Client>
    {
       new Clients
       {
         ClientName = "Silicon-only Client",
         ClientId = "silicon",
         Enabled = true,
         AccessTokenType = AccessTokenType.Reference,
         Flow = Flows.ClientCredentials,
         ClientSecrets = new List<Secret>
         {
           new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
         },
         AllowedScopes = new List<string>
         {
           "api1"
         }
       }
    };
  }
}
```

__配置IdentityServer__

IdentityServer 作为Owin中间件实现，它是在启动类(`Startup` class)中使用 `UseIdentityServer` 扩展方法，
下面的代码片段将使用我们的作用域和客户机(scopes and clients) 简历一个简单的服务器，我们还设置了一个空的用户列表，稍后将添加用户.

```
using Owin;
using System.Collections.Generic;
using IdenityServer3.Core.Configuration;
using IdentityServer3.Core.Services.InMemory;

namespace IdentitServer3
{
  class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      var options = new IdentityServerOptions
      {
        Factory = new IdentityServerServiceFactory()
                      .UseInMemoryClients(Clients.Get())
                      .UseInMemoryScopes(Scopes.Get())
                      .UseInMemoryUsers(new List<InMemoryUser>()),
        RequireSsl = false
      };

      app.UseIdentityServer(options);
    }
  }
}
```

__添加日志__

因为我们是在控制台中运行，所以将日志输出直接显示到控制台窗口非常方便，`Serilog` 是一个非常好的日志库
```
install-package serilog -Version 1.5.14
install-package serilog.sinks.literate -Version 1.2.0
```
__IdentityServer 宿主(Hosting IdentityServer)__

最后一步是托管 IdentityServer,为此,我们将添加 `Katana selft-hosting`包到我们的控制台程序端
```
install-package Microsoft.Owin.SelfHost
```
添加如下代码到 `Program.cs`
```
Log.Logger = new LoggerConfiguration()
                 .WriteTo
                 .LiterateConsole(outputTemplate:"{Timestamp:HH:mm} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}")
                 .CreateLogger();

using(WebApp.Start<Startup>("http://localhost:5000"))
{
  Console.WriteLine("Server running......");
  Console.ReadLine();
}                 
```
当您运行控制台程序时，您应该会看到一些诊断输出 `Server running......`

__添加API__

在这一部分，我们将添加一个简单的WebApi，它被配置为需要来自我们刚刚设置的IdentityServer的访问令牌

* 创建 Web Host

添加一个 `Asp.net Web Application` 到解决方案，并且选择为 `Empty`（没有任何框架引用）
```
install-package Microsoft.Owin.Host.SystemWeb
install-package Microsoft.AspNet.WebApi
install-package IdentityServer3.AccessTokenValidation
```
* 添加一个控制器(Controller)

```
[Route("test")]
public class TestContoller : ApiController
{
   public IHttpActionResult Get()
   {
      var caller = User as ClaimsPrincipal;

      return Json(new
        {
            message = "OK computer",
            client = caller.FindFirst("client_id").Value
        });
   }
}
```

在这个控制器中的 `User` 属性允许您从访问令牌访问声明。

__添加 Startup__

添加如下的 `Startup` 类,使WebApi与IdentityServer之间建立信任

```
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using IdentityServer3.AccessTokenValidation;

[assembly:OwinStartup(typeof(Apis.Startup))]

namespace WebApis
{
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      //accept access tokens from idenityserver and require a scope of 'api1'
       app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions{
          Authority = "http://localhost:5000",
          ValidationMode = ValidationMode.ValidationEndpoint,
          RequiredScopes = new[]{"api1"}
         });

        //configure web api
        var config = new HttpConfiguration();
        config.MapHttpAttributeRoutes();

        //require authentication for all controllers
        config.Filters.Add(new AuthorizeAttribute());

        app.UseWebApi(config);
    }
  }
}
```

当您尝试浏览器访问 test controller 的时候，您应该看到401页面,因为必要的 `access token`丢失了。

__添加控制台类型的Client（Console Client）__

在下一部分中，我们将添加一个简单的控制台客户端,该客户端将请求一个访问令牌,并使用该令牌对api进行身份验证。
首先添加一个新的控制台项目，并安装OAuth2客户端类库
```
install-package IdentityModel
```

* 使用客户端凭据请求访问令牌

```
using IdentityModel.Client;

static TokenResponse GetClientToken()
{
  var client = new TokenClient(
    "http://localhost:5000/connect/token",
    "silicon",
    "F621F470-9731-4A25-80EF-67A6F7C5F4B8"
    );

    return client.RequestClientCredentialsAsync("api1").Result;
}
```

* 使用访问令牌访问 API

```
static void CallApi(TokenResponse response)
{
  var client = new HttpClient();
  client.SetBearerToken(response.AccessToken);
  Console.WriteLine(client.GetStringAsync("http://localhost:14869/test").Result);
}
```
您应该看到 `{"message":"OK computer","client":"silicon"}` 在您的控制台输出端

__添加用户__

到目前为止,客户端为自己请求一个访问令牌，而不涉及任何用户。下面让我们介绍用户这一块

* 添加一个用户服务

这个用户服务管理用户，我们将使用点单的内存用户服务，首先我们需要定义一些用户
```
using IdentityServer3.Core.Services.InMemory;

internal static UseInMemoryScopes
{
  public static List<InMemoryUser> Get()
  {
    return new List<InMemoryUser>
    {
      new InMemoryUser
      {
        Username = "bob",
        Password = "secret",
        Subject = "1"
      },
      new InMemoryUser
      {
        Username = "alice",
        Password = "secret",
        Subject = "2"
      }
    }
  }
}
```

`Username` 与 `Password` 用于对用户进行身份验证, `Subject` 是将嵌入到访问令牌中的该用户的唯一标识符，在 `Startup` 中使用上面的`Get` 方法替换掉空用户列表

* 添加一个客户

接下来，我们将添加一个使用称为资源所有者密码凭据授予的流的客户端定义。此流允许客户端将用户的用户名和密码发送到令牌服务，并返回一个访问令牌。

```
using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdSrv
{
   static class Clients
   {
     public static List<Client> Get()
     {
       return new List<Client>
       {
          //没有用户信息的 (no human involved)
          new Clients
          {
             ClientName = "Silicon-only Client",
             ClientId = "silicon",
             Enabled = true,
             AccessTokenType = AccessTokenType.Reference,

             Flow = Flows.ClientCredentials,

             ClientSecrets = new List<Secret>
             {
                 new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
             },

             AllowedScopes = new List<string>
             {
                 "api1"
             }
          },

          //有用户信息的 (human is involved)
          new Client
          {
            ClientName = "Silicon on behalf of Carbon Client",
            ClientId = "carbon",
            Enabled = true,
            AccessTokenType = AccessTokenType.Reference,

            Flow = Flows.ResourceOwner, //注意这里的不同

            ClientSecrets = new List<Secret>
            {
                new Secret("21B5F798-BE55-42BC-8AA8-0025B903DC3B".Sha256())
            },

            AllowedScopes = new List<string>
            {
                "api1"
            }
          }
       }
     }
   }
}
```

__更新API端__

当涉及到人时，访问令牌将包含唯一标识用户的 `sub` claim (子声明),让我们对 API控制器做微小的调整

```
[Route("test")]
public class TestController:ApiController
{
  public IHttpActionResult Get()
  {
    var caller = User as ClaimsPrincipal;

    var subjectClaim = caller.FindFirst("sub");
    if(subjectClaim != null)
    {
      return Json(new
        {
         message = "OK user",
         client = caller.FindFirst("client_id").Value,
         subject = subjectClaim.Value
        });
    }else{
       return Json(new{
          message ="OK computer",
          client = caller.FindFirst("client_id").Value
         });
    }
  }
}
```

__更新Client__

向客户端添加一个新方法以代表用户请求访问令牌

```
static TokenResponse GetUserToken()
{
  var client = new TokenClient(
        "http://localhost:5000/connect/token",
        "carbon",
        "21B5F798-BE55-42BC-8AA8-0025B903DC3B");

    return client.RequestResourceOwnerPasswordAsync("bob", "secret", "api1").Result;
}
```
