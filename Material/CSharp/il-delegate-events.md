### IL 的Delegate 与 Event

1. `Exception` 在IL中操作

```
public class Demo
{
  public static void Main()
  {
    try
    {
      abc();
      System.Console.WriteLine("Bye");
    }
    catch(System.Exception ex)
    {
      System.Console.WriteLine("In Exception");
    }
    System.Console.WriteLine("After Exception");
  }

  public static void abc()
  {
    throw new System.Exception();
  }
}
```
对应的IL
```
.method public hidebysig static void Main () cil managed
{
    .maxstack 1
    .locals init (
        [0] class [mscorlib]System.Exception
    )

    IL_0000: nop
    .try
    {
        IL_0001: nop
        IL_0002: call void Demo::abc()
        IL_0007: nop
        IL_0008: ldstr "Bye"
        IL_000d: call void [mscorlib]System.Console::WriteLine(string)
        IL_0012: nop
        IL_0013: nop
        IL_0014: leave.s IL_0026
    } // end .try
    catch [mscorlib]System.Exception
    {
        IL_0016: stloc.0
        IL_0017: nop
        IL_0018: ldstr "In Exception"
        IL_001d: call void [mscorlib]System.Console::WriteLine(string)
        IL_0022: nop
        IL_0023: nop
        IL_0024: leave.s IL_0026
    }

    IL_0026: ldstr "After Exception"
    IL_002b: call void [mscorlib]System.Console::WriteLine(string)
    IL_0030: nop
    IL_0031: ret
}

.method public hidebysig static void abc () cil managed
{
    .maxstack 8

    IL_0000: nop
    IL_0001: newobj instance void [mscorlib]System.Exception::.ctor()
    IL_0006: throw
}

.method public hidebysig specialname rtspecialname instance void .ctor () cil managed
{
    .maxstack 8

    IL_0000: ldarg.0
    IL_0001: call instance void [mscorlib]System.Object::.ctor()
    IL_0006: nop
    IL_0007: ret
}
```

2. 委托

```
public delegate int GetAge(string userName);
```
对应的IL代码
```
.class public auto ansi sealed GetAge extends [mscorlib]System.MulticastDelegate
{
    .method public hidebysig specialname rtspecialname instance void .ctor (object 'object',  native int 'method') runtime managed
    {
    }

    .method public hidebysig newslot virtual instance int32 Invoke (string userName) runtime managed
    {
    }

    .method public hidebysig newslot virtual  instance class [mscorlib]System.IAsyncResult BeginInvoke(
            string userName,class [mscorlib]System.AsyncCallback callback,object 'object') runtime managed
    {
    }

    .method public hidebysig newslot virtual instance int32 EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
    {
    }
}
```

3. 事件

```
public delegate void ParseAge(string userName);

public class Student
{
    public event ParseAge TryParseAge;
    public void TriggerMethod()
    {
	    if(TryParseAge!=null)
	    {
		    TryParseAge("Student");
	    }
    }
}
```

对应的IL 代码

```
.class public auto ansi beforefieldinit Student
    extends [mscorlib]System.Object
{
    // Fields
    .field private class ParseAge TryParseAge
    // Methods
    .method public hidebysig specialname instance void add_TryParseAge(class ParseAge 'value') cil managed
    {
        .maxstack 3
        .locals init (
            [0] class ParseAge,
            [1] class ParseAge,
            [2] class ParseAge
        )

        IL_0000: ldarg.0
        IL_0001: ldfld class ParseAge Student::TryParseAge
        IL_0006: stloc.0
        // loop start (head: IL_0007)
            IL_0007: ldloc.0
            IL_0008: stloc.1
            IL_0009: ldloc.1
            IL_000a: ldarg.1
            IL_000b: call class [mscorlib]System.Delegate [mscorlib]System.Delegate::Combine(class [mscorlib]System.Delegate, class [mscorlib]System.Delegate)
            IL_0010: castclass ParseAge
            IL_0015: stloc.2
            IL_0016: ldarg.0
            IL_0017: ldflda class ParseAge Student::TryParseAge
            IL_001c: ldloc.2
            IL_001d: ldloc.1
            IL_001e: call !!0 [mscorlib]System.Threading.Interlocked::CompareExchange<class ParseAge>(!!0&, !!0, !!0)
            IL_0023: stloc.0
            IL_0024: ldloc.0
            IL_0025: ldloc.1
            IL_0026: bne.un.s IL_0007
        // end loop
        IL_0028: ret
    } // end of method Student::add_TryParseAge

    .method public hidebysig specialname instance void remove_TryParseAge (class ParseAge 'value') cil managed
    {
        .maxstack 3
        .locals init (
            [0] class ParseAge,
            [1] class ParseAge,
            [2] class ParseAge
        )

        IL_0000: ldarg.0
        IL_0001: ldfld class ParseAge Student::TryParseAge
        IL_0006: stloc.0
        // loop start (head: IL_0007)
            IL_0007: ldloc.0
            IL_0008: stloc.1
            IL_0009: ldloc.1
            IL_000a: ldarg.1
            IL_000b: call class [mscorlib]System.Delegate [mscorlib]System.Delegate::Remove(class [mscorlib]System.Delegate, class [mscorlib]System.Delegate)
            IL_0010: castclass ParseAge
            IL_0015: stloc.2
            IL_0016: ldarg.0
            IL_0017: ldflda class ParseAge Student::TryParseAge
            IL_001c: ldloc.2
            IL_001d: ldloc.1
            IL_001e: call !!0 [mscorlib]System.Threading.Interlocked::CompareExchange<class ParseAge>(!!0&, !!0, !!0)
            IL_0023: stloc.0
            IL_0024: ldloc.0
            IL_0025: ldloc.1
            IL_0026: bne.un.s IL_0007
        // end loop
        IL_0028: ret
    } // end of method Student::remove_TryParseAge

    .method public hidebysig instance void TriggerMethod () cil managed
    {
        .locals init (
            [0] bool
        )

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: ldfld class ParseAge Student::TryParseAge
        IL_0007: ldnull
        IL_0008: cgt.un
        IL_000a: stloc.0
        // sequence point: hidden
        IL_000b: ldloc.0
        IL_000c: brfalse.s IL_0021

        IL_000e: nop
        IL_000f: ldarg.0
        IL_0010: ldfld class ParseAge Student::TryParseAge
        IL_0015: ldstr "Student"
        IL_001a: callvirt instance void ParseAge::Invoke(string)
        IL_001f: nop
        IL_0020: nop

        IL_0021: ret
    } // end of method Student::TriggerMethod

    .method public hidebysig specialname rtspecialname instance void .ctor () cil managed
    {
        IL_0000: ldarg.0
        IL_0001: call instance void [mscorlib]System.Object::.ctor()
        IL_0006: nop
        IL_0007: ret
    } // end of method Student::.ctor

    // Events
    .event ParseAge TryParseAge
    {
        .addon instance void Student::add_TryParseAge(class ParseAge)
        .removeon instance void Student::remove_TryParseAge(class ParseAge)
    }
}
```
