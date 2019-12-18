### 含有逗号的关联查询

1. 需要查询的数据如下,分别为表`ItemValue`,`CodeListItem`

```
*--表ItemValue--*
Id      ItemValue       CodeListOID
1       1,2,3           Fav
2       2,3             Fav
3       1,3             Fav
4       1               Fav
5       3               Fav
```

```
*--表CodeListItem--*
Id      CodeListOID     Decode
1       Fav             篮球
2       Fav             足球
3       Fav             乒乓球
```

2. 需要查询出来的结果为

```
*--查询结果--*
Id      ItemValue       FavValues
1       1,2,3           篮球,足球,乒乓球
2       2,3             足球,乒乓球
3       1,3             篮球,乒乓球
4       1               篮球
5       3               乒乓球
```

3. 最后的Sql语句为

```
Select t.Id,t.ItemValue,FavValues=
    stuff(
            (
                Select ','+s.Decode
                From CodeListItem s
                Where charindex(','+convert(varchar,s.Id),','+t.ItemValue)>0 for xml path('')
            ),
            1,
            1,
            ''
         )
From ItemValue t
```

4. 分析`stuff`函数与`charindex` 函数

*stuff函数*
`STUFF ( character_expression , start , length , replaceWith_expression ) `,
`character_expression` 字符串表达式,可以是常量、变量,也可以是字符列或二进制数据列
`start` 一个整数值,指定删除和插入的开始位置。如果start为负或为零,则返回空字符串。如果start的长度
大于第一个character_expression，则返回空字符串。
`length` 一个整数,指定要删除的字符数。如果length为负,则返回空字符串。如果length的长度大于第一个
character_expression，则最多可以删除到最后一个character_expression中的最后一个字符。
`replaceWith_expression` 字符数据的表达式,character_expression可以是常量、变量,也可以是字符列或二进制数据列


*charindex函数*
`CHARINDEX ( expressionToFind , expressionToSearch [ , start_location ] ) `,
`expressionToFind` 一个字符串表达式,其中包含要查找的序列。
`expressionToSearch` 要搜索的字符表达式。
`start_location` 表示搜索开始位置的integer或bigint表达式
``
