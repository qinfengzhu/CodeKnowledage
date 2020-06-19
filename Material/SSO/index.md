#### 单点登录SSO

> 1. SSO设计与实现

![核心应用与依赖](images/sso-design.png)


> 2. 用户登录与登录校验(非跨域情况)

* 2.1 登录时序图

![登录时序图](images/login-validate.png)

* 2.2 登录信息获取/登录状态校验

![登录信息获取 状态校验](images/logininfo.png)

* 2.3 登出时序图

![登出时序图](images/logout.png)

> 3. 跨域情况

* 3.1 跨域登录(主域名已经登录)

![主域名已经登录](images/crossdomain-ylogin.png)

* 3.2 跨域登录(主域名未登录)

![主域名未登录](images/crossdomain-nlogin.png)

* 3.3 跨域登出

![跨域登出](images/crossdomain-logout.png)
