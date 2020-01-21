### IO通信种类

#### 理解同步与异步，阻塞与非阻塞

__同步__：同步就是发起一个调用后，被调用者未处理完请求之前，调用不返回
__异步__: 异步就是发起一个调用后，立刻得到被调用者的回应表示已接收到请求，但是被调用者并没有返回结果，此时我们可以处理其他的请求，被调用者通常依靠事件，回调等机制来通知调用者其返回结果

__阻塞__: 阻塞就是发起一个请求，调用者一直等待请求结果返回，也就是当前线程会被挂起，无法从事其他任务，只有当条件就绪才能继续
__非阻塞__: 非阻塞就是发起一个请求，调用者不用一直等着结果返回，可以先去干其他事情

#### BIO,PIO,NIO

1. `BIO` :同步阻塞IO, 每个线程都处理着一个客户端.客户端与线程数是1:1

![BIO通信模型图](images/bio.png)

通常由一个独立的 Acceptor 线程负责监听客户端的连接。我们一般通过在while(true) 循环中服务端会调用 accept() 方法等待接收客户端的连接的方式监听请求，请求一旦接收到一个连接请求，就可以建立通信套接字在这个通信套接字上进行读写操作，此时不能再接收其他客户端连接请求，只能等待同当前连接的客户端的操作执行完成， 不过可以通过多线程来支持多个客户端的连接。

通常代码如下,初级版本`BIO`,没有实现客户端与线程数1:1，而是N:1,因为处理接受数据与处理的是在主线程中
```
public class BIOServer: IWebServer
{
    public void Run()
    {
        //监听
        var ip = IPAddress.Parse("127.0.0.1");
        int port = 9000;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(ip, port));
        serverSocket.Listen(1000);

        //获取请求链接,Accept会阻塞进程,所以会是一个接着一个去处理来自客户端的请求
        while (true)
        {
            var clientSocket = serverSocket.Accept();  //特点就在这里,这里会被阻塞
            var bufferBytes = new byte[1024];
            try
            {
                int receiveNumber = 0;
                bool firstLoad = true;
                while (firstLoad||receiveNumber==bufferBytes.Length)
                {
                    receiveNumber = clientSocket.Receive(bufferBytes);
                    string message = Encoding.UTF8.GetString(bufferBytes, 0, receiveNumber);
                    Console.WriteLine("浏览器请求消息(长度{0}):\r\n{1}",receiveNumber,message);
                    firstLoad = false;
                }
            }
            finally
            {
                StringBuilder htmlContent = new StringBuilder();
                htmlContent.AppendLine(@"http/1.0 200 OK");
                htmlContent.AppendLine(@"Content-Type:text/html;charset:UTF-8");
                htmlContent.AppendLine();

                htmlContent.AppendLine(@"<html lang=""zh-cn"">");
                htmlContent.AppendLine(@"<head>");
                htmlContent.AppendLine(@"<meta charset=""utf-8""/>");
                htmlContent.AppendLine(@"</head>");
                htmlContent.AppendLine(@"<body>");
                htmlContent.AppendLine(@"自定义测试Server模拟接受请求");
                htmlContent.AppendLine(@"</body>");
                htmlContent.AppendLine(@"</html>");
                //写消息
                clientSocket.Send(Encoding.UTF8.GetBytes(htmlContent.ToString()));
                //告知断开连接
                clientSocket.Shutdown(SocketShutdown.Both);
                //关闭socket
                clientSocket.Close();
            }
        }
    }
}
```

