### 打包与编译

1. `Core` 即 `IdentityServer.Core`

包含 IdentityServerd的核心对象模型,服务，以及服务器。 `Core` 仅支持内存级别的配置与用户存储，
但是您可以通过配置插件支持其他存储。这就是其他仓储以及包的意义所在。

2. `Configuration stores` 配置存储

存储 配置数据(客户端与范围(Scopes)) 以及运行时的数据(consent,令牌句柄(token handles),刷新令牌(refresh tokens))

官方实现的EF存储,[Entity Framework](https://github.com/identityserver/IdentityServer3.EntityFramework)

社区贡献的MongoDb 存储,[MongoDb](https://github.com/jageall/IdentityServer.v3.MongoDb)

3. `User stores` 用户存储 [MembershipReboot](https://github.com/identityserver/IdentityServer3.MembershipReboot) 与 [Asp.net Identity](https://github.com/identityserver/IdentityServer3.AspNetIdentity)

4. `Plugins` 插件

  协议插件，如: [WS-Federation](https://github.com/identityserver/IdentityServer3.WsFederation)

5. `Access token validation middleware` Owin访问令牌验证中间件

用于api的owin中间件，提供了一种验证访问令牌和强制作用域要求的简单方法
如: [AccessTokenValidation](https://github.com/IdentityServer/IdentityServer3.AccessTokenValidation)
