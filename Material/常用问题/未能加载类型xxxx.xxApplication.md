##### 未能加载类型"xxxxxxx.WebApiApplication"

1. 一般就是项目进行了调整名称，但是 `Global.asax` 文件里头的引用没有改变类型路径

打开项目工程，可以看到 `Global.asax` 与 `Global.asax.cs` 两个文件,

Global.asax 文件中内容为

```
<%@ Application Codebehind="Global.asax.cs" Inherits="xxxxx.xxxxx.WebApiApplication" Language="C#" %>
```
确保上面的 `xxxxx.xxxxx.WebApiApplication` 是正确的
