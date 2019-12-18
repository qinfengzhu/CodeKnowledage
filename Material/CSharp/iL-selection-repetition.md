### IL的选择与重复

1. `br` 简单跳转功能
```
//反编译出来的IL 带有标签，方便使用 br 进行跳转,格式为 "标签:"
IL_0000:  ldstr      "hi"
IL_0005:  call       void [mscorlib]System.Console::WriteLine(class System.String)
IL_000a:  call       void zzz::abc()
IL_000f:  ret
```
跳转的IL 例子

```
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (int32 V_0,class zzz V_1)
        newobj instance void zzz::.ctor()
        stloc.1
        call int32 zzz::abc()
        stloc.0
        ldloc.0
        call void [mscorlib]System.Console::WriteLine(int32)
        ret
    }

    .method private hidebysig static int32 abc() il managed
    {
        .locals (int32 V_0)
        ldc.i4.s   20
        br.s  a2
        ldc.i4.s   30
        a2:  ret
    }
}
```
使用 `ilasm /nologo /quiet` 编译上面的程序得出 `exe`文件，执行打印`20`

2. `if` 与 `bool 默认值` 怎么实现的

```
class zzz
{
    static bool i = true;
    public static void Main()
    {
        if (i)
        System.Console.WriteLine("hi");
    }
}

//对应的IL 代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static bool i
    .method public hidebysig static void vijay() il managed
    {
        .entrypoint
        ldsfld     bool zzz::i //ldsfld将静态变量的值放在堆栈上
        brfalse.s  IL_0011 //brfalse指令扫描堆栈。如果它将数字设置为1，则将其解释为TRUE，如果它找到数字0，则将其解释为FALSE
        ldstr      "hi"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        IL_0011: ret
    }

    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.1  //ldc指令将值1置于静态构造函数的堆栈中
        stsfld     bool zzz::i //stsfld用于将静态变量i初始化为值1,这证明IL支持名为bool的数据类型的概念，但它不识别单词true和false
        ret
    }
}
```
3. 有条件跳转 `brfalse.s` 与 无条件跳转 `br.s` 组合实现 `if......else......`

```
class zzz
{
    static bool i = true;
    public static void Main()
    {
        if (i)
            System.Console.WriteLine("hi");
        else
            System.Console.WriteLine("false");
    }
}
//对应的IL
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static bool i
    .method public hidebysig static void vijay() il managed
    {
        .entrypoint
        ldsfld bool zzz::i
        brfalse.s  IL_0013  //有条件跳转
        ldstr  "hi"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        br.s IL_001d        //无条件跳转
        IL_0013:  ldstr      "false"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        IL_001d: ret
    }

    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.1
        stsfld     bool zzz::i
        ret
    }
}
```

4. IL 对块中的变量限制,下面说明 IL 对块变量的宽松说明:IL，生活相对轻松。这两个变成了两个独立的变量V_0和V_1。因此，IL不对变量施加任何限制

```
class zzz
{
    public static void Main()
    {
    }

    void abc( bool a)
    {
        if (a)
        {
            int i = 0;
        }
        if (a)
        {
            int i = 3;
        }
    }
}
//对应的IL代码
.assembly mukhi {}
.class public auto ansi zzz extends [mscorlib]System.Object
{

    .field private int32 x
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ret
    }

    .method private hidebysig instance void abc(bool a) il managed
    {
        .locals (int32 V_0,int32 V_1)  //方法体里面有 2个i ,所以在方法体中有2个变量
        ldarg.1         //把索引为1出的参数加入到计算栈中,类的ldarg.0 一般都是this，所以bool a 是索引为1的参数
        brfalse.s  IL_0005
        ldc.i4.0        //将整数值 0 作为 int32 推送到计算堆栈上
        stloc.0         //从计算堆栈的顶部弹出当前值并将其存储到【索引 0 处的局部变量列表中】
        IL_0005:  ldarg.1
        brfalse.s  IL_000a
        ldc.i4.3
        stloc.1
        IL_000a:  ret
    }
}
```

5. `add` `mul` `sub` `div` 等操作命令

```
class zzz
{
    static int i = 2;
    public static void Main()
    {
        i = i + 3;
        System.Console.WriteLine(i);
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static int32 i
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ldsfld int32 zzz::i
        ldc.i4.3
        add
        stsfld int32 zzz::i
        ldsfld int32 zzz::i
        call void [mscorlib]System.Console::WriteLine(int32)
        ret
    }
    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.2
        stsfld     bool zzz::i
        ret
    }
}
```

6. `cgt` 命令处理`>`条件运算,`clt` 处理`<` 运算符

```
class zzz
{
    static bool i;
    static int j = 19;
    public static void Main()
    {
        i = j > 16;
        System.Console.WriteLine(i);
    }
}
//相应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static bool i
    .field private static int32 j
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ldsfld     int32 zzz::j
        ldc.i4.s   16
        cgt        //比较两个值。 如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上
        stsfld     bool zzz::i
        ldsfld     bool zzz::i
        call void [mscorlib]System.Console::WriteLine(bool)
        ret
    }
    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.s   19
        stsfld int32 zzz::j
        ret
    }
}
```

