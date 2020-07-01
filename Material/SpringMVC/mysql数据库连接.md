#### com.mysql.jdbc.Driver 和 com.mysql.cj.jdbc.Driver的区别

com.mysql.jdbc.Driver 是 mysql-connector-java 5中的，
com.mysql.cj.jdbc.Driver 是 mysql-connector-java 6中的

1. JDBC连接Mysql5 com.mysql.jdbc.Driver:

```verClassName=com.mysql.jdbc.Driver
url=jdbc:mysql://localhost:3306/test?useUnicode=true&characterEncoding=utf8&useSSL=false
username=root
password=
```

2. JDBC连接Mysql6 com.mysql.cj.jdbc.Driver， 需要指定时区serverTimezone:

```
driverClassName=com.mysql.cj.jdbc.Driver
url=jdbc:mysql://localhost:3306/test?serverTimezone=UTC&?useUnicode=true&characterEncoding=utf8&useSSL=false
username=root
password=
```
在设定时区的时候，如果设定serverTimezone=UTC，会比中国时间早8个小时，如果在中国，可以选择Asia/Shanghai或者Asia/Hongkong，例如：

```
driverClassName=com.mysql.cj.jdbc.Driver
url=jdbc:mysql://localhost:3306/test?serverTimezone=Shanghai&?useUnicode=true&characterEncoding=utf8&useSSL=false
username=root
password=
```
