#### 运算符重载

1. ">" ,"+" 等运算符,而 ">" 与"<"就必须成对出现，这是c#编译器强加的,同理"+"与"-"等

```
public class zzz
{
    public static void Main()
    {
        yyy a = new yyy(10);
        yyy b = new yyy(5);
        yyy c;
        c = a + b ;
        System.Console.WriteLine(c.i);
    }
}
public class yyy
{
    public int i;
    public yyy( int j)
    {
        i = j;
    }
    public static yyy operator + ( yyy x , yyy y)
    {
        System.Console.WriteLine(x.i);
        yyy z = new yyy(12);
        return z;
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class yyy V_0,class yyy V_1,class yyy V_2)
        ldc.i4.s   10
        newobj     instance void yyy::.ctor(int32)
        stloc.0
        ldc.i4.5
        newobj     instance void yyy::.ctor(int32)
        stloc.1
        ldloc.0
        ldloc.1
        call class yyy yyy::op_Addition(class yyy,class yyy) //调用运算重载符号
        stloc.2
        ldloc.2
        ldfld      int32 yyy::i
        call void [mscorlib]System.Console::WriteLine(int32)
        ret
        ret
    }
}
.class public auto ansi yyy extends [mscorlib]System.Object
{
    .field public int32 i
    .method public hidebysig specialname static class yyy  op_Addition(class yyy x,class yyy y) il managed //为一个静态的函数
    {
        .locals (class yyy V_0,class yyy V_1)
        ldarg.0
        ldfld      int32 yyy::i
        call       void [mscorlib]System.Console::WriteLine(int32)
        ldc.i4.s   12
        newobj     instance void yyy::.ctor(int32)
        stloc.0
        ldloc.0
        stloc.1
        ldloc.1
        ret
    }
    .method public hidebysig specialname rtspecialname instance void  .ctor(int32 j) il managed
    {
        ldarg.0
        call       instance void [mscorlib]System.Object::.ctor()
        ldarg.0
        ldarg.1
        stfld      int32 yyy::i
        ret
    }
}
```

2.  `string` 操作符，C＃编译器非常智能。每当yyy对象必须转换为字符串时，它首先检查类yyy中是否存在名为string的运算符。如果存在，则调用该运算符。名为string的运算符是C＃中的预定义数据类型。因此，它被转换为运算符op_Implicit。此运算符将yyy对象作为参数。它在堆栈上为WriteLine函数返回一个字符串。不调用ToString函数。

```
public class zzz
{
    public static void Main()
    {
        yyy a = new yyy();
        System.Console.WriteLine(a);
    }
}
public class yyy
{
    public static implicit operator string(yyy y)
    {
        System.Console.WriteLine("operator string");
        return "yyy class " ;
    }
    public override string ToString()
    {
        System.Console.WriteLine("ToString");
        return "mukhi";
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class yyy V_0)
        newobj     instance void yyy::.ctor()
        stloc.0
        ldloc.0
        call class System.String yyy::op_Implicit(class yyy)
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}

.class public auto ansi yyy extends [mscorlib]System.Object
{
    .method public hidebysig specialname static class System.String  op_Implicit(class yyy y) il managed
    {
        .locals (class System.String V_0)
        ldstr      "operator string"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        ldstr      "yyy class "
        stloc.0
        ldloc.0
        ret
    }
    .method public hidebysig virtual instance class System.String ToString() il managed
    {
        .locals (class System.String V_0)
        ldstr      "ToString"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        ldstr      "mukhi"
        stloc.0
        ldloc.0
        ret
    }
}
```

3. `typeof` 运算符转化