7. `ceq` 指令检查是否相等,如果它们相等，则将值1（TRUE）置于堆栈上，如果它们不相等，则将值0（FALSE）置于堆栈上。

```
class zzz
{
    static bool i;
    static int j = 19;
    public static void Main()
    {
        i = j >= 16;
        System.Console.WriteLine(i);
    }
}
//对应的IL 代码
.assembly mukhi {}

.class private auto ansi zzz extends System.Object
{
    .field private static bool i
    .field private static int32 j
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ldsfld     int32 zzz::j
        ldc.i4.s   16
        cgt        //比较两个值。 如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上
        ldc.i4.0
        ceq        //比较两个值。 如果这两个值相等，则将整数值 1 (int32) 推送到计算堆栈上；否则，将 0 (int32) 推送到计算堆栈上
        stsfld     bool zzz::i
        ldsfld     bool zzz::i
        call void [mscorlib]System.Console::WriteLine(bool)
        ret
    }
    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.s   19
        stsfld int32 zzz::j
        ret
    }
}
```

8. `!=` 也是使用`ceq`进行

```
class zzz
{
    static bool i;
    static int j = 19;
    public static void Main()
    {
        i = j != 16;
        System.Console.WriteLine(i);
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static bool i
    .field private static int32 j
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ldsfld     int32 zzz::j
        ldc.i4.s   16
        ceq        //ceq 指令用于检查堆栈上的值是否相等,如果他们相等则返回true,不相等则返回false
        ldc.i4.0   //加载 false
        ceq        //ceq 将早期ceq的结果与false进行比较,如果第一个ceq的结果为true，则最终答案为false反之亦然
        stsfld     bool zzz::i
        ldsfld     bool zzz::i
        call void [mscorlib]System.Console::WriteLine(bool)
        ret
    }
    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.s   19
        stsfld int32 zzz::j
        ret
    }
}
```

9. `while` 的IL转换, `ble.s` 指令是基于 `cgt`与`brfalse`,while结构的条件存在于顶部，但条件的代码存在于底部。在转换为IL时，将在while结构的持续时间内执行的代码放在条件的代码上方

```
class zzz
{
    static int i = 1;
    public static void Main()
    {
        while ( i <= 2)
        {
            System.Console.WriteLine(i);
            i++;
        }
    }
}
//对应的IL 代码为
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static int32 i
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        br.s IL_0018  //无条件地将控制转移到目标指令（短格式）
        IL_0002:  ldsfld     int32 zzz::i
        call void [mscorlib]System.Console::WriteLine(int32)
        ldsfld int32 zzz::i
        ldc.i4.1
        add
        stsfld int32 zzz::i
        IL_0018:  ldsfld int32 zzz::i
        ldc.i4.2
        ble.s IL_0002  //如果第一个值小于或等于第二个值，则将控制转移到目标指令（短格式）
        ret
    }
    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.s   1
        stsfld int32 zzz::i
        ret
    }
}
```

10. `for`  第一个分号的代码只执行一次。因此，要初始化的变量i被放置在循环之外。然后，我们无条件地跳转到标签IL_001e以检查i的值是否小于2。如果为TRUE，则代码跳转到标签IL_0008，这是for语句代码的起始点。

```
class zzz
{
    static int i = 1;
    public static void Main()
    {
        for ( i = 1; i <= 2 ; i++)
        {
            System.Console.WriteLine(i);
        }
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .field private static int32 i
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ldc.i4.1
        stsfld     int32 zzz::i  //for中的第一个分号代码只执行一次
        br.s       IL_001e       //无条件跳转到判断地方
        IL_0008:  ldsfld     int32 zzz::i
        call       void [mscorlib]System.Console::WriteLine(int32)
        ldsfld     int32 zzz::i
        ldc.i4.1
        add
        stsfld int32 zzz::i
        IL_001e:  ldsfld     int32 zzz::i   //判断 i<=2地方
        ldc.i4.2
        ble.s IL_0008          //跳转到执行业务代码地方
        ret
    }
    .method public hidebysig specialname rtspecialname static void .cctor() il managed
    {
        ldc.i4.s   1
        stsfld int32 zzz::i
        ret
    }
}
```

11. `break` 有助于退出 for 循环, while 循环,do-while 循环等

```
public class zzz
{
    public static void Main()
    {
        int i ;
        for ( i = 1; i<= 10 ; i++)
        {
            if ( i == 2)
            break;
            System.Console.WriteLine(i);
        }
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void  Main() il managed
    {
        .entrypoint
        .locals (int32 V_0)
        ldc.i4.1
        stloc.0
        br.s       IL_0014  //for 无条件跳转
        IL_0004:  ldloc.0
        ldc.i4.2
        bne.un.s   IL_000a  //当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令（短格式）
        br.s       IL_0019  //无条件跳出循环
        IL_000a:  ldloc.0
        call       void [mscorlib]System.Console::WriteLine(int32)
        ldloc.0
        ldc.i4.1
        add
        stloc.0
        IL_0014:  ldloc.0  //判断点
        ldc.i4.s   10
        ble.s      IL_0004
        IL_0019:  ret
    }
}
```

下一章[关键字与操作符](il-keyword-operation.md)
