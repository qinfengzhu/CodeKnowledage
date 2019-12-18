### 属性与索引

1. 简单属性代码

```
public class Demo
{
  public static void Main()
  {
    var a = new Student();
    int score = a.Score+10;
    System.Console.WriteLine(score);
  }
}
public class Student
{
  private System.Collections.Generic.List<int> arrayValues = new System.Collections.Generic.List<int>();
  private int _score;
  public int Score
  {
    get
    {
        return _score;
    }
    set
    {
        _score=value;
    }
  }
  public int this[int i]
  {
      set
      {
          arrayValues[i]=value;
      }
      get
      {
          return arrayValues[i];
      }
  }
}
```

对应的IL 代码

```
.class public auto ansi beforefieldinit Demo extends [mscorlib]System.Object
{
    .method public hidebysig static void Main () cil managed
    {
        .maxstack 2
        .locals init ([0] class Student,[1] int32)

        IL_0000: nop
        IL_0001: newobj instance void Student::.ctor()
        IL_0006: stloc.0
        IL_0007: ldloc.0
        IL_0008: callvirt instance int32 Student::get_Score()
        IL_000d: ldc.i4.s 10
        IL_000f: add
        IL_0010: stloc.1
        IL_0011: ldloc.1
        IL_0012: call void [mscorlib]System.Console::WriteLine(int32)
        IL_0017: nop
        IL_0018: ret
    } // end of method Demo::Main

    .method public hidebysig specialname rtspecialname instance void .ctor () cil managed
    {
        .maxstack 8

        IL_0000: ldarg.0
        IL_0001: call instance void [mscorlib]System.Object::.ctor()
        IL_0006: nop
        IL_0007: ret
    } // end of method Demo::.ctor

} // end of class Demo

.class public auto ansi beforefieldinit Student extends [mscorlib]System.Object
{
    .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string) = (01 00 04 49 74 65 6d 00 00)
    // Fields
    .field private class [mscorlib]System.Collections.Generic.List`1<int32> arrayValues
    .field private int32 _score

    // Methods
    .method public hidebysig specialname instance int32 get_Score () cil managed
    {
        .maxstack 1
        .locals init ([0] int32)

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: ldfld int32 Student::_score
        IL_0007: stloc.0
        IL_0008: br.s IL_000a

        IL_000a: ldloc.0
        IL_000b: ret
    } // end of method Student::get_Score

    .method public hidebysig specialname instance void set_Score (int32 'value') cil managed
    {
        .maxstack 8

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: ldarg.1
        IL_0003: stfld int32 Student::_score
        IL_0008: ret
    } // end of method Student::set_Score

    .method public hidebysig specialname instance void set_Item (int32 i, int32 'value') cil managed
    {
        .maxstack 8

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: ldfld class [mscorlib]System.Collections.Generic.List`1<int32> Student::arrayValues
        IL_0007: ldarg.1
        IL_0008: ldarg.2
        IL_0009: callvirt instance void class [mscorlib]System.Collections.Generic.List`1<int32>::set_Item(int32, !0)
        IL_000e: nop
        IL_000f: ret
    } // end of method Student::set_Item

    .method public hidebysig specialname instance int32 get_Item (int32 i) cil managed
    {
        .maxstack 2
        .locals init ([0] int32)

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: ldfld class [mscorlib]System.Collections.Generic.List`1<int32> Student::arrayValues
        IL_0007: ldarg.1
        IL_0008: callvirt instance !0 class [mscorlib]System.Collections.Generic.List`1<int32>::get_Item(int32)
        IL_000d: stloc.0
        IL_000e: br.s IL_0010

        IL_0010: ldloc.0
        IL_0011: ret
    } // end of method Student::get_Item

    .method public hidebysig specialname rtspecialname instance void .ctor () cil managed
    {
        .maxstack 8

        IL_0000: ldarg.0
        IL_0001: newobj instance void class [mscorlib]System.Collections.Generic.List`1<int32>::.ctor()
        IL_0006: stfld class [mscorlib]System.Collections.Generic.List`1<int32> Student::arrayValues
        IL_000b: ldarg.0
        IL_000c: call instance void [mscorlib]System.Object::.ctor()
        IL_0011: nop
        IL_0012: ret
    } // end of method Student::.ctor

    // Properties
    .property instance int32 Score()
    {
        .get instance int32 Student::get_Score()
        .set instance void Student::set_Score(int32)
    }
    .property instance int32 Item(int32 i)
    {
        .get instance int32 Student::get_Item(int32)
        .set instance void Student::set_Item(int32, int32)
    }

}
```

[委托与事件](il-delegate-events.md)
