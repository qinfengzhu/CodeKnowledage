# 数据库行转列

```
SELECT tb.SiteName,
 max(CASE tb.Status WHEN 0 THEN tb.Total ELSE 0 end)InitNum,
 max(CASE tb.Status WHEN 1 THEN tb.Total ELSE 0 end)FilteringNum,
 max(CASE tb.Status WHEN 2 THEN tb.Total ELSE 0 end)FilterFailNum,
 max(CASE tb.Status WHEN 3 THEN tb.Total ELSE 0 end)EnrollNum,
 max(CASE tb.Status WHEN 5 THEN tb.Total ELSE 0 end)RandomNum,
 max(CASE tb.Status WHEN 6 THEN tb.Total ELSE 0 end)FailEventViewNum,
 max(CASE tb.Status WHEN 7 THEN tb.Total ELSE 0 end)CompletedNum
FROM
(
    Select s.Name SiteName,IsNull(t.Status,0) Status,Count(t.Id) Total
    From SubjectData t Right Join Site s on t.SiteId=s.Id
    Group By s.Name,t.Status
)tb
Group By tb.SiteName
```

# 网上具体的明确实例

[一个比较具体的实例](https://www.cnblogs.com/no27/p/6398130.html)
