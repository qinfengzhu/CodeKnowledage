### Redis 安装

1. `Redis` Windows 安装 [windows安装版下载](https://github.com/microsoftarchive/redis/releases)

### Redis 通信

1. `redis` ping pong
```
public static void main(String[] args) throws Exception
{
        // socket
        Socket socket = new Socket("140.143.135.210", 6379);

        // oi流
        OutputStream os = socket.getOutputStream();
        InputStream is = socket.getInputStream();

        // 向redis服务器写
        os.write("PING\r\n".getBytes());

        //从redis服务器读,到bytes中
        byte[] bytes = new byte[1024];
        int len = is.read(bytes);

        // to string 输出一下，输出 +PONG
        System.out.println(new String(bytes,0,len));
}
```
为什么是 `+PONG` , $ * + 这几个符号挨个判断来确定传输内容的含义的

2. `set` ,如果成功 返回 `+OK`

```
public static void main(String[] args) throws Exception
{
        // socket
        Socket socket = new Socket("140.143.135.210", 6379);

        // oi流
        OutputStream os = socket.getOutputStream();
        InputStream is = socket.getInputStream();

        // 向redis服务器写
        os.write("set hello helloWorld\r\n".getBytes());

        //从redis服务器读,到bytes中
        byte[] bytes = new byte[1024];
        int len = is.read(bytes);

        // to string 输出一下,返回 +OK
        System.out.println(new String(bytes,0,len));
}
```

3. `get`,如果成功返回 `$10 helloWorld`

```
public static void main(String[] args) throws Exception
{
        // socket
        Socket socket = new Socket("140.143.135.310", 6379);

        // oi流
        OutputStream os = socket.getOutputStream();
        InputStream is = socket.getInputStream();

        // 向redis服务器写
        os.write("get hello\r\n".getBytes());

        //从redis服务器读,到bytes中
        byte[] bytes = new byte[1024];
        int len = is.read(bytes);

        // to string 输出一下
        System.out.println(new String(bytes,0,len));
}
```

有关协议的[官方文档](https://redis.io/topics/protocol),还有一个[中文版本文档](http://doc.redisfans.com/topic/protocol.html)

 $ 表示批量读取, 一般格式是: $<数字>, 数字来表示正文的内容的字节数

4. 客户端向服务端发送 `get hello`,这种只是内联命令,而不是 `Redis` 真正的通信协议

```
Q: 什么意思？
A: 就是说你可以像之前那样给服务端发, 服务器端接受到后, 会遍历一遍你发送的内容, 最后根据空格来分析你所发的内容的含义.

Q: 这样有什么坏处
A: 如果这样的话, 你就把解析的工作交给了服务器来做, 会加大服务器的工作量.

Q: 什么样才符合通信协议规范?如果符合规范真的会提高服务器的效率?
A: 看一下符合协议的客户端和服务器端之间的交互.

```

Demo如下
```
// set userinfo bestkf，抓包后是
*3  //表示客户端将发送3段内容  第一段 $3 SET ,第二段 $8 userinfo ,第三段 $6 bestkf
$3  
SET
$8
userinfo
$6
bestkf
+OK   //这是服务端返回的信息
```
5. 类库`jedis` 做的工作，就是把 `set key value` 这样转化为一下格式

```
*3\r\n
$3\r\n
SET\r\n
$3\r\n
key\r\n
$5\r\n
value\r\n
```
最后发送给`Redis`服务端
