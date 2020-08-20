[TOC]

## 介绍

从它的网站上：“……使用支持Swagger的API，您可以获得交互式文档、客户端SDK生成和可发现性。”

## Asp.net core

### 安装包 __Swashbuckle.AspNetCore__ 到你的Web Project

### 配置

将Swagger的配置代码添加到Startup.cs

```
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    //your other code...

      services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "AbpZeroTemplate API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
            });

    //your other code...
}
```

然后，将下面的代码添加到Startup.cs  以便使用Swagger

```
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    //your other code...

     app.UseSwagger();
            //Enable middleware to serve swagger - ui assets(HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AbpZeroTemplate API V1");
            }); //URL: /swagger

    //your other code...
}
```

测试
现在你可以使用浏览器打开 "/swagger".

## Asp.net 5.x

### 安装包 __Swashbuckle.Core__ 到你的WebApi Project或者 Web Project

### 配置

将Swagger的配置代码添加到模块的Initialize方法中。例子：

```
public class SwaggerIntegrationDemoWebApiModule : AbpModule
{
    public override void Initialize()
    {
        //your other code...

        ConfigureSwaggerUi();
    }

    private void ConfigureSwaggerUi()
    {
        Configuration.Modules.AbpWebApi().HttpConfiguration
            .EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "SwaggerIntegrationDemo.WebApi");
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            })
            .EnableSwaggerUi(c =>
            {
                c.InjectJavaScript(Assembly.GetAssembly(typeof(AbpProjectNameWebApiModule)), "AbpCompanyName.AbpProjectName.Api.Scripts.Swagger-Custom.js");
            });
    }
}
```

注意，我们注入了一个名为“Swagger”的javascript文件-自定义.js“配置swagger ui时。此脚本文件用于在swagger ui上测试api服务时向请求添加CSRF令牌。您还需要将此文件作为嵌入式资源添加到WebApi项目中，并在注入时在InjectJavaScript方法中使用它的逻辑名称。

重要提示：上面的代码对于您的项目将略有不同（命名空间不会abp公司名称.AbpProjectName... 而AbpProjectNameWebApiModule将是您的projectnameWebApiModule）。

Swagger-Custom.js在这里：

```
var getCookieValue = function(key) {
    var equalities = document.cookie.split('; ');
    for (var i = 0; i < equalities.length; i++) {
        if (!equalities[i]) {
            continue;
        }

        var splitted = equalities[i].split('=');
        if (splitted.length !== 2) {
            continue;
        }

        if (decodeURIComponent(splitted[0]) === key) {
            return decodeURIComponent(splitted[1] || '');
        }
    }

    return null;
};

var csrfCookie = getCookieValue("XSRF-TOKEN");
var csrfCookieAuth = new SwaggerClient.ApiKeyAuthorization("X-XSRF-TOKEN", csrfCookie, "header");
swaggerUi.api.clientAuthorizations.add("X-XSRF-TOKEN", csrfCookieAuth);
```
