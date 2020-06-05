#### RoutingKey 与 BindingKey

1. 使用RoutingKey 与 BindingKey 创建交换机与队列，并且进行消息发布
```
RoutingKey = BindingKey;
channel.exchangeDeclare(EXCHANGE_NAME , "direct " , true , false , null) ;
channel.queueDeclare(QUEUE_NAME , true , false , false , null );
channel.queueBind(QUEUE_NAME , EXCHANGE_NAME , BindingKey) ; //BindingKey
String message = "Hello World! " ;
channel.basicPublish(EXCHANGE_NAME , RoutingKey,MessageProperties . PERSISTENT_TEXT_PLAIN,message.getBytes{)) ;
```

2. Key 关键字 `# *` ,`.`分割的算一个但是 `*`表示模糊一个单词 `#`表示多个单词
