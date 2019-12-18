### 数据库`ConnctionString`做 `mirroring` 情况

```
 <add name="DefaultConnection" connectionString="Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;
 Initial Catalog=myDataBase;Integrated Security=True;User ID=sa;Password=123456" providerName="System.Data.SqlClient" />
```
