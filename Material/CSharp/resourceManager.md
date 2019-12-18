### 多语言资源管理

`System.Resources.ResourceManger` 使用方式

1. `Messages.zh.resx` 格式要求 title.文化标识.resx; .resx 文件是在程序用设置为嵌入式资源

2. 资源的初始化

```
new System.Resources.ResourceManager("xxxxxx.Messages",typeof(Messages).Assembly);
xxxxxx为Messages.xx.resx嵌入资源所在命名空间
```
3. 调用方式

```
var culture = new System.Globalization.CultureInfo("zh");
var email = ResourceManager.GetString("email",culture);
```
