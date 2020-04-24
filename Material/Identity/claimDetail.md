#### asp.net core认证和授权的初始认识--claim、claimsidentity、claimsprincipal

Claim表示一个声明单元，它用来组成ClaimsIdentity。ClaimsIdentity表示一个证件，例如身份证，身份证上面的名字表示一个Claim，身份证号也表示一个Claim，所有这些Claim组成身份证，即ClaimsIdentity。一个人不止有一个能够表示身份的东西，还有驾驶证、户口本等等，这些都是一个一个的CLaimsIdentity，而我们人本身是一个ClaimsPrincipal。用程序来表示就是：

```
Claim nameClaim = new Claim(ClaimTypes.Name, "pangjianxin");
Claim idClaim = new Claim(ClaimTypes.Sid, "1502xxxxxxxxxx");
Claim genderClaim = new Claim(ClaimTypes.Gender, "female");
Claim countryClaim = new Claim(ClaimTypes.Country, "china");
//....省略身份证上面的其他要素....
ClaimsIdentity id = new ClaimsIdentity("身份证");
id.AddClaim(nameClaim);
id.AddClaim(idClaim);
id.AddClaim(genderClaim);
id.AddClaim(countryClaim);
ClaimsPrincipal principal = new ClaimsPrincipal(id);
```
上面的代码展现了一个身份主体的构造过程，但是这个身份主体构造完成之后如何保存到客户那里呢？要知道只有保存了这个信息，下次登陆网站的时候拿上这个东西才能访问到你该访问到的资源（Authorize）。在asp.net core中，会将上面的ClaimsPrincipal序列化成一个AuthenticationTicket。也就是一个票根，asp.net core会将这个票据发送给你，当然不是你，而是你的浏览器，浏览器会帮你妥善保管。然后，当你再次访问网站的时候，浏览器会自动带上这个票据（Cookie）去访问资源。AuthenticationTicket中有AuthenticationScheme，这个用来表示认证（Authentication）的方式（Scheme:方案）。比如我们现有的技术有Cookie认证，jwtbear认证、OATH2&openIdConnect等，Scheme作用就是找一个Handler，来实现最终的认证。这个Handler可能是CookieAuthenticationHandler、JwtbearerHandler等等。还有一个重要的东西是AuthenticationOptions，这个用来配置Scheme，并且使用option模式（具体来说，就是这样的：services.Configure(Action(options))）进行配置，然后到某一步需要这个option呢，就在构造函数中注入一个IOption<Toption>的东西来进行注入配置。很灵活哦。这只是冰山一角，要吧这个故事全部讲述完，需要太长的时间和精力了。有时间再进行补充。
