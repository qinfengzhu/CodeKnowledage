
### Web请求的目前.net类库支持的种类

`WebClient` ,`HttpWebRequest`,`HttpClient` ,目前我们讲的是 `restful`调用最后用到的底层类.
其实 `HttpWebRequest` 继承自 `WebRequest`,而`WebRequest` 又是抽象类，它的具体实现者不仅仅只有 `HttpWebRequest` 还有 `FileWebRequest` ,`FtpWebRequest`


#### `HttpWebRequest` 可以控制 请求/响应的每个方面，如超时、Cookie、头文件、协议，另一个就是不会阻塞用户界面线程

具体使用方式，代码如下,最少5行代码
```csharp
HttpWebRequest http = (HttpWebRequest)WebRequest.Create("http://example.com");
WebResponse response = http.GetResponse();

MemoryStream stream = response.GetResponseStream();
StreamReader sr = new StreamReader(stream);
string content = sr.ReadToEnd();
```

#### `WebClient` 是为了简化`HttpWebRequest`的任务而构建的更高级别的抽象，与直接调用`HttpWebRequest`相比，可能会更慢()大约几毫秒

更简单的调用方式,代码如下
```csharp
var client = new WebClient();
var text = client.DownloadString("http://example.com/page.html");
```

#### `HttpClient` 提供强大的功能，对更新的线程功能提供更好的语法支持,例如支持 `await`
，缺点就是需要 `.Net Framework 4.5`


### 一个全新的类库 `RestSharp` 支持所有的平台

```csharp
ServicePointManager.UseNagleAlgorithm = false; //设置了才会提高性能
```

### 结论就是

1. HttpWebRequest 控制更具体
2. WebClient 更简单调用
3. RestSharp 兼容 .net 4.5与.net 4.5以下，控制也更自由
4. HttpClient 控制更具体，还支持async 新线程特性，不过要.net 4.5以上