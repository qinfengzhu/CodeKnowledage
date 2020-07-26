#### `Java Web` 站点部署

1. `JDK` 部署, 以下载好的包为例 `jdk-8u251-linux-x64.tar.gz`

```
#解压tar.gz包
tar -zxvf  jdk-8u251-linux-x64.tar.gz

#配置环境变量 JAVA_HOME ,/etc/profile.d/javaConfig.sh 文件
#java config
export JAVA_HOME=/usr/local/jdk1.8.0_251
export CLASSPATH=.:${JAVA_HOME}/jre/lib/rt.jar:${JAVA_HOME}/lib/dt.jar:${JAVA_HOME}/lib/tools.jar
export PATH=$PATH:${JAVA_HOME}/bin

#重新加载 Profile文件
source /etc/profile
```

2. `Tomcat` 部署, 以下载好的包为例 `apache-tomcat-8.5.57.tar.gz`

```
#解压 tar.gz包
tar -zxvf apache-tomcat-8.5.57.tar.gz

#配置命令符加到 环境变量中去
#tomcat config
export CATALINA_BASE=/usr/local/apache-tomcat-8.5.57
export PATH=${CATALINA_BASE}/bin:$PATH

```

3. `Tomcat` 中站点那点配置,准备包 `blog.war` ，监听8080端口

```
<Host name="localhost"  appBase="webapps" unpackWARs="true" autoDeploy="true">
      <Valve className="org.apache.catalina.valves.AccessLogValve" directory="logs"  prefix="localhost_access_log" suffix=".txt" pattern="%h %l %u %t &quot;%r&quot; %s %b" />       
       <Context Path="" docBase="/var/www/bestkf/blog.war" debug="0" privileged="true" reloadable="true" />
</Host>
```

开启 `Tomcat` ， bin下的 `startup.sh` , 执行 `./startup.sh &`

```
1. 启动tomcat服务
方式一：直接启动 ./startup.sh
方式二：作为服务启动 nohup ./startup.sh &
方式三：控制台动态输出方式启动 ./catalina.sh run 动态地显示tomcat后台的控制台输出信息,Ctrl+C后退出并关闭服务
```

4. `Nginx` 配置转发规则,`/etc/nginx/conf.d/bestkf.conf`

```
server {
    listen       80;
    server_name  www.bestkf.site bestkf.site;

    location / {
        proxy_pass http://127.0.0.1:8080;
    }

    error_page   500 502 503 504  /50x.html;
    location = /50x.html {
        root   /usr/share/nginx/html;
    }
}
```
