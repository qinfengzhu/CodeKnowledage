### webapi文件下载

1. 核心操作代码

```
ar = new HttpResponseMessage(HttpStatusCode.OK);
ar.Content = new StreamContent(stream....);
string fileName = string.Format("{0}.xlsx", "测试excel");
Encoding encoding = System.Text.Encoding.Default;
fileName = GetBrowserFileName(fileName, ref encoding); //文件名编码部分
HttpContext.Current.Response.ContentEncoding = encoding;
ar.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
ar.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
{
    FileName = fileName
};
```
2. 文件导出根据浏览器，对文件名进行编码部分
```
private string GetBrowserFileName(string fileName,ref Encoding encoding)
{
    string outputFileName = null;
    string browser = HttpContext.Current.Request.UserAgent.ToUpper();
    if (browser.Contains("MS") == true && browser.Contains("IE") == true)
    {
        outputFileName = HttpUtility.UrlEncode(fileName);
        encoding = System.Text.Encoding.Default;
    }
    else if (browser.Contains("FIREFOX") == true)
    {
        outputFileName = fileName;
        encoding = System.Text.Encoding.GetEncoding("GB2312");
    }
    else
    {
        outputFileName = HttpUtility.UrlEncode(fileName);
        encoding = System.Text.Encoding.Default;
    }
    return outputFileName;
}
```
3. Http协议中文件下载的标准解析
