#### SQL优化

1.  介绍

在应用系统开发初期，由于开发数据库数据比较少，对于查询SQL语句，复杂视图的的编写等体会不出SQL语句各种写法的性能优劣，但是随着互联网大数据的兴起，随着数据库中数据的增加，系统的响应速度就成为目前系统需要解决的最主要的问题之一。

系统优化中一个很重要的方面就是SQL语句的优化。对于海量数据，劣质SQL语句和优质SQL语句之间的速度差别可以达到上百倍，可见对于一个系统不是简单地能实现其功能就可，而是要写出高质量的SQL语句，提高系统的可用性。

在多数情况下，Oracle使用索引来更快地遍历表，优化器主要根据定义的索引来提高性能。但是，如果在SQL语句的where子句中写的SQL代码不合理，就会造成优化器删去索引而使用全表扫描，一般就这种SQL语句就是所谓的劣质SQL语句。在编写SQL语句时我们应清楚优化器根据何种原则来删除索引，这有助于写出高性能的SQL语句。我们要做到不但会写SQL,还要做到写出性能优良的SQL,以下是我工作，学习的经验，汇总了部分资料与大家分享！，如发现不合理或错误的地方请大家及时指出来，以便大家共同成长。

2. 常见优化规则

2.1 表连接数

连接的表越多，性能越差
可能的话，将连接拆分成若干个过程逐一执行
优先执行可显著减少数据量的连接，既降低了复杂度，也能够容易按照预期执行
如果不可避免多表连接，很可能是设计缺陷
外链接效果差，因为必须对左右表进行表扫描
尽量使用inner join查询

2.2 使用临时表

2.3 少用子查询

2.4 视图嵌套

不要过深,一般视图嵌套不要超过2个为宜

3. SQL编写注意事项

3.1 NULL列

Null列使用索引没有意义，任何包含null值的列都不会被包含在索引中。因此where语句中的is null或is not null的语句优化器是不允许使用索引的。

3.2 concat或||

concat或||是mysql和oracle的字符串连接操作，如果对列进行该函数操作，那么也开会忽略索引的使用。比较下面的查询语句

```
-- 忽律索引
select ... from .. where first_name || '' || last_name = 'bill gates' ;
-- 使用索引
select ... from .. where first_name = 'bill' and last_name = 'bill gates' ;
```

3.3 Like

```
-- 无法使用索引
select .. from .. where name like '%t%' ;
-- 可以使用索引
select .. from .. where name like 't%' ;
```

3.4 Order by

order by子句中不要使用非索引列或嵌套表达式，这样都会导致性能降低。

3.5 Not 运算

not运算无法使用索引，可以改成其他能够使用索引的操作。如下：

```
-- 索引无效
select .. from .. where sal != 3000 ;
-- 索引生效
select .. from .. where sal < 3000  or sal > 3000;
```

3.6 where与having

select .. from .. on .. where .. group by .. having .. order by .. limit ..，以上是sql语句的语法结构，其中on、where和having是有过滤行为的，过滤行为越能提前完成就越可以减少传递给下一个阶段的数据量，因此如果在having中的过滤行为能够在where中完成，则应该优先考虑where来实现。

3.7 exists替代in

not in是最低效的，因为要对子查询的表进行全表扫描。可以考虑使用外链接或not exists。如下

```
-- 正确
SELECT  *
FROM EMP
WHERE  
    EMPNO > 0
    AND  EXISTS (SELECT ‘X' FROM DEPT WHERE DEPT.DEPTNO = EMP.DEPTNO AND LOC = ‘MELB')

-- 错误
SELECT  *
FROM  EMP
WHERE  EMPNO > 0  AND  DEPTNO IN(SELECT DEPTNO  FROM  DEPT  WHERE  LOC = ‘MELB')
```

3.8 索引

索引的好处可以实现折半查找，时间复杂度是 O(log_{2}n)

，但是也有成本，需要额外的空间存放索引数据，并且每次insert、update和delete都会对索引进行更新，因此会多增加4、5次的磁盘IO。所以给一些不必要使用索引的字段增加索引，会降低系统的性能。对于oracle来讲，SQL语句尽量大写，内部需要向将小写转成大写，再执行。

不要在索引列上使用函数，这样会停止使用索引，进行全表扫描，如下：

```
-- 错误
SELECT … FROM  DEPT  WHERE SAL * 12 > 25000;
-- 正确
SELECT … FROM  DEPT  WHERE SAL > 25000/12;
```

3.9 >与>=

```
-- 直接定位到4的记录(推荐)
select .. from .. where SAL >= 4 ;
-- 先定位到3，再向后找1个(不推荐)
select .. from .. where SAL > 3 ;
```

3.10 Union 代替 Or

在索引列上，可以使用union替换or操作。索引列上的or操作会造成全表扫描。

```
-- 高效:
SELECT LOC_ID , LOC_DESC , REGION FROM LOCATION WHERE LOC_ID = 10
UNION
SELECT LOC_ID , LOC_DESC , REGION FROM LOCATION WHERE REGION = 'MELBOURNE'

-- 低效:
SELECT LOC_ID ,LOC_DESC ,REGION FROM LOCATION WHERE LOC_ID=10 OR REGION ='MELBOURNE'
```

3.11  is null & is not null

如果列可空，避免使用索引。对于多个列使用的索引，起码保证至少有个列不为空。对于多列索引，只有访问了第一个列才会启用索引，如果访问后面的列则使用的是全表扫描。

```
-- 低效: (索引失效)
SELECT .. FROM  DEPARTMENT  WHERE  DEPT_CODE IS NOT NULL;
-- 高效: (索引有效)
SELECT .. FROM  DEPARTMENT  WHERE  DEPT_CODE >=0;
```

3.12 union & union all

union具有去重的操作，增加了计算时间。union all不需要去重，但会包含相同记录。同样功能下，首选union all操作。
