#### 读取配置

```
 NameValueCollection props = (NameValueCollection)System.Configuration.ConfigurationManager.GetSection("quartz");
```
对应的 `Config`文件配置

```
<configSections>
  <section name="quartz" type="System.Configuration.NameValueSectionHandler,System"/>
</configSections>
<quartz>
  <add key="quartz.scheduler.instanceName" value="ExampleDefaultQuartzScheduler" />
  <add key="quartz.threadPool.class" value="Quartz.Simpl.SimpleThreadPool, Quartz" />
  <add key="quartz.threadPool.threadCount" value="10" />
  <add key="quartz.threadPool.threadPriority" value="2" />
  <add key="quartz.jobStore.misfireThreshold" value="60000" />
  <add key="quartz.jobStore.class" value="Quartz.Simpl.RAMJobStore, Quartz" />
</quartz>
```
