#### Fastfds 具体安装

# 环境准备
## 使用的系统软件
| 名称         | 说明           |
|:--|-|
| centos|7.x|
|libfatscommon|FastDFS分离出的一些公用函数包|
|FastDFS|FastDFS本体|
|fastdfs-nginx-module|FastDFS和nginx的关联模块|
|nginx|nginx1.15.4|
## 编译环境
```shell
yum install git gcc gcc-c++ make automake autoconf libtool pcre pcre-devel zlib zlib-devel openssl-devel wget vim -y
```
## 磁盘目录
|说明|位置|
|-|-|
|所有安装包|/usr/local/src|
|数据存储位置|/home/dfs/|
#这里我为了方便把日志什么的都放到了dfs
```shell
mkdir /home/dfs #创建数据存储目录
cd /usr/local/src #切换到安装目录准备下载安装包
```
## 安装libfatscommon
```shell
git clone https://github.com/happyfish100/libfastcommon.git --depth 1
cd libfastcommon/
./make.sh && ./make.sh install #编译安装
```
## 安装FastDFS
```shell
cd ../ #返回上一级目录
git clone https://github.com/happyfish100/fastdfs.git --depth 1
cd fastdfs/
./make.sh && ./make.sh install #编译安装
#配置文件准备
cp /usr/etc/fdfs/tracker.conf.sample /etc/fdfs/tracker.conf
cp /usr/etc/fdfs/storage.conf.sample /etc/fdfs/storage.conf
cp /usr/etc/fdfs/client.conf.sample /etc/fdfs/client.conf #客户端文件，测试用
cp /usr/local/src/fastdfs/conf/http.conf /etc/fdfs/ #供nginx访问使用
cp /usr/local/src/fastdfs/conf/mime.types /etc/fdfs/ #供nginx访问使用
```
## 安装fastdfs-nginx-module
```shell
cd ../ #返回上一级目录
git clone https://github.com/happyfish100/fastdfs-nginx-module.git --depth 1
cp /usr/local/src/fastdfs-nginx-module/src/mod_fastdfs.conf /etc/fdfs
```
## 安装nginx
```shell
wget http://nginx.org/download/nginx-1.15.4.tar.gz #下载nginx压缩包
tar -zxvf nginx-1.15.4.tar.gz #解压
cd nginx-1.15.4/
#添加fastdfs-nginx-module模块
./configure --add-module=/usr/local/src/fastdfs-nginx-module/src/
make && make install #编译安装
```
# 单机部署

## tracker配置

```shell
#服务器ip为 192.168.52.1
#我建议用ftp下载下来这些文件 本地修改
vim /etc/fdfs/tracker.conf
#需要修改的内容如下
port=22122  # tracker服务器端口（默认22122,一般不修改）
base_path=/home/dfs  # 存储日志和数据的根目录
```
## storage配置
```shell
vim /etc/fdfs/storage.conf
#需要修改的内容如下
port=23000  # storage服务端口（默认23000,一般不修改）
base_path=/home/dfs  # 数据和日志文件存储根目录
store_path0=/home/dfs  # 第一个存储目录
tracker_server=192.168.52.1:22122  # tracker服务器IP和端口
http.server_port=8888  # http访问文件的端口(默认8888,看情况修改,和nginx中保持一致)
```
## client测试
```shell
vim /etc/fdfs/client.conf
#需要修改的内容如下
base_path=/home/dfs
tracker_server=192.168.52.1:22122    #tracker服务器IP和端口
#保存后测试,返回ID表示成功 如：group1/M00/00/00/xx.tar.gz
fdfs_upload_file /etc/fdfs/client.conf /usr/local/src/nginx-1.15.4.tar.gz
```
## 配置nginx访问
```shell
vim /etc/fdfs/mod_fastdfs.conf
#需要修改的内容如下
tracker_server=192.168.52.1:22122  #tracker服务器IP和端口
url_have_group_name=true
store_path0=/home/dfs
#配置nginx.config
vim /usr/local/nginx/conf/nginx.conf
#添加如下配置
server {
    listen       8888;    ## 该端口为storage.conf中的http.server_port相同
    server_name  localhost;
    location ~/group[0-9]/ {
        ngx_fastdfs_module;
    }
    error_page   500 502 503 504  /50x.html;
    location = /50x.html {
    root   html;
    }
}
#测试下载，用外部浏览器访问刚才已传过的nginx安装包,引用返回的ID
http://192.168.52.1:8888/group1/M00/00/00/wKgAQ1pysxmAaqhAAA76tz-dVgg.tar.gz
#弹出下载单机部署全部跑通
```
# 分布式部署

