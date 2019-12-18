### 关键字与操作符

1. `return` return 后续的代码 IL将不进行翻译

```
class zzz
{
    public static void Main()
    {
        return;
        System.Console.WriteLine("hi");
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        br.s IL_0002
        IL_0002:  ret
    }
}
```

2. 带参数的构造函数,如果源代码中不存在构造函数，则会生成不带参数的构造函数。如果存在构造函数，则从代码中消除没有参数的构造函数。
总是在没有任何参数的情况下调用基类构造函数，并首先调用它。

```
class zzz
{
    public static void Main()
    {
    }
    zzz( int i)
    {
        System.Console.WriteLine("hi");
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ret
    }
    .method private hidebysig specialname rtspecialname instance void  .ctor(int32 i) il managed
    {
        ldarg.0  //加载对象的引用地址
        call instance void [mscorlib]System.Object::.ctor()  //初始化object
        ldstr "hi"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

3. 变量调用,调用指令ldloca.s，它将变量的地址放在堆栈上。如果指令是ldloc，则变量的值将放在堆栈上。
在函数调用中，我们在类型名称的末尾添加符号＆以指示变量的地址。＆后缀为数据类型表示变量的内存位置，而不是其中包含的值。
指令stind具体为 一个地址推送到堆栈上;一个值推送到堆栈上;从堆栈中弹出值和地址值存储在该地址。

```
class zzz
{
    public static void Main()
    {
        int i = 6;
        zzz a = new zzz();
        a.abc(ref i);
        System.Console.WriteLine(i);
    }
    public void abc(ref int i)
    {
        i = 10;
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (int32 V_0,class zzz V_1)
        ldc.i4.6
        stloc.0
        newobj instance void zzz::.ctor()
        stloc.1
        ldloc.1
        ldloca.s V_0  //把参数V_0的地址放入到堆栈中
        call instance void zzz::abc(int32&)
        ldloc.0
        call void [mscorlib]System.Console::WriteLine(int32)
        ret
    }
    .method public hidebysig instance void abc(int32& i) il managed
    {
        ldarg.1    //把参数1的地址放到堆栈上
        ldc.i4.s   10 //把int10 放到堆栈上
        stind.i4   //把10 存入 参数1的地址空间中
        ret
    }
}
```

4. 连接两个字符串，编译器的优化操作

```
class zzz
{
    public static void Main()
    {
        string s = "hi" + "bye";
        System.Console.WriteLine(s);
    }
}
//相应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class System.String V_0)
        ldstr      "hibye"
        stloc.0
        ldloc.0
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

5. 字符变量与字符串相加的时候,底层调用的是String.Concat

