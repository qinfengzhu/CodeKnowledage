####  `mybatis`单独配置

1. `Pom.xml` 文件

```
<properties>
    <junit.version>4.13</junit.version>
  	<mybatis.version>3.5.5</mybatis.version>
  	<mysqlconnector.version>5.1.40</mysqlconnector.version>
  	<log4j.version>2.13.3</log4j.version>
</properties>
<dependencies>
    <!--Junit单元测试-->
    <dependency>
	    <groupId>junit</groupId>
	    <artifactId>junit</artifactId>
	    <version>${junit.version}</version>
	    <scope>test</scope>
	</dependency>
  	<!--mybatis的基本配置-->
	<dependency>
	    <groupId>org.mybatis</groupId>
	    <artifactId>mybatis</artifactId>
	    <version>${mybatis.version}</version>
	</dependency>
	<dependency>
	    <groupId>MySQL</groupId>
	    <artifactId>mysql-connector-java</artifactId>
	    <version>${mysqlconnector.version}</version>
	</dependency>
	<dependency>
	    <groupId>org.apache.logging.log4j</groupId>
	    <artifactId>log4j-core</artifactId>
	    <version>${log4j.version}</version>
	</dependency>
	<dependency>
	    <groupId>org.apache.logging.log4j</groupId>
	    <artifactId>log4j-api</artifactId>
	    <version>${log4j.version}</version>
	</dependency>
</dependencies>
```

2. `jdbc_mysql.properties` 文件内容

```
jdbc.driver=com.mysql.jdbc.Driver
jdbc.url=jdbc:mysql://127.0.0.1:3306/bestkf
jdbc.user=root
jdbc.password=Abcd1234
```

3. `log4j2.xml` 文件内容

```
<?xml version="1.0" encoding="UTF-8"?>
<configuration status="error">
    <!--先定义所有的appender -->
    <appenders>
        <!--这个输出控制台的配置 -->
        <Console name="Console" target="SYSTEM_OUT">
            <!--             控制台只输出level及以上级别的信息（onMatch），其他的直接拒绝（onMismatch） -->
            <ThresholdFilter level="trace" onMatch="ACCEPT" onMismatch="DENY"/>
            <!--             这个都知道是输出日志的格式 -->
            <PatternLayout pattern="%d{HH:mm:ss.SSS} %-5level %class{36} %L %M - %msg%xEx%n"/>
        </Console>

        <!--文件会打印出所有信息，这个log每次运行程序会自动清空，由append属性决定，这个也挺有用的，适合临时测试用 -->
        <!--append为TRUE表示消息增加到指定文件中，false表示消息覆盖指定的文件内容，默认值是true -->
        <File name="log" fileName="logs/log4j2.log" append="false">
            <PatternLayout pattern="%d{HH:mm:ss.SSS} %-5level %class{36} %L %M - %msg%xEx%n"/>
        </File>

        <!--添加过滤器ThresholdFilter,可以有选择的输出某个级别以上的类别  onMatch="ACCEPT" onMismatch="DENY"意思是匹配就接受,否则直接拒绝  -->
        <File name="ERROR" fileName="logs/error.log">
            <ThresholdFilter level="error" onMatch="ACCEPT" onMismatch="DENY"/>
            <PatternLayout pattern="%d{yyyy.MM.dd 'at' HH:mm:ss z} %-5level %class{36} %L %M - %msg%xEx%n"/>
        </File>

        <!--这个会打印出所有的信息，每次大小超过size，则这size大小的日志会自动存入按年份-月份建立的文件夹下面并进行压缩，作为存档 -->
        <RollingFile name="RollingFile" fileName="D:/logs/web.log"
                     filePattern="logs/$${date:yyyy-MM}/web-%d{MM-dd-yyyy}-%i.log.gz">
            <PatternLayout pattern="%d{yyyy-MM-dd 'at' HH:mm:ss z} %-5level %class{36} %L %M - %msg%xEx%n"/>
            <SizeBasedTriggeringPolicy size="2MB"/>
        </RollingFile>
    </appenders>

    <!--然后定义logger，只有定义了logger并引入的appender，appender才会生效 -->
    <loggers>
        <root level="trace">
            <appender-ref ref="RollingFile"/>
            <appender-ref ref="Console"/>
            <appender-ref ref="ERROR" />
            <appender-ref ref="log"/>
        </root>
    </loggers>
</configuration>
```

4. `mybatis-config.xml` 文件内容

```
<?xml version="1.0" encoding="UTF-8" ?>
<!DOCTYPE configuration
  PUBLIC "-//mybatis.org//DTD Config 3.0//EN"
  "http://mybatis.org/dtd/mybatis-3-config.dtd">
<configuration>
  <properties resource="jdbc_mysql.properties"></properties>
  <settings>
  	<setting name="logImpl" value="LOG4J2"/>
  </settings>
  <environments default="development">
    <environment id="development">
      <transactionManager type="JDBC"/>
      <dataSource type="POOLED">
        <property name="driver" value="${jdbc.driver}"/>
        <property name="url" value="${jdbc.url}"/>
        <property name="username" value="${jdbc.user}"/>
        <property name="password" value="${jdbc.password}"/>
      </dataSource>
    </environment>
  </environments>
  <mappers>
<!--  	<mapper resource="mappers/StudentMapper.xml"/> -->
	<mapper class="simple.mybaits.dao.StudentMapper"/>
  </mappers>
</configuration>
```
