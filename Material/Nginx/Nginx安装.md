#### Nginx 在Centos上安装

> nginx 安装

```
rpm -Uvh http://nginx.org/packages/centos/7/noarch/RPMS/nginx-release-centos-7-0.el7.ngx.noarch.rpm
yum install -y nginx
```

> nginx 开启监听，并且配置

```
/var/www/html/fd-biotech    #站点位置
/var/webcert/*   fd-biotech.com_chain.crt   fd-biotech.com.key     fd-biotech.com_public.crt #公钥与私钥
/usr/bin/php  #php 位置
```
> 关闭apache 开启 nginx

```
systemctl stop httpd.service
systemctl start nginx.service
systemctl disable httpd.service
systemctl enable nginx.service
```

1. nginx 绑定域名

2. nginx 配置转发

3. nginx 配合apache