```
class zzz
{
    public static void Main()
    {
        string s = "hi" ;
        string t = s + "bye";
        System.Console.WriteLine(t);
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class System.String V_0,class System.String V_1)
        ldstr      "hi"
        stloc.0
        ldloc.0
        ldstr "bye"
        call class System.String [mscorlib]System.String::Concat(class System.String,class System.String)
        stloc.1
        ldloc.1
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

6. 字符串相等`==` 转换为 `String.Equals`

```
class zzz
{
    public static void Main()
    {
        string a = "bye";
        string b = "bye";
        System.Console.WriteLine(a == b);
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class System.String V_0,class System.String V_1)
        ldstr      "bye"
        stloc.0
        ldstr      "bye"
        stloc.1
        ldloc.0
        ldloc.1
        call bool [mscorlib]System.String::Equals(class System.String,class System.String)
        call void [mscorlib]System.Console::WriteLine(bool)
        ret
    }
}
```

7. `inheritance` 继承，继承的概念在支持它的所有编程语言中都是相同的, `extends`这个词起源于IL和Java而不是C#;
下面:
如果类xxx具有函数abc,则函数Main中的调用将具有前缀xxx
如果类yyy具有函数abc,则函数Main中的调用将具有前缀yyy
因此,决定调用哪个函数的abc的智能驻留在编译器中而不是生成的IL代码中

```
class zzz
{
    public static void Main()
    {
        xxx a = new xxx();
        a.abc();
    }
}
class yyy
{
    public void abc()
    {
        System.Console.WriteLine("yyy abc");
    }
}
class xxx : yyy
{
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class xxx V_0)
        newobj  instance void xxx::.ctor()
        stloc.0
        ldloc.0
        call instance void yyy::abc() //这里是编译器进行抉择的
        ret
    }
}
.class private auto ansi yyy extends [mscorlib]System.Object
{
    .method public hidebysig instance void abc() il managed
    {
        ldstr      "yyy abc"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
.class private auto ansi xxx extends yyy
{
}
```

8. 函数覆盖问题

```
class zzz
{
    public static void Main()
    {
        yyy a = new xxx();
        a.abc();
    }
}
class yyy
{
    public virtual void abc()
    {
        System.Console.WriteLine("yyy abc");
    }
}
class xxx : yyy
{
    public new void abc()
    {
        System.Console.WriteLine("xxx abc");
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
        newobj instance void xxx::.ctor()
        stloc.0
        ldloc.0
        callvirt instance void yyy::abc()   //callvirt 同样适用与接口
        ret
    }
}
.class private auto ansi yyy extends [mscorlib]System.Object
{
    .method public hidebysig newslot virtual instance void abc() il managed //基类函数 newslot virtual 标记为最新
    {
        ldstr      "yyy abc"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
.class private auto ansi xxx extends yyy
{
    .method public hidebysig instance void abc() il managed
    {
        ldstr      "xxx abc"
        call       void [mscorlib]System.Console::WriteLine(class System.String)
        ret
    }
}
```

9. `Finalize` 析构函数

```
class zzz
{
    public static void Main()
    {
    }
    ~zzz()
    {
        System.Console.WriteLine("hi");
    }
}
//对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .method public hidebysig static void Main() il managed
    {
        .entrypoint
        ret
    }
    .method family hidebysig virtual instance void Finalize() il managed
    {
        ldstr      "hi"
        call void [mscorlib]System.Console::WriteLine(class System.String)
        ldarg.0
        call       instance void [mscorlib]System.Object::Finalize() //最终都会调用Object的Finalize
        ret
    }
}
```

10. C# 不允许从某些类派生比如：`System.Array` ，但是IL中没有这个限制。C#中我们无法从Delegate 、Enum 、ValueType类型派生.

11. 常量是仅在编译时存在的实体,它在运行时不可见,证明编译器删除了所有编译时对象的痕迹。在转换为IL时,C#代码中出现的所有int i都被数字10替换。 `literal` 常量标量,`initonly` 标记 readonly

```
public class zzz
{
    const int i = 10;
    public static void Main()
    {
        System.Console.WriteLine(i);
    }
}
///对应的IL代码
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .field public static literal int32 i = int32(10)  //其中literal 常量
    .method public  hidebysig static void Main() il managed
    {
        .entrypoint
        ldc.i4.s   10
        call void [mscorlib]System.Console::WriteLine(int32)
        ret
        ret
    }
}
```

12. 类中的方法与类中的静态方法调用区别

```
public class zzz
{
    public static void Main()
    {
        zzz a = new zzz();
        pqr();
        a.abc();
    }
    public static void pqr()
    {
    }
    public void abc()
    {
    }
}
//对应的IL代码为
.assembly mukhi {}
.class private auto ansi zzz extends [mscorlib]System.Object
{
    .field public static initonly int32 i
    .method public  hidebysig static void Main() il managed
    {
        .entrypoint
        .locals (class zzz V_0)
        newobj instance void zzz::.ctor()
        stloc.0     //把栈顶的对象存入变量0中
        call void zzz::pqr()  //静态函数则是直接调用,且前面没有 instance关键字
        ldloc.0     //把变量0加载,进入栈顶
        call instance void zzz::abc()
        ret
    }
    .method public hidebysig static void pqr() il managed
    {
        ret
    }
    .method public hidebysig instance void abc() il managed
    {
        ret
    }
}
```

13. 属性的操作`ldfld` , 第一步: 对象引用 （或指针） 推送到堆栈上;第二步:从堆栈中弹出对象引用 （或指针）找到的对象中的指定字段的值;第三步:在字段中存储的值推送到堆栈上。

```
public class Room
{
	public int Number{get;set;}
}
//对应的IL
.class public auto ansi beforefieldinit Room extends [mscorlib]System.Object
{
    // Fields
    .field private int32 '<Number>k__BackingField'

    .method public hidebysig specialname instance int32 get_Number () cil managed
    {
        .maxstack 8

        IL_0000: ldarg.0
        IL_0001: ldfld int32 Room::'<Number>k__BackingField'  //必须先放入对象
        IL_0006: ret
    }

    .method public hidebysig specialname instance void set_Number (int32 'value') cil managed
    {
        .maxstack 8
        IL_0000: ldarg.0
        IL_0001: ldarg.1
        IL_0002: stfld int32 Room::'<Number>k__BackingField'
        IL_0007: ret
    }

    .method public hidebysig specialname rtspecialname instance void .ctor () cil managed
    {
        .maxstack 8

        IL_0000: ldarg.0
        IL_0001: call instance void [mscorlib]System.Object::.ctor()
        IL_0006: nop
        IL_0007: ret
    }

    // Properties
    .property instance int32 Number()
    {
        .get instance int32 Room::get_Number()
        .set instance void Room::set_Number(int32)
    }
}
```

[下一篇:运算符重载](il-operator-overloading.md)
