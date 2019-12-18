####  四种方式读取配置信息

1. 通过 `AppSettings` 读取配置信息

配置文件 `Web.config` 或者 `App.config`
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" />
  </startup>
  <appSettings>
    <add key="RedisIP" value="127.0.0.1"/>
    <add key="RedisPort" value="6379"/>
    <add key="RedisPassword" value="111111"/>
  </appSettings>
</configuration>
```
对应读取配置的代码
```
var redisIp = ConfigurationManager.AppSettings["RedisIP"];
var redisPort = ConfigurationManager.AppSettings["RedisPort"];
var redisPassword = ConfigurationManager.AppSettings["RedisPassword"];
```

2. 通过 `Section` 读取配置信息

配置文件 `Web.config` 或者 `App.config`
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="RedisSettings" type="System.Configuration.NameValueSectionHandler"/>
  </configSections>

  <RedisSettings>
    <add key="IP" value="127.0.0.1"/>
    <add key="Port" value="6379"/>
    <add key="Password" value="111111"/>
  </RedisSettings>

</configuration>
```

对应的读取配置的代码
```
var redisSettings = ConfigurationManager.GetSection("RedisSettings") as NameValueCollection;
```

3. 通过 `Group`/`Section` 读取配置信息

配置文件 `Web.config` 或者 `App.config`
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <sectionGroup name="Redis">
      <section name="Settings" type="System.Configuration.NameValueSectionHandler"/>
    </sectionGroup>
  </configSections>

  <Redis>
    <Settings>
      <add key="IP" value="127.0.0.1"/>
      <add key="Port" value="6379"/>
      <add key="Password" value="111111"/>
    </Settings>
  </Redis>

</configuration>
```

对应的读取配置的代码
```
 var redisSetting = ConfigurationManager.GetSection("Redis/Settings") as NameValueCollection;
```

4. 通过定义 `ConfigurationProperty` 读取配置信息

配置文件 `Web.config` 或者 `App.config`
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="RedisSettings" type="ConfigurationTest.RedisSettings, ConfigurationTest"/>
  </configSections>

  <RedisSettings>
    <ConnectionSettings IP="20001" Port="6379" Password="111111"></ConnectionSettings>
  </RedisSettings>
</configuration>
```

对应的读取配置的代码
```
public class RedisInfos : ConfigurationElement
{
    [ConfigurationProperty("IP",IsRequired = true)]
    public int ProductNumber
    {
        get
        {
            return (int)this["IP"];
        }
    }

    [ConfigurationProperty("Port", IsRequired = true)]
    public string ProductName
    {
        get
        {
            return (string)this["Port"];
        }
    }

    [ConfigurationProperty("Password", IsRequired = false)]
    public string Color
    {
        get
        {
            return (string)this["Password"];
        }
    }
}

public class RedisSettings : ConfigurationSection
{
    [ConfigurationProperty("ConnectionSettings")]
    public RedisInfos RedisInfos
    {
        get
        {
            return (RedisInfos)this["ConnectionSettings"];
        }
    }
}
```
读取配置代码
```
 var redisSettings = ConfigurationManager.GetSection("RedisSettings") as ConfigurationTest.RedisSettings;
```
