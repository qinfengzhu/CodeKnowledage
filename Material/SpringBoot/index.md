####  springboot

1. Eclipse中如何启动Spring Boot 项目

找到SpringBoot 中启动的Application ,右键 Run as  Java Application 就好了

![SpringBoot启动方式](run-springBoot.png)

访问的话，就是直接找到`application.properties` ，这里配置如下,`server.port`是重点，如下是`8080`,上面application运行后，就可以在
浏览器直接访问 `http://localhost:8080` ;当看到文件中有unicode码的时候，请进行 右键该文件，然后 修改 Text File encoding 为UTF-8

```
#启动的端口号
server.port=8080
#登录超时-秒
server.servlet.session.timeout=36000
#数据库连接
spring.datasource.url=jdbc:mysql://127.0.0.1:3306/naying?useUnicode=true&characterEncoding=utf8&useCursorFetch=true&defaultFetchSize=500&allowMultiQueries=true&rewriteBatchedStatements=true&useSSL=false
spring.datasource.driverClassName=com.mysql.jdbc.Driver
spring.datasource.username=root
spring.datasource.password=Abcd1234
#站点资源配置
project=src/main/java
resource=src/main/resources
web.front.baseDir=erp_web
mybatis.type-aliases-package=com.jsh.erp.datasource.entities.*
mybatis.mapper-locations=classpath:./mapper_xml/*.xml
#mybatis-plus配置
mybatis-plus.mapper-locations=classpath:./mapper_xml/*.xml
#租户对应的角色id
manage.roleId=10
#租户允许创建的用户数
tenant.userNumLimit=5
#租户允许创建的单据数
tenant.billsNumLimit=200
#插件配置
plugin.runMode=prod
plugin.pluginPath=plugins
plugin.pluginConfigFilePath=pluginConfig
```
