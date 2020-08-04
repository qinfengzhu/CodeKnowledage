#### 去掉bin 目录下生成 roslyn目录

1. `packages.config` 中会多一个包 `<package id="Microsoft.CodeDom.Providers.DotNetCompilerPlatform" version="2.0.1" targetFramework="net472" />`

处理办法，通过 nuget管理程序包删除它

2. WebConfig中引入

```
<system.web>
  <compilation debug="true" targetFramework="4.7.2">
    <assemblies>
      <add assembly="netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" />
    </assemblies>
  </compilation>
  <httpRuntime targetFramework="4.7.2" />
</system.web>
```
