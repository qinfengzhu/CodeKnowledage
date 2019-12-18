### C#中常用的缓存以及原理

1. `System.Web.Caching.HttpRuntime.Cache` ，通过 `HttpContext.Cache` 来进行获取,或者 `HttpRuntime.Cache` 来进行操作。主要Web使用

2. `System.Runtime.Caching.MemoryCache`

3. `System.Runtime.Caching.ObjectCache`


### 比较有意思的框架 [CacheManager](https://github.com/MichaCo/CacheManager) ,[LazyCache](https://github.com/alastairtree/LazyCache) ,[FluentCache](https://github.com/cordialgerm/FluentCache)
