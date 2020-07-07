#### 搭建Spring-MVC项目框架

1.  `Spring`中包之间的依赖关系

![依赖关系](images/spring-dependency.jpg)

所以当建Spring MVC项目的时候

```
<?xml version="1.0" encoding="UTF-8"?>

<project xmlns="http://maven.apache.org/POM/4.0.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 http://maven.apache.org/xsd/maven-4.0.0.xsd">
    <modelVersion>4.0.0</modelVersion>
    <groupId>site.bestkf</groupId>
    <artifactId>sms</artifactId>
    <version>1.0</version>
    <packaging>war</packaging>
    <name>simple ssm web app</name>
    <url>http://www.bestkf.site</url>

    <properties>
        <springmvc.version>5.1.7.RELEASE</springmvc.version>
        <commons-fileupload.version>1.4</commons-fileupload.version>
        <jackson-databind.version>2.9.10</jackson-databind.version>
        <spring-jdbc.version>5.1.7.RELEASE</spring-jdbc.version>
        <mybaits.version>3.5.1</mybaits.version>
        <pagehelper.version>5.1.9</pagehelper.version>
        <mybatis-spring.version>2.0.1</mybatis-spring.version>
        <javax.servlet-api.version>3.1.0</javax.servlet-api.version>
        <jstl.version>1.2</jstl.version>
        <mysql-connector-java.version>8.0.16</mysql-connector-java.version>
        <c3p0.version>0.9.5.4</c3p0.version>
        <commons-logging.version>1.2</commons-logging.version>
        <log4j2.version>2.12.1</log4j2.version>
        <compiler.source>1.8</compiler.source>
        <compiler.target>1.8</compiler.target>
        <buildHelperPlugin.version>1.8</buildHelperPlugin.version>
    </properties>

    <dependencies>
        <!-- spring mvc -->
        <dependency>
            <groupId>org.springframework</groupId>
            <artifactId>spring-webmvc</artifactId>
            <version>${springmvc.version}</version>
        </dependency>
        <!-- Spring mvc:JSON -->
        <dependency>
            <groupId>com.fasterxml.jackson.core</groupId>
            <artifactId>jackson-databind</artifactId>
            <version>${jackson-databind.version}</version>
        </dependency>
        <!-- Spring mvc:file -->
        <dependency>
            <groupId>commons-fileupload</groupId>
            <artifactId>commons-fileupload</artifactId>
            <version>${commons-fileupload.version}</version>
        </dependency>
        <!-- Spring JDBC -->
        <dependency>
            <groupId>org.springframework</groupId>
            <artifactId>spring-jdbc</artifactId>
            <version>${spring-jdbc.version}</version>
        </dependency>
        <!-- MyBatis -->
        <dependency>
            <groupId>org.mybatis</groupId>
            <artifactId>mybatis</artifactId>
            <version>${mybaits.version}</version>
        </dependency>
        <!-- MyBatis 分页插件 -->
        <dependency>
            <groupId>com.github.pagehelper</groupId>
            <artifactId>pagehelper</artifactId>
            <version>${pagehelper.version}</version>
        </dependency>
        <!-- Mybatis与Spring的整合包 -->
        <dependency>
            <groupId>org.mybatis</groupId>
            <artifactId>mybatis-spring</artifactId>
            <version>${mybatis-spring.version}</version>
        </dependency>
        <!-- Servelt API -->
        <dependency>
            <groupId>javax.servlet</groupId>
            <artifactId>javax.servlet-api</artifactId>
            <version>${javax.servlet-api.version}</version>
            <scope>provided</scope>
        </dependency>
        <!-- JSTL -->
        <dependency>
            <groupId>javax.servlet</groupId>
            <artifactId>jstl</artifactId>
            <version>${jstl.version}</version>
        </dependency>
        <!-- 数据库驱动 -->
        <dependency>
            <groupId>mysql</groupId>
            <artifactId>mysql-connector-java</artifactId>
            <version>${mysql-connector-java.version}</version>
        </dependency>
        <!-- 数据库连接池 -->
        <dependency>
            <groupId>com.mchange</groupId>
            <artifactId>c3p0</artifactId>
            <version>${c3p0.version}</version>
        </dependency>
        <!-- 日志组件 -->
        <dependency>
            <groupId>commons-logging</groupId>
            <artifactId>commons-logging</artifactId>
            <version>${commons-logging.version}</version>
        </dependency>
        <dependency>
   			<groupId>org.apache.logging.log4j</groupId>
    		<artifactId>log4j-core</artifactId>
		    <version>${log4j2.version}</version>
		</dependency>
		<dependency>
		    <groupId>org.apache.logging.log4j</groupId>
		    <artifactId>log4j-web</artifactId>
		    <version>${log4j2.version}</version>
		</dependency>
    </dependencies>

    <build>
        <finalName>sms</finalName>
        <plugins>
          <!--编译与打包的时候走配置的JDK打包编译-->
  				<plugin>
  					<groupId>org.apache.maven.plugins</groupId>
  					<artifactId>maven-compiler-plugin</artifactId>
  					<version>2.3.2</version>
  					<configuration>
  						<source>${compiler.source}</source>
  						<target>${compiler.target}</target>
  					</configuration>
  				</plugin>
          <!--打包的时候排除或者包含指定文件-->
          <plugin>
            <groupId>org.codehaus.mojo</groupId>
            <artifactId>build-helper-maven-plugin</artifactId>
            <version>${buildHelperPlugin.version}</version>
            <executions>
              <execution>
                <id>add-resource</id>
                <phase>generate-resources</phase>
                <goals>
                  <goal>add-resource</goal>
                </goals>
                <configuration>
                  <resources>
                    <resource>
                      <directory>src/main/resource</directory>
                      <includes>
                        <include>**/*</include>
                      </includes>
                    </resource>
                  </resources>
                </configuration>
              </execution>
            </executions>
          </plugin>
        </plugins>
    </build>
</project>

```