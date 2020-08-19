[TOC]

## 安装 MySql.Data.Entity

打开DbContext的配置类(Configuration.cs)并将下面的代码放在它的构造函数中

```
SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
```

### 配置连接字符串(Configure ConnectionString)

您需要在web.config文件文件，以便使用MySql数据库。一个示例连接字符串是

```
<add name="Default" connectionString="server=localhost;port=3306;database=sampledb;uid=root;password=***" providerName="MySql.Data.MySqlClient"/>
```
### 重新生成迁移(Re-generate migrations)

如果选择“包括模块 Zero？”下载启动模板时，项目中会包含一些迁移文件，但这些文件是为Sql Server生成的。删除迁移文件夹下.EntityFramework项目中的所有迁移文件。迁移文件以时间戳开头。迁移文件名如下“201506210746108_AbpZero_Initial”

删除所有迁移文件后，选择.Web项目作为启动项目，打开Visual Studio的包管理器控制台，并在包管理器控制台中选择.EntityFramework项目作为默认项目。然后运行下面的命令来添加MySql的迁移。

```
Add-Migration "AbpZero_Initial"
```

现在可以使用下面的命令创建数据库

```
update-Database
```
现在你可以用MySql运行你的项目了。
