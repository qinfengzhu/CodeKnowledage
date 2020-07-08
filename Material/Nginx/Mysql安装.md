#### 这里使用 `Mariadb` 替代 `Mysql`

1. 以 `Centos 7.5` 为例,第一步安装

```
yum install mariadb-server
```

2. 配置MariaDB

```
systemctl start mariadb
systemctl enable mariadb
```

```
进行第一次Mariadb配置

mysql_secure_installation

Enter current password for root (enter for none):  # 输入数据库超级管理员root的密码(注意不是系统root的密码)，第一次进入还没有设置密码则直接回车
Set root password? [Y/n]  # 设置密码，y
New password:  # 新密码
Re-enter new password:  # 再次输入密码
Remove anonymous users? [Y/n]  # 移除匿名用户， y
Disallow root login remotely? [Y/n]  # 拒绝root远程登录，n，不管y/n，都会拒绝root远程登录
Remove test database and access to it? [Y/n]  # 删除test数据库，y：删除。n：不删除，数据库中会有一个test数据库，一般不需要
Reload privilege tables now? [Y/n]  # 重新加载权限表，y。或者重启服务也许
```

3. 测试是否安装成功

```
mysql -u root -p
Enter password:

MariaDB >
```

4. 设置字符集为 utf-8

```
#文件 /etc/my.cnf ,在 [mysqld] 标签下添加
init_connect='SET collation_connection = utf8_unicode_ci'
init_connect='SET NAMES utf8'
character-set-server=utf8
collation-server=utf8_unicode_ci
skip-character-set-client-handshake
```

```
#文件/etc/my.cnf.d/client.cnf 文件，在 [client] 标签下添加
default-character-set=utf8
 ```

 ```
 #文件 /etc/my.cnf.d/mysql-clients.cnf文件,在 [mysql] 标签下添加
 default-character-set=utf8
 ```

 5. 重启服务

 ```
 systemctl restart mariadb
 ```
