### IL 基础学习过程

##### [在线C#代码对应IL代码翻译](https://sharplab.io)
##### [Exprert .NET 1.1 Programming](https://books.google.com/books?id=IiB0FpGdCJ0C&pg=PA22&lpg=PA22&dq=How+to+remember+IL+Opcode&source=bl&ots=qBM3xlip_R&sig=ACfU3U2okmNR-5NGVAcp2nVy14UMVsW_5g&hl=en&sa=X&ved=2ahUKEwjJoefjh9zjAhWNOnAKHXgABzg4ChDoATACegQICRAB#v=onepage&q=How%20to%20remember%20IL%20Opcode&f=false) 对IL 操作符讲解仔细的书籍

1. 配置 `path` 环境变量 方便使用 `ilasm`，`ilasm.exe` 一般在的位置为 `C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe`,添加到系统环境变量 `Path`中

### 介绍Microsoft IL

```
//第一个IL文件 a.il
.method void HelloWord()
{    
}
```
`powershell` 执行 `ilasm /nologo /quiet a.il`,下面说下几个参数含义

`/nologo` : 不标记logo
`/quiet` : 不展示程序集过程
`/dll` : 编译为dll
`/exe` : 编译为.exe程序(默认)

会报警告与错误
```
warning: Non-static global method 'HelloWord',made static
warning: Method has no body,'ret' emitted
Error: No entry point declared for executable
```

2. 继续调整上面 `a.il`

```
.method void HelloWord()
{    
    .entrypoint
}
```

执行 `ilasm /nologo /quiet a.il` ，有两个警告

2.1 Non-static global method 'HelloWord',made static

2.2 Method has no body,'ret' emitted

会编译得到 a.exe ，然后 `.\a.exe` 会出现 `未能加载文件或程序集 a.exe`

3. 函数结束指令 `ret`

```
.method void HelloWord()
{    
    .entrypoint
    ret
}
```
继续编译会得到一个警告： Non-static global method 'HelloWord',made static
分析 `.entrypoint` 相当于 `Main` 函数，首先执行的函数在IL中就是 `.entrypoint`,相当于C#中的 `Main`

4. `ldarg.0` 表示 `this指针`,每一个非静态或实例函数都传递一个句柄,该句柄指示调用此函数的对象的变量位置。
此句柄始终作为第一个参数传递给每一个实例函数,由于它始终默认传递,因此在函数的参数列表中未提及。

```
//可以 csc 编译该文件，然后再 ildasm a.exe /text
//ildasm 工具一般在 C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools
class zzz
{
    public static void Main()
    {
        System.Console.WriteLine("hi");
        new zzz();
    }

    zzz()
    {
        System.Console.WriteLine("bye");
    }

    static zzz()
    {
        System.Console.WriteLine("byes");
    }
}
```

```
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void vijay() il managed
    {
        .entrypoint
        ldstr "hi"
        call void System.Console::WriteLine(class System.String)
        newobj instance void zzz::.ctor()
        pop
        ret
    }

    .method private hidebysig specialname rtspecialname instance void .ctor() il managed
    {
        ldarg.0
        call instance void [mscorlib]System.Object::.ctor()
        ldstr "bye"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }

    .method private hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldstr "byes"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

5. 通过IL确定变量先被初始化还是构造函数代码先执行.IL输出非常清楚的表妹,首先所有变量都被初始化,然后,构造函数中的代码被执行。
而且，基类的构造函数首先被执行。然后只有这样，才能调用构造函数中编写的代码。

```
class zzz
{
    static int i= 6 ;
    public long j = 7;
    public static void Main()
    {
        new zzz();
    }
    static zzz()
    {
        System.Console.WriteLine("zzzs");
    }
    zzz()
    {
        System.Console.WriteLine("zzzi");
    }
}
```

```
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{

    .field private static int32 i  //私有静态变量字段 i
    .field public int64 j          //公有变量字段 j

    .method public hidebysig static void vijay() il managed
    {
        .entrypoint      
        newobj instance void zzz::.ctor()
        pop //移除当前位于计算堆栈顶部的值
        ret
    }

    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.6                   //数字6 入计算栈操作
        stsfld int32 zzz::i        //静态字段变量栈顶赋值 stsfld
        ldstr "zzzs"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }

    .method public hidebysig specialname rtspecialname instance void .ctor() il managed
    {
        ldarg.0                   //索引 0 处的参数值推送到堆栈上代表this,当操作类中任何 变量、属性、方法的时候都得this先入栈
        ldc.i4.7                  //装入计算栈操作 int->int32->4字节
        conv.i8                   //字节变大 8字节
        stfld int64 zzz::j        //实例字段变量栈顶赋值 stfld
        ldarg.0
        call  instance void [mscorlib]System.Object::.ctor()
        ldstr "zzzi"              //字符串 入栈操作   
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

6. `ldc.i4.s` 将提供的 int8 值作为 int32 推送到计算堆栈上

```
class zzz
{
    public static void Main()
    {
        System.Console.WriteLine(10);
    }
}

//相关IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void vijay() il managed
    {
        .entrypoint
        ldc.i4.s 10  //将提供的 int8 值作为 int32 推送到计算堆栈上
        call void [mscorlib]System.Console::WriteLine(int32)
        ret
    }
}
```

7. `.locals` 与 `stloc`  与 `ldloca.s` 与 `box`

```
class zzz
{
    public static void Main()
    {
        System.Console.WriteLine("{0}",20);
    }
}

//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void vijay() il managed
    {
        .entrypoint
        .locals (int32 V_0)  //不必初始化的int32 变量V_0
        ldstr "{0}"          //推送对元数据中存储的字符串的新对象引用
        ldc.i4.s 20          //将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）
        stloc.0              //从计算堆栈的顶部弹出当前值并将其存储到索引 0 处的局部变量列表中
        ldloca.s V_0         //将位于特定索引处的局部变量的地址加载到计算堆栈上（短格式）
        box [mscorlib]System.Int32  //将值类转换为对象引用
        call void [mscorlib]System.Console::WriteLine(class System.String,class System.Object)
        ret
    }
}
```

8. `ldarga.s` 将参数地址加载到计算堆栈上,一般就是方法里面使用方法的参数时候使用

```
class zzz
{
    public static void Main()
    {
        zzz a = new zzz();
        a.abc(10);
    }
    void abc(int i)
    {
        System.Console.WriteLine("{0}",i);
    }
}

//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class zzz V_0)  //定义zzz类型变量 V_0
        newobj instance void zzz::.ctor()  //创建一个值类型的新对象或新实例，并将对象引用（zzz 类型）推送到计算堆栈上
        stloc.0   //从计算堆栈的顶部弹出当前值并将其存储到指定索引处的局部变量列表中
        ldloc.0   //将指定索引处的局部变量加载到计算堆栈上
        ldc.i4.s 10  //将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）
        call instance void zzz::abc(int32)  //call 参数中从左到右的顺序放置在堆栈上。
                                            //调用方法实例 （或虚拟） 必须推送之前的所有用户可见的参数的该实例引用
        ret
    }

    .method private hidebysig instance void abc(int32 i) il managed
    {
        ldstr "{0}"
        ldarga.s   i   //将参数地址加载到计算堆栈上 而ldarg（将参数值加载到堆栈上）
        box [mscorlib]System.Int32 //装箱
        call void [mscorlib]System.Console::WriteLine(class System.String,class System.Object)
        ret
    }
}
```

[IL 选择与重复](iL-selection-repetition.md)
