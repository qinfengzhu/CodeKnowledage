### 值得收藏的几本书

1. Microsoft .NET IL汇编语言程序设计


### 大概的一些内容讲解

[ManagedHead_内部详情](Material/Bookes/images/managedHead.png)
Managed Heap(托管堆):用于存放引用类型的值
Evaluation Statck(计算栈)：临时存放值类型数据，引用类型地址的堆栈(这个是栈，所以遵循栈的操作特点，先进后出)
Call Stack(调用栈)：其中的Record Frame 用于存放.locals init(int32 V_0)指令的参数值如：V_0 (Record Frame是一个局部变量表，所以不遵守FILO原则 )
