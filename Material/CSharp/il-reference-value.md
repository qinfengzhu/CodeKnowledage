### 引用类型与值类型

1. 值类型与引用类型IL初始化

```
public class Product
{
    public Product(ProductPosition x)
    {

    }
}
public struct ProductPosition
{
    public int X;
    public int Y;
    public ProductPosition(int x,int y)
    {
        X=x;
        Y=y;
    }
}

public class Program
{
    static void Main(params object[] objects)
    {
        var position = new ProductPosition(2,3);
        var product = new Product(position);
    }
}
//对应的IL代码
.class public sequential ansi sealed beforefieldinit ProductPosition
    extends [mscorlib]System.ValueType  //这里是重点 sealed  System.ValueType
{
    // Fields
    .field public int32 X
    .field public int32 Y

    // Methods
    .method public hidebysig specialname rtspecialname instance void .ctor (int32 x,int32 y) cil managed
    {
        // Method begins at RVA 0x205a
        // Code size 16 (0x10)
        .maxstack 8

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: ldarg.1
        IL_0003: stfld int32 ProductPosition::X
        IL_0008: ldarg.0
        IL_0009: ldarg.2
        IL_000a: stfld int32 ProductPosition::Y
        IL_000f: ret
    }
}
.class public auto ansi beforefieldinit Program
    extends [mscorlib]System.Object
{
    .method private hidebysig static void Main (object[] objects) cil managed
    {
        .maxstack 3
        .locals init (
            [0] valuetype ProductPosition,
            [1] class Product
        )

        IL_0000: nop
        IL_0001: ldloca.s 0
        IL_0003: ldc.i4.2
        IL_0004: ldc.i4.3
        IL_0005: call instance void ProductPosition::.ctor(int32, int32) //值类型调用初始化方式
        IL_000a: ldloc.0
        IL_000b: newobj instance void Product::.ctor(valuetype ProductPosition) //引用类型初始化方式
        IL_0010: stloc.1
        IL_0011: ret
    }
}

```

2. 引用类型强转换 `Castclass`

[属性与索引](il-properties.md)