升级后的`BIO` 实现了客户端与线程数是1:1，当请求多的时候，线程也就开启了无数个，根本无法控制线程的数量，可能会导致资源耗尽而服务器宕机
```
public class ProBIOServer : IWebServer
{
    public void Run()
    {
        //监听
        var ip = IPAddress.Parse("127.0.0.1");
        int port = 9000;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(ip, port));
        serverSocket.Listen(1000);

        //获取请求链接,Accept会阻塞进程,所以会是一个接着一个去处理来自客户端的请求
        while (true)
        {
            Socket clientSocket = serverSocket.Accept();               
            ParameterizedThreadStart job = new ParameterizedThreadStart((cliSocket) =>
            {
                var ss = cliSocket as Socket;
                var bufferBytes = new byte[1024];
                try
                {
                    int receiveNumber = 0;
                    bool firstLoad = true;
                    while (firstLoad || receiveNumber == bufferBytes.Length)
                    {
                        receiveNumber = ss.Receive(bufferBytes);
                        string message = Encoding.UTF8.GetString(bufferBytes, 0, receiveNumber);
                        Console.WriteLine("浏览器请求消息(长度{0}):\r\n{1}", receiveNumber, message);
                        firstLoad = false;
                    }
                }
                finally
                {
                    StringBuilder htmlContent = new StringBuilder();
                    htmlContent.AppendLine(@"http/1.0 200 OK");
                    htmlContent.AppendLine(@"Content-Type:text/html;charset:UTF-8");
                    htmlContent.AppendLine();

                    htmlContent.AppendLine(@"<html lang=""zh-cn"">");
                    htmlContent.AppendLine(@"<head>");
                    htmlContent.AppendLine(@"<meta charset=""utf-8""/>");
                    htmlContent.AppendLine(@"</head>");
                    htmlContent.AppendLine(@"<body>");
                    htmlContent.AppendLine(@"自定义测试Server模拟接受请求");
                    htmlContent.AppendLine(@"</body>");
                    htmlContent.AppendLine(@"</html>");
                    //写消息
                    ss.Send(Encoding.UTF8.GetBytes(htmlContent.ToString()));
                    //告知断开连接
                    ss.Shutdown(SocketShutdown.Both);
                    //关闭socket
                    ss.Close();
                }
            });
            Thread thread = new Thread(job);
            thread.Start(clientSocket);                
        }
    }
}
```

2. `PIO`: 在`BIO`基础之上，设想一下如果这个连接不做任何事情的话就会造成不必要的线程开销，不过可以通过 线程池机制 改善，线程池还可以让线程的创建和回收成本相对较低。
为了解决同步阻塞I/O面临的一个链路需要一个线程处理的问题，后来有人对它的线程模型进行了优化一一一后端通过一个线程池来处理多个客户端的请求接入，形成客户端个数M：线程池最大线程数N的比例关系，其中M可以远远大于N.通过线程池可以灵活地调配线程资源，设置线程的最大值，防止由于海量并发接入导致线程耗尽。

![伪异步通信模型图](images/pio.pong)

通常代码如下：
```
public class PIOServer : IWebServer
{
    public void Run()
    {
        //监听
        var ip = IPAddress.Parse("127.0.0.1");
        int port = 9000;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(ip, port));
        serverSocket.Listen(1000);

        //获取请求链接,Accept会阻塞进程,所以会是一个接着一个去处理来自客户端的请求
        ThreadPool.SetMaxThreads(50, 50);
        while (true)
        {
            Socket clientSocket = serverSocket.Accept();
            ThreadPool.QueueUserWorkItem(HandlerRequest, clientSocket);
        }
    }
    /// <summary>
    /// 处理请求
    /// </summary>
    public static void HandlerRequest(object cliSocket)
    {
        var ss = cliSocket as Socket;
        var bufferBytes = new byte[1024];
        try
        {
            int receiveNumber = 0;
            bool firstLoad = true;
            while (firstLoad || receiveNumber == bufferBytes.Length)
            {
                receiveNumber = ss.Receive(bufferBytes);
                string message = Encoding.UTF8.GetString(bufferBytes, 0, receiveNumber);
                Console.WriteLine("\r\n==========================================\r\n浏览器请求消息(长度{0}):\r\n{1}", receiveNumber, message);
                firstLoad = false;
            }
        }
        finally
        {
            StringBuilder htmlContent = new StringBuilder();
            htmlContent.AppendLine(@"http/1.0 200 OK");
            htmlContent.AppendLine(@"Content-Type:text/html;charset:UTF-8");
            htmlContent.AppendLine();

            htmlContent.AppendLine(@"<html lang=""zh-cn"">");
            htmlContent.AppendLine(@"<head>");
            htmlContent.AppendLine(@"<meta charset=""utf-8""/>");
            htmlContent.AppendLine(@"</head>");
            htmlContent.AppendLine(@"<body>");
            htmlContent.AppendLine(@"自定义测试Server模拟接受请求");
            htmlContent.AppendLine(@"</body>");
            htmlContent.AppendLine(@"</html>");
            //写消息
            ss.Send(Encoding.UTF8.GetBytes(htmlContent.ToString()));
            //告知断开连接
            ss.Shutdown(SocketShutdown.Both);
            //关闭socket
            ss.Close();
        }
    }
}
```

3. `NIO` : .net中是使用`IOCP`方式进行实现,特点如下：

通常代码如下:
```
```
