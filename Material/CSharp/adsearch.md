### 域用户验证,核心代码，需要引用 `System.DirectoryServices`

```
bool rightADUser = false;
try
{
    using (DirectoryEntry entry = new DirectoryEntry(@"LDAP://172.16.66.109", "ladp-test5", "Abcd1234", AuthenticationTypes.None))
    {
        DirectorySearcher src = new DirectorySearcher(entry);
        src.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + "ladp-test5" + "))";
        src.PropertiesToLoad.Add("cn");
        src.SearchRoot = entry;
        src.SearchScope = SearchScope.Subtree;
        SearchResult result = src.FindOne();
        if(result!=null)
        {
            rightADUser = true;
        }
    }
}catch(Exception ex)
{
    rightADUser = false;
}
```
