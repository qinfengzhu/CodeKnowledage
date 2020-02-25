### RPC简介

分布式系统的基本功能

1. 网络通信

2. 序列化/反序列化

3. 传输协议

4. 服务调用

### 需要的基本组件

1. RpcClient :负责导入远程接口的代理实现

2. RpcProxy : 远程接口的代理实现

3. RpcInvoker : 负责编码和发送调用请求到服务并等待结果

4. RpcProtocol : 负责协议编/解码

5. RpcConnector : 负责维护客户/服务方连接通道和发送数据到服务方

6. RpcChannel : 数据传输通道

### 基本知识内容

1. [通信协议](protocol.md)

2. [C# 的高性能Socket编程](csharpSocket.md)

3. [C# 高性能Socket编程如何与Protocol协议进行配合使用](socketWithProtocol.md)

### RPC技术选型

1. Protobuf(数据转换) + Netty(网络传输)

2. gRPC 或 Thrift(数据转换+网络传输)
