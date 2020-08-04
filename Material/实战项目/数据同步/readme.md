#### 底层数据库同步Job支持(该类库提供底层的数据驱动支撑)

1. 同步程序可以直接连接2个数据库(sqlserver,mysql,oracle)等

2. 同步程序不能直接连接2个数据库,需要通过webapi等方式进行同步调用处理


#### 配置文件形式(简单的形式)

1. 可以直接连的

```
<?xml version="1.0" encoding="utf-8" ?>
<!--此配置例子主要为可以直接连接两个库-->
<!--更多条件操作见底层扩展,condition仅为同步数据的关键时间点-->
<configuration>
  <connectionStrings>
    <add name="wetrial.from" connectionString="Data Source=127.0.0.1;Initial Catalog=fromDatabase ;Integrated Security=false;User ID=sa;Password=Abcd1234" providerName="System.Data.SqlClient"/>
    <add name="wetrial.to" connectionString="Data Source=127.0.0.1;Initial Catalog=toDatabase ;Integrated Security=false;User ID=sa;Password=Abcd1234" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  <configSections>
    <section name="dataAsync" type="DataAsync.DataBase.DataAsyncSection,DataAsync.DataBase"/>
  </configSections>
  <dataAsync>
    <transferMode value="Integrated"/>
    <tables>
      <table id="ctskey" name="cts_User" from="cts_User" to="auth_User">
        <conditions>
          <condition name="CreatedTime" operator="max" target="to"/>
          <condition name="CreatedTime" operator=">" target="from"/>
        </conditions>
        <columns>
          <column from="UserName" to="UserName"/>
          <column from="Password" to="Password"/>
          <column from="Id" to="Id"/>
          <column from="Email" to="Email"/>
          <column from="PhoneNumber" to="PhoneNumber"/>
        </columns>
      </table>
    </tables>
  </dataAsync>
</configuration>
```

//定时任务 quartz_jobs.xml 配置

2. 需要api跳转方式


2.1 数据提供端

```
<?xml version="1.0" encoding="utf-8" ?>
<!--此配置例子为Api数据提供端-->
<configuration>
  <connectionStrings>
    <add name="wetrial.provider" connectionString="Data Source=127.0.0.1;Initial Catalog=fromDatabase ;Integrated Security=false;User ID=sa;Password=Abcd1234" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  <configSections>
    <section name="dataAsync" type="DataAsync.DataBase.DataAsyncSection,DataAsync.DataBase"/>
  </configSections>
  <dataAsync>
    <transferMode value="DataProvider"/>
    <tables>
      <table id="ctskey" name="cts_User">
        <conditions>
          <condition name="CreatedTime" operator=">="/>
        </conditions>
        <columns>
          <column name="UserName" to="UserName"/>
          <column name="Password" to="Password"/>
          <column name="Id" to="Id"/>
          <column name="Email" to="Email"/>
          <column name="PhoneNumber" to="PhoneNumber"/>
        </columns>
      </table>
    </tables>
  </dataAsync>
</configuration>
```

2.2 数据获取端

```
<?xml version="1.0" encoding="utf-8" ?>
<!--此配置例子为获取api端提供的数据-->
<configuration>
  <connectionStrings>
    <add name="wetrial.store" connectionString="Data Source=127.0.0.1;Initial Catalog=toDatabase ;Integrated Security=false;User ID=sa;Password=Abcd1234" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  <configSections>
    <section name="dataAsync" type="DataAsync.DataBase.DataAsyncSection,DataAsync.DataBase"/>
  </configSections>
  <dataAsync>
    <transferMode value="DataPull"/>
    <api route="http://testdata.vip/api/datacrawl/"/> <!--提供端默认的提取url为 xxxx/api/datacrawl/{tablename}-->
    <tables>
      <table id="ctskey" name="cts_User" from="cts_User">
        <conditions>
          <condition name="CreatedTime" operator="max"/> <!--支持 max >= = =<  <>-->
        </conditions>
        <columns>
          <column from="UserName" to="UserName"/>
          <column from="Password" to="Password"/>
          <column from="Id" to="Id"/>
          <column from="Email" to="Email"/>
          <column from="PhoneNumber" to="PhoneNumber"/>
        </columns>
      </table>
    </tables>
  </dataAsync>
</configuration>
```
//定时任务 quartz_jobs.xml 配置

##### 走代码形式(实现复杂的同步,底层Enginer支撑)

##### 数据库底层ORM -> NPoco

##### 依赖注入框架-> NInject
