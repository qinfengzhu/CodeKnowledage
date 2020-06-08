#### `fd-biotech` 整体设计

> 功能页面需求分析

1. 首页头部区域

* 公司Logo

* 标题与头部

* 导航栏

2. 宣传轮播区

* 轮播图片

3. 商品区域

* 新商品(展示的都是Wrap商品,对应的有商品明细及价格)

对商品思考的扩展: 1. 商品有明细，价钱是从明细开始计算，购物车中放的也是明细；2.商品有 `技术指标`,`提供者`(站点提供，提供者信息),`参考文献` (有附件,后端编辑可以提供文献资料下载),`相关产品`（后端自己设置）

* 特殊材料

4. 合作单位

* 展示新成员

5. 站点一些服务及条款说明（站点支持文章等说明编辑）

6. [站点颜色搭配](http://tool.c7sky.com/webcolor/#character_0) ;[站点颜色搭配2](http://brandcolors.net/) 进行参考

7. [全仿照站点](https://www.kerafast.com/)


> 技术选型

1. ORM 选择 `https://smartsql.net/guide/#%E5%A5%B9%E6%98%AF%E5%A6%82%E4%BD%95%E5%B7%A5%E4%BD%9C%E7%9A%84%EF%BC%9F`  或者 `https://www.cnblogs.com/FreeSql/p/11531300.html`

2. 后台UI框架选择 [LayuiMini](https://github.com/qinfengzhu/layuimini)

3. 后台后端语言选择 .net core

4. 前端框架选择 jquery + bootstrap

> 数据库设计

1. 数据库使用  `mariadb`，与 `mysql` 同款
