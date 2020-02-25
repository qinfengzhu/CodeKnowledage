###  Maven学习之旅

#### Maven 能够做什么

1. 管理jar包 : 增加三方jar包; 解决jar包之间的依赖关系,并且不会冲突问题

2. 将项目拆分层多个模块

####　Maven的概念

1. 基于Java平台的自动化构建工具

2. 清理，编译，测试，报告，打包，安装，部署

#### Maven 安装

1. 下载maven

2. 配置环境变量 MAVEN_HOME 或者 M2_HOME ,maven的根目录

2. 配置Maven的Path, %MAVEN_HOME%\bin

4. 配置本地仓库，根据maven/conf/setting.xml中的注释

```
 <localRepository>F:/mvnrepository</localRepository>
```

5. 添加国内镜像

```
<mirror>
    <id>alimaven</id>
    <mirrorOf>central</mirrorOf>
    <name>aliyun maven</name>
    <url>http://maven.aliyun.com/nexus/content/repositories/central/</url>
</mirror>
```

#### 如何使用`Maven`

1. maven 的约定目录结构

```
src
    --main            :程序代码
          --java           ：java代码
          --resources      ：资源代码、配置代码
    --test            ：程序代码
          --java
          --resources  
    --pom.xml         :项目对象模型    
```

2. maven 命令必须在，pom.xml 文件所在的路径下执行

`mvn compile` 编译
`mvn test`  执行测试命令
`mvn package` 打包,打成jar/war 包
`mvn install` 将开放的模块放入本地仓库，供其他模块使用
`mvn clean` 清楚target目录(删除打包文件)

通过 `nexus` 搭建私服

3. 依赖的范围，依赖的有效型

```
                compile           test            provided
主程序(main)     是                 否              是

测试程序(test)   是                 是              是

部署(运行)       是                 否              否
```

依赖排除

```
<exclusions>
    <exclusion>
        <groupId></groupId>
        <artifactId></artifactId>
    <exclusion>
<exclusions>
```

4. 在Eclipse中创建maven工程,需要配置Eclipse 的Maven信息

5.  maven 的`生命周期` 跟构建的关系

三个阶段

`clean lifecycle` : 清理, pre-clean  clean   post-clean

`default lifecycle` : 默认（最常用）

`site lifecycle` ：站点, pre-site  site  post-site site-deploy

