### 设置阿里源以及配置静态ip

1. 配置阿里源
```
yum install -y net-tools
wget -O /etc/yum.repos.d/CentOS-Base.repo http://mirrors.aliyun.com/repo/Centos-7.repo
yum clean all
yum makecache
```

2. 配置网络地址, /etc/sysconfig/network-scripts/ifcfg-ens33
```
TYPE=Ethernet
PROXY_METHOD=none
BROWSER_ONLY=no
DEFROUTE=yes
IPV4_FAILURE_FATAL=no
IPV6INIT=yes
IPV6_AUTOCONF=yes
IPV6_DEFROUTE=yes
IPV6_FAILURE_FATAL=no
IPV6_ADDR_GEN_MODE=stable-privacy
NAME=ens33
UUID=1b1733af-efa9-4ab4-9848-a480dca5cb43
DEVICE=ens33
#VMWare的NAT模式下
BOOTPROTO=static
ONBOOT=yes
IPV6_PRIVACY=no
IPADDR=192.168.31.101
NETMASK=255.255.255.0
GATEWAY=192.168.31.2
DNS1=192.168.31.2
NM_CONTROLLED=no
```

重启网络服务 `systemctl restart network`
