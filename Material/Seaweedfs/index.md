SeaweedFS是一种简单的、高度可扩展的分布式文件系统。有两个目标:　　

存储数十亿的文件!　　   storage billions of files　　
查看档案快!                     serve the files fast
weed-fs起初是为了搞一个基于Fackbook的Haystack论文的实现，Haystack旨在优化Fackbook内部图片存储和获取。

后来这个基础上，weed-fs作者又增加了若干feature，形成了目前的weed-fs。

SeaweedFS最初作为一个对象存储来有效地处理小文件。中央主服务器只管理文件卷，而不是管理中央主服务器中的所有文件元数据，它允许这些卷服务器管理文件及其元数据。这减轻了中央主服务器的并发压力，并将文件元数据传播到卷服务器，允许更快的文件访问(只需一个磁盘读取操作)。

每个文件的元数据只有40字节的磁盘存储开销。使用O(1)磁盘读取非常简单。
Githup地址为： https://github.com/chrislusf/seaweedfs
官方文档：https://github.com/chrislusf/seaweedfs/wiki
相关背景技术论文：
　　中文版： http://www.importnew.com/3292.html
　　英文版： http://static.usenix.org/event/osdi10/tech/full_papers/Beaver.pdf
安装
有两种安装方式，第一种下载编译好的直接使用，第二种是下载源码进行编译。由于暂时不更改源码， 故使用第一种方式运行
下载地址：https://github.com/chrislusf/seaweedfs/releases 　　　　下载完成后，解压出来直接运行。


作者在官网上将weed-fs与其他分布式文件系统如Ceph，hdfs等做了简要对比，强调了weed-fs相对于其他分布式文件系统的优点。
weed-fs没有提供官方client包，但在wiki上列出多种第三方client包（各种语言），就Go client包来看，似乎还没有特别理想的。
