#### 动态添加Assembly,Module,Type

后期绑定代码以及动态执行代码

```
//获取当前的CurrentDomain
var currentDomain = AppDomain.CurrentDomain;

//在当前CurrentDomain中创建程序集
var assemblyName = new AssemblyName();
assemblyName.Name = "FastProxyAssembly";
var assemblyBuilder = currentDomain.DefineDynamicAssembly(assemblyName,AssemblyBuilderAccess.Run);

//在程序集中创建一个模块
var moduleBuilder = assemblyBuilder.DefineDynamicModule("FastProxyAssembly");

//在模块中创建类型
var typeBuilder = moduleBuilder.DefineType("FastProxyRequest",TypeAttributes.Public);

//向类型中添加成员(方法)
var methodBuilder = typeBuilder.DefineMethod("Welcome",MethodAttributes.Public,null,null);

//使用Emit来生成方法内部的逻辑
var methodGenIL = methodBuilder.GetILGenerator();
methodGenIL.EmitWriteLine("Welcome to Dynamic code generation!");
methodGenIL.Emit(OpCodes.Ret);

//最后一步,产生类型
var type = typeBuilder.CreateType();
```

上述动态产生类型`FastProxyRequest`后,使用如下

```
var fastProxy = Activator.CreateInstance(type);
```

### 获取`ILGenerator`的方式

1. `DynamicMethod` 的 `GetILGenerator()` 方法

1.1 基本的加法应用

```
//动态方法Add(加法):基本的参数为 1,方法名称;2,返回结果类型;3,参数类型集合数组
var dynamicMethod = new DynamicMethod("Add",typeof(int), new Type[] { typeof(int), typeof(int) });
var il = dynamicMethod.GetILGenerator();
il.Emit(OpCodes.Ldarg_0);
il.Emit(OpCodes.Ldarg_1);
il.Emit(OpCodes.Add);
il.Emit(OpCodes.Ret);

var delegateMethod = (Func<int,int,int>)dynamicMethod.CreateDelegate(typeof(Func<int,int,int>));
var result = delegateMethod(2,3);
```
1.2 委托实现获取源数据属性值,而不是使用反射

```
public delegate object PropertyValueFunc(object source);
//字段
public static FiledValueFunc CreateFiledValueFunc(Type type, FieldInfo fieldInfo)
{
    DynamicMethod dynamicGet = new DynamicMethod("DynamicGet", typeof(object),new Type[] { typeof(object) }, type, true);
    ILGenerator getGenerator = dynamicGet.GetILGenerator();

    getGenerator.Emit(OpCodes.Ldarg_0);
    getGenerator.Emit(OpCodes.Ldfld, fieldInfo);
    if (type.IsValueType)
    {
        getGenerator.Emit(OpCodes.Box, type);
    }
    getGenerator.Emit(OpCodes.Ret);

    return (PropertyValueFunc)dynamicGet.CreateDelegate(typeof(PropertyValueFunc));
}
//属性
public static PropertyValueFunc CreatePropertyValueFunc(Type type, MethodInfo propertyMethodInfo)
{
    DynamicMethod dynamicGet = new DynamicMethod("DynamicGet", typeof(object),new Type[] { typeof(object) }, type, true);
    ILGenerator getGenerator = dynamicGet.GetILGenerator();

    getGenerator.Emit(OpCodes.Ldarg_0);
    getGenerator.EmitCall(OpCodes.Callvirt, propertyMethodInfo, null);
    if (type.IsValueType)
    {
        getGenerator.Emit(OpCodes.Box, type);
    }
    getGenerator.Emit(OpCodes.Ret);

    return (PropertyValueFunc)dynamicGet.CreateDelegate(typeof(PropertyValueFunc));
}
```

2. `ConstructorBuilder` 的 `GetILGenerator()` 方法,动态构建构造函数

```
```

3. `MethodBuilder` 的 `GetILGenerator()` 方法,动态构建方法函数

```
```