```
class zzz
{
    public static void Main()
    {
        System.Type m;
        m = typeof(int);
        System.Console.WriteLine(m.FullName);
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class [mscorlib]System.Type V_0)
        ldtoken [mscorlib]System.Int32  //ldtoken指令将类型放在堆栈上
        //调用函数GetTypeFromHandle来获取标记，即堆栈中的结构或值类,此后函数返回表示类型的Type对象
        call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(value class [mscorlib]System.RuntimeTypeHandle)
        stloc.0
        ldloc.0
        callvirt   instance class System.String [mscorlib]System.Type::get_FullName() //调用函数get_FullName
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

4. `is` 类型判断符号转换

```
class zzz
{
    public static void Main()
    {
        zzz z = new zzz();
        z.abc(z);
        object o = new object();
        z.abc(o);
    }
    void abc(object a)
    {
        if ( a is zzz)
        System.Console.WriteLine("zzz");
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class zzz V_0,class System.Object V_1)
        newobj     instance void zzz::.ctor()
        stloc.0
        ldloc.0
        ldloc.0
        call       instance void zzz::abc(class System.Object)
        newobj     instance void [mscorlib]System.Object::.ctor()
        stloc.1
        ldloc.0
        ldloc.1
        call       instance void zzz::abc(class System.Object)
        ret
    }
    .method private hidebysig instance void abc(class System.Object a) il managed
    {
        ldarg.1      //参数压入栈
        isinst zzz   //isinst指令在堆栈顶部获取值并将其转换为指定的数据类型,如果无法执行此操作，则会在堆栈上放置NULL
        brfalse.s  IL_0012  //如果为false,则跳转到 IL_0012
        ldstr  "zzz"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        IL_0012: ret
    }
    .method public hidebysig specialname rtspecialname instance void .ctor() il managed
    {
        ldarg.0
        call instance void [mscorlib]System.Object::.ctor()
        ret
    }
}
```

5. `as` 关键字，类似于`is`

```
class zzz
{
    public static void Main()
    {
        abc(100);
        abc("hi");
    }
    static void abc( object a)
    {
        string s;
        s =  a as string;
        System.Console.WriteLine(s);
    }
}
//对应的IL代码为
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (int32 V_0)
        ldc.i4.s   100
        stloc.0
        ldloca.s   V_0
        box        [mscorlib]System.Int32  //必须转换为object 进行装箱
        call       void zzz::abc(class System.Object)
        ldstr      "hi"
        call       void zzz::abc(class System.Object)
        ret
    }
    .method private hidebysig static void  abc(class System.Object a) il managed
    {
        .locals (class System.String V_0)
        ldarg.0
        isinst     [mscorlib]System.String  //isinst指令在堆栈顶部获取值并将其转换为指定的数据类型,如果无法执行此操作，则会在堆栈上放置NULL
        stloc.0
        ldloc.0
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

6. `sizeof` 操作符与 `unsafe` 及指针

sizeof关键字是IL中的一条指令，它返回作为参数传递给它的变量的大小。它只能用于值类型变量，而不能用于引用类型
在引入指针时使用修饰符unsafe 。IL中不存在此修饰符，因为IL将所有内容视为不安全。注意，C＃中的一个字节被转换为IL中的int8。
```
class zzz
{
    unsafe public static void Main()
    {
        System.Console.WriteLine(sizeof(byte *));
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        sizeof     unsigned int8*
        call       void [mscorlib]System.Console::WriteLine(int32)
        ret
    }
}
```

7. `stackalloc` 内存分配,下面展示的是:最终分配总共400字节的存储器

```
class zzz
{
    public static unsafe void Main()
    {
        int* i = stackalloc int[100];
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (int32* V_0)
        ldc.i4.4    //将整数值 4 作为 int32 推送到计算堆栈上
        ldc.i4.s   100  //将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）
        mul         //将两个值相乘并将结果推送到计算堆栈上
        localloc    //从本地动态内存池分配特定数目的字节并将第一个分配的字节的地址（瞬态指针， 类型）推送到计算堆栈上
        stloc.0
        ret
    }
}
```

8. `Enum` 枚举,创建了三个枚举。在转换为IL时，将创建三个具有相同名称的相应文字字段。枚举变量的值在编译时计算。引入了一个名为value__的特殊变量,IL丢弃所有枚举名称并且仅处理这些值。

```
class zzz
{
    public static void Main()
    {
        System.Console.WriteLine(yyy.black);
    }
}
enum yyy
{
    a1,
    black,
    hell
}
//对应的IL 代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void  Main() il managed
    {
        .entrypoint
        .locals (value class yyy V_0)
        ldc.i4.1
        stloc.0
        ldloca.s   V_0
        box  yyy
        call void [mscorlib]System.Console::WriteLine(class System.Object)
        ret
    }
}
.class value private auto ansi serializable sealed yyy extends [mscorlib]System.Enum
{
    .field public specialname rtspecialname int32 value__
    .field public static literal value class yyy a1 = int32(0x00000000)
    .field public static literal value class yyy black = int32(0x00000001)
    .field public static literal value class yyy hell = int32(0x00000002)
}
```

9. `switch` 转换

```
public class zzz
{
    public static void Main()
    {
        zzz a = new zzz();
        a.abc(1);
        a.abc(10);
    }
    void abc(int i)
    {
        switch (i)
        {
            case 0:
            System.Console.WriteLine("zero");
            break;
            case 1:
            System.Console.WriteLine("one");
            break;
            default:
            System.Console.WriteLine("end");
        }
    }
}
//对应的IL 代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void  Main() il managed
    {
        .entrypoint
        .locals (class zzz V_0)
        newobj instance void zzz::.ctor()
        stloc.0
        ldloc.0
        ldc.i4.1
        call instance void zzz::abc(int32)
        ldloc.0
        ldc.i4.s   10
        call instance void zzz::abc(int32)
        ret
    }
    .method private hidebysig instance void abc(int32 i) il managed
    {
        .locals (int32 V_0)
        ldarg.1
        stloc.0
        ldloc.0
        switch     (IL_0012,IL_001e) //一个值推送到堆栈上,堆栈中弹出值并将执行转移到指令编制索引的值的偏移量，其中的值是小于N 实现跳转表
        br.s       IL_002a
        IL_0012:  ldstr      "zero"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        br.s       IL_0034
        IL_001e:  ldstr      "one"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        br.s       IL_0034
        IL_002a:  ldstr      "end"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        IL_0034:  ret
    }
}
```

10. `checked` 与 `unchecked`

```
class zzz
{
    int b = 1000000;
    int c = 1000000;
    public static void Main()
    {
        zzz a = new zzz();
        a.pqr(a.b,a.c);
        a.xyz(a.b,a.c);
    }
    int pqr( int x, int y)
    {
        return unchecked(x*y);
    }
    int xyz( int x, int y)
    {
        return checked(x*y);
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .field private int32 b
    .field private int32 c
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class zzz V_0)
        newobj     instance void zzz::.ctor()
        stloc.0
        ldloc.0
        ldloc.0
        ldfld      int32 zzz::b
        ldloc.0
        ldfld      int32 zzz::c
        call       instance int32 zzz::pqr(int32,int32)
        pop
        ldloc.0
        ldloc.0
        ldfld      int32 zzz::b
        ldloc.0
        ldfld      int32 zzz::c
        call       instance int32 zzz::xyz(int32,int32)
        pop
        ret
    }
    .method private hidebysig instance int32 pqr(int32 x,int32 y) il managed
    {
        .locals (int32 V_0)
        ldarg.1
        ldarg.2
        mul    //不检查溢出
        stloc.0
        br.s       IL_0006
        IL_0006:  ldloc.0
        ret
    }
    .method private hidebysig instance int32 xyz(int32 x,int32 y) il managed
    {
        .locals (int32 V_0)
        ldarg.1
        ldarg.2
        mul.ovf  //ovf 检查溢出
        stloc.0
        br.s       IL_0006
        IL_0006:  ldloc.0
        ret
    }
    .method public hidebysig specialname rtspecialname instance void .ctor() il managed
    {
        ldarg.0
        ldc.i4     0xf4240
        stfld      int32 zzz::b
        ldarg.0
        ldc.i4     0xf4240
        stfld      int32 zzz::c
        ldarg.0
        call       instance void [mscorlib]System.Object::.ctor()
        ret
    }
}
```

11. `>>` 与 `<<` 操作符为 `SHR` 与 `SHL` 比 乘法2与除以2 要高效

12. `~` 非指令, `not` ;`%` 指令为 `Rem`

13. `&` 对应 `and`; `|` 对应 `or`; `^` 对应 `xor`;

[下一章:引用类型与值类型](il-reference-value.md)
