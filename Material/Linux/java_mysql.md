### 排查JavaWeb项目问题以及Mysql备份

1. 查看Tomcat安装路径

```
find / -name *tomcat*
```
2. 查看Tomcat是否启动

```
ps aux|grep tomcat
```

3. 启动Tomcat，作为服务启动,在`Tomcat`根目录下的`bin`

```
nohup bin/startup.sh &
```
4. 备份mysql数据库(如数据库为:myweb)

```
mysqldump -u root -p myweb > /var/myweb.sql
```
然后输入root用户的密码，记住这是mysql的管理员用户root的密码

5. 备份好后，如何把备份的东西下载下来,这里对Window或者linux都好实用,尤其是使用`xshell`进行远程
登录Linux 服务器的方式
```
Centos:   yum install -y lrzsz
Ubuntu:   apt install lrzsz

rz /var/myweb.sql

6. 把备份的数据文件进行还原

先进入mysql 控制端, mysql -u root -p myweb;
然后执行命令  source /var/myweb.sql

这样就可以还原之前备份的数据库了

```
