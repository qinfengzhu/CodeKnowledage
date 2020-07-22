#### 关于ABP框架

ASP.NET Boilerplate 的设计是为了帮助我们使用最佳实践不重复自己开发的应用。不要重复你自己！在ASP.NET Boilerplate 的核心理念

所有应用程序都有一些常见的问题，需要一些常见的结构。ASP.NET Boilerplate是适当的从小型应用到大型企业的Web应用程序，实现快速启动和可维护的代码库。

> Considerations (注意事项)

在这里，很多重要的概念需要注意在开发 ASP.NET Boilerplate 的时候。

1. Modularity(模块化)

在Web应用程序之间共享实体、存储库、服务和视图应该是很容易的。他们应该被封装成模块，可以很容易地分布（首选公共/私人NuGet包）。模块可以依赖和使用其他模块。我们应该能够在模块中扩展模型以满足应用程序的需要。

模块化为我们提供了“代码重用性”例如，我们可以开发一个模块，其中包含用户管理、角色管理、登录页面、错误页面…然后所有这些都可以由不同的应用程序共享。

2. Best Practices(最佳实践)

应该考虑软件开发中的最佳实践来开发应用程序。使用依赖注入是该领域最重要的课题之一。应该在需要和可能的情况下使用AOP（面向方面编程），特别是横切关注点。应正确使用建筑模式如MVC，MVVM。同时，应遵循整体应用的SOLID原则。

遵循最佳实践使我们的代码库更易于理解和可扩展。它也能防止我们犯别人以前所犯的错误。

3. Scalable code base(可扩展的代码库)

应用程序的体系结构应该提供（甚至强制）保持可维护代码基的方法。分层和模块化是实现这一目标的主要技术。此外，遵循最佳实践是很重要的。否则，应用程序会变得复杂。我们知道，有很多应用程序是从零重新编写的，因为它太难维护代码库。

4. Libraries & frameworks (库和框架)

应用程序应该使用和组合有用的库和框架来完成众所周知的任务。不应试图重新发明车轮如果现有的工具，满足它的要求。它应该尽可能地专注于自己的工作（它自己的业务逻辑）。例如，它可能使用NHibernate EntityFramework或对象关系映射。可以使用AngularJS或durandaljs单页面应用程序框架。

喜欢或不喜欢，今天我们需要学习许多不同的工具来构建一个应用程序。即使是客户端也比较复杂。有更多的库（例如，数以千计的jQuery插件）和框架。因此，我们应该谨慎选择库并适应我们的应用。

ASP.NET Boilerplates撰写的，结合了最佳的工具为你而阻止你使用你自己喜欢的工具。

5. Cross-cutting concerns（横切关注点）

错误处理、验证、授权、日志、缓存，都是在同一个水平上所有应用实现的普通的东西。这些代码应该是通用的，并且由不同的应用程序共享。它也应该与业务逻辑代码分离，并尽可能地自动化。这使我们能够更专注于特定于应用程序的业务逻辑代码，并防止我们一次又一次重新思考相同的东西。

6. More automation(更多的自动化)

如果它可以自动化，它应该是自动化的（至少在大多数情况下）。数据库迁移、单元测试、部署是自动化工作的任务之一。自动化可以节省我们的时间（即使是在中期），防止手工作业出错

7. Convention over configuration（约定优于配置）

约定优于配置是一个非常流行的原则。应用程序框架应该尽可能地实现缺省值。当遵循约定时应该更容易，但也应该在需要时进行配置。

8. Project startup(项目启动)

启动一个新的应用程序应该很容易也很快。我们不应该重复一些繁琐的步骤来创建一个空的应用程序，项目/解决方案模板是一种合适的方法

> Source Codes (源代码)

ASP.NET Boilerplate is an open source project developed under Github.

Source code: [https://github.com/aspnetboilerplate/aspnetboilerplate](https://github.com/aspnetboilerplate/aspnetboilerplate)
Project templates: [https://github.com/aspnetboilerplate/aspnetboilerplate-templates](https://github.com/aspnetboilerplate/aspnetboilerplate-templates)
Sample projects: [https://github.com/aspnetboilerplate/aspnetboilerplate-samples](https://github.com/aspnetboilerplate/aspnetboilerplate-samples)
Module-Zero: [https://github.com/aspnetboilerplate/module-zero](https://github.com/aspnetboilerplate/module-zero)
