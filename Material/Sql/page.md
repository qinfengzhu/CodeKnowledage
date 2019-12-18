### Sql Server 分页

1. SqlServer 2012以下

```
Select *
From(
        Select Id,Row_Number() over(Order By CreateTime Asc) as rownumber
        Form Articles
    ) t
Where t.rownumber between 1 And 10
```

2. SqlServer 2012

```
Select Id,Row_Number() over(Order By CreateTime Asc) as rownumber
Form Articles
Order By rownumber offset 0 Rows Fetch Next 10 Rows Only
```
