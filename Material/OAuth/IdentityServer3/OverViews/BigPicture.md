### 大局图

很多现在的应用程序看起来跟下图类似

![一般应用程序](images/appArch.png)

它们之间的关系

1. Browsers 与 Web 应用程序交互
2. Web 应用程序与 Web Api 交互
3. 基于浏览器的应用程序与 Web Api交互
4. 本地的应用程序与 Web Api交互
5. Web Api 与 Web Api 之间交互

通常，每一层（前端,中间层,后端）必须保护资源并且必须实现`认证`与`授权`，而且通常针对同一个用户存储（也就是说，这些层的用户数据源是同一个）

基于上面的问题，导致如下图的安全体系结构和协议的使用

![新的安全体系结构和协议应用](images/protocols.png)

这中方式，把安全的问题分为了`2个部分`

#### 认证

当应用程序需要知道当前用户的身份时，需要进行身份认证。通常，这些应用程序代表该用户管理数据，并且需要确保该用户只能访问他被允许访问的数据。最常见的例子是（经典的）web应用程序，但是本地和基于JS的应用程序也需要身份验证。

最常见的身份验证协议是SAML2p、WS-Federation和OpenID-Connect，SAML2p是最流行和部署最广泛的。

OpenID Connect是这三个应用中的最新一个，但通常被认为是未来的发展方向，因为它在现代应用中最有潜力。它从一开始就为移动应用程序场景而构建，并且设计为对API友好的。

#### API 访问

应用程序有两种与api通信的基本方式：使用应用程序标识或委托用户标识。有时这两种方式需要结合起来。

OAuth2是一个协议，它允许应用程序从安全令牌服务请求访问令牌并使用它们与API通信。这降低了客户端应用程序和api的复杂性，因为身份验证和授权可以集中化。

#### OpenID Connect 与 OAuth2

OpenID Connect和OAuth2非常相似——实际上OpenID Connect是OAuth2之上的一个扩展。这意味着您可以将两个基本的安全问题（身份验证和API访问）组合到一个协议中，并且通常是安全令牌服务的一次往返。

这就是为什么我们认为OpenID Connect和OAuth2的结合是在可预见的将来保护现代应用程序的最佳方法。IdentityServer3是这两个协议的一个实现，并且高度优化以解决当今移动、本机和web应用程序的典型安全问题。