## tracker配置

```shell
#服务器ip为 192.168.52.2,192.168.52.3,192.168.52.4
#我建议用ftp下载下来这些文件 本地修改
vim /etc/fdfs/tracker.conf
#需要修改的内容如下
port=22122  # tracker服务器端口（默认22122,一般不修改）
base_path=/home/dfs  # 存储日志和数据的根目录
```
## storage配置
```shell
vim /etc/fdfs/storage.conf
#需要修改的内容如下
port=23000  # storage服务端口（默认23000,一般不修改）
base_path=/home/dfs  # 数据和日志文件存储根目录
store_path0=/home/dfs  # 第一个存储目录
tracker_server=192.168.52.2:22122  # 服务器1
tracker_server=192.168.52.3:22122  # 服务器2
tracker_server=192.168.52.4:22122  # 服务器3
http.server_port=8888  # http访问文件的端口(默认8888,看情况修改,和nginx中保持一致)
```
## client测试
```shell
vim /etc/fdfs/client.conf
#需要修改的内容如下
base_path=/home/moe/dfs
tracker_server=192.168.52.2:22122  # 服务器1
tracker_server=192.168.52.3:22122  # 服务器2
tracker_server=192.168.52.4:22122  # 服务器3
#保存后测试,返回ID表示成功 如：group1/M00/00/00/xx.tar.gz
fdfs_upload_file /etc/fdfs/client.conf /usr/local/src/nginx-1.15.4.tar.gz
```
## 配置nginx访问
```shell
vim /etc/fdfs/mod_fastdfs.conf
#需要修改的内容如下
tracker_server=192.168.52.2:22122  # 服务器1
tracker_server=192.168.52.3:22122  # 服务器2
tracker_server=192.168.52.4:22122  # 服务器3
url_have_group_name=true
store_path0=/home/dfs
#配置nginx.config
vim /usr/local/nginx/conf/nginx.conf
#添加如下配置
server {
    listen       8888;    ## 该端口为storage.conf中的http.server_port相同
    server_name  localhost;
    location ~/group[0-9]/ {
        ngx_fastdfs_module;
    }
    error_page   500 502 503 504  /50x.html;
    location = /50x.html {
    root   html;
    }
}
```
# 启动
## 防火墙
```shell
#不关闭防火墙的话无法使用
systemctl stop firewalld.service #关闭
systemctl restart firewalld.service #重启
```
## tracker
```shell
/etc/init.d/fdfs_trackerd start #启动tracker服务
/etc/init.d/fdfs_trackerd restart #重启动tracker服务
/etc/init.d/fdfs_trackerd stop #停止tracker服务
chkconfig fdfs_trackerd on #自启动tracker服务
```
## storage
```shell
/etc/init.d/fdfs_storaged start #启动storage服务
/etc/init.d/fdfs_storaged restart #重动storage服务
/etc/init.d/fdfs_storaged stop #停止动storage服务
chkconfig fdfs_storaged on #自启动storage服务
```
## nginx
```shell
/usr/local/nginx/sbin/nginx #启动nginx
/usr/local/nginx/sbin/nginx -s reload #重启nginx
/usr/local/nginx/sbin/nginx -s stop #停止nginx
```
## 检测集群
```shell
/usr/bin/fdfs_monitor /etc/fdfs/storage.conf
# 会显示会有几台服务器 有3台就会 显示 Storage 1-Storage 3的详细信息
```
# 说明

## 配置文件
```shell
tracker_server #有几台服务器写几个
group_name #地址的名称的命名
bind_addr #服务器ip绑定
store_path_count #store_path(数字)有几个写几个
store_path(数字) #设置几个储存地址写几个 从0开始
```
## 可能遇到的问题
```shell
如果不是root 用户 你必须在除了cd的命令之外 全部加sudo
如果不是root 用户 编译和安装分开进行 先编译再安装
如果上传成功 但是nginx报错404 先检查mod_fastdfs.conf文件中的store_path0是否一致
如果nginx无法访问 先检查防火墙 和 mod_fastdfs.conf文件tracker_server是否一致
如果不是在/usr/local/src文件夹下安装 可能会编译出错
```
教程是在上一位huayanYu(小锅盖)的基础上添加了一些东西，本质上还是huayanYu(小锅盖)写的教程
