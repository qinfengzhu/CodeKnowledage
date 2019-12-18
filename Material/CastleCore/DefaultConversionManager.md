### `DefaultConversionManager`,组件类型转换管理

```
public class DefaultConversionManager : AbstractSubSystem, IConversionManager, ITypeConverterContext
{
    //获取线程槽,在线程上保存数据是一直的
    private static LocalDataStoreSlot slot = Thread.AllocateDataSlot();
	private IList converters;
	private IList standAloneConverters;

	public DefaultConversionManager()
	{
		converters = new ArrayList();
		standAloneConverters = new ArrayList();

		InitDefaultConverters();
	}

	protected virtual void InitDefaultConverters()
	{
		Add(new PrimitiveConverter());
		Add(new TimeSpanConverter());
		Add(new TypeNameConverter());
		Add(new EnumConverter());
		Add(new ListConverter());
		Add(new DictionaryConverter());
		Add(new GenericDictionaryConverter());
		Add(new GenericListConverter());
		Add(new ArrayConverter());
		Add(new ComponentConverter());
		Add(new AttributeAwareConverter());
		Add(new ComponentModelConverter());
	}

	#region IConversionManager Members
	public void Add(ITypeConverter converter)
	{
		converter.Context = this;

		converters.Add(converter);

		if (!(converter is IKernelDependentConverter))
		{
			standAloneConverters.Add(converter);
		}
	}

	public bool IsSupportedAndPrimitiveType(Type type)
	{
		foreach(ITypeConverter converter in standAloneConverters)
		{
			if (converter.CanHandleType(type)) return true;
		}

		return false;
	}

	#endregion

	#region ITypeConverter Members
	public ITypeConverterContext Context
	{
		get { return this; }
		set { throw new NotImplementedException(); }
	}

	public bool CanHandleType(Type type)
	{
		foreach(ITypeConverter converter in converters)
		{
			if (converter.CanHandleType(type)) return true;
		}

		return false;
	}

	public bool CanHandleType(Type type, IConfiguration configuration)
	{
		foreach(ITypeConverter converter in converters)
		{
			if (converter.CanHandleType(type, configuration)) return true;
		}

		return false;
	}

	public object PerformConversion(String value, Type targetType)
	{
		foreach(ITypeConverter converter in converters)
		{
			if (converter.CanHandleType(targetType))
			{
				return converter.PerformConversion(value, targetType);
			}
		}

		String message = String.Format("No converter registered to handle the type {0}",
		                               targetType.FullName);

		throw new ConverterException(message);
	}

	public object PerformConversion(IConfiguration configuration, Type targetType)
	{
		foreach(ITypeConverter converter in converters)
		{
			if (converter.CanHandleType(targetType, configuration))
			{
				return converter.PerformConversion(configuration, targetType);
			}
		}

		String message = String.Format("No converter registered to handle the type {0}",
		                               targetType.FullName);

		throw new ConverterException(message);
	}

	#endregion

	#region ITypeConverterContext Members
	IKernel ITypeConverterContext.Kernel
	{
		get { return base.Kernel; }
	}

	public void PushModel(ComponentModel model)
	{
		CurrentStack.Push(model);
	}

	public void PopModel()
	{
		CurrentStack.Pop();
	}

	public ComponentModel CurrentModel
	{
		get
		{
			if (CurrentStack.Count == 0) return null;
			else return (ComponentModel) CurrentStack.Peek();
		}
	}

	public ITypeConverter Composition
	{
		get { return this; }
	}
	#endregion

	private Stack CurrentStack
	{
		get
		{
			Stack stack = (Stack) Thread.GetData(slot);

			if (stack == null)
			{
				stack = new Stack();
				Thread.SetData(slot, stack);//在槽中放数据
			}

			return stack;
		}
	}
}
```

1. 内置转换类型支持`PrimitiveConverter`原始转换器,对.net的基本数据类型支持

```
public class PrimitiveConverter : AbstractTypeConverter
{
	private Type[] types;

	public PrimitiveConverter()
	{
		types = new Type[]
			{
				typeof(Char),
				typeof(DateTime),
				typeof(Decimal),
				typeof(Boolean),
				typeof(Int16),
				typeof(Int32),
				typeof(Int64),
				typeof(UInt16),
				typeof(UInt32),
				typeof(UInt64),
				typeof(Byte),
				typeof(SByte),
				typeof(Single),
				typeof(Double),
				typeof(String)
			};
	}

	public override bool CanHandleType(Type type)
	{
		return Array.IndexOf(types, type) != -1;
	}

	public override object PerformConversion(String value, Type targetType)
	{
		if (targetType == typeof(String)) return value;

		try
		{
			return Convert.ChangeType(value, targetType);
		}
		catch(Exception ex)
		{
			String message = String.Format(
				"Could not convert from '{0}' to {1}",
				value, targetType.FullName);

			throw new ConverterException(message, ex);
		}
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}
}
```

2. 内置转换器对 `TimeSpan`类型的支持转换器 `TimeSpanConverter`

```
public class TimeSpanConverter : AbstractTypeConverter
{
    public override bool CanHandleType(Type type)
    {
        return type == typeof(TimeSpan);
    }

    public override object PerformConversion(string value, Type targetType)
    {
        try
        {
            return TimeSpan.Parse(value);
        }
        catch (Exception ex)
        {
            String message = String.Format(
                "Could not convert from '{0}' to {1}",
                value, targetType.FullName);

            throw new ConverterException(message, ex);
        }
    }

    public override object PerformConversion(Castle.Core.Configuration.IConfiguration configuration, Type targetType)
    {
        return PerformConversion(configuration.Value, targetType);
    }
}
```
3. 内置转换器,根据TypeName转换为Type,`TypeNameConverter`

```
public class TypeNameConverter : AbstractTypeConverter
{
	public override bool CanHandleType(Type type)
	{
		return type == typeof(Type);
	}

	public override object PerformConversion(String value, Type targetType)
	{
		try
		{
			Type type = Type.GetType(value, true, false);

			if (type == null)
			{
				String message = String.Format(
					"Could not convert from '{0}' to {1} - Maybe type could not be found",
					value, targetType.FullName);

				throw new ConverterException(message);
			}

			return type;
		}
		catch(ConverterException)
		{
			throw;
		}
		catch(Exception ex)
		{
			String message = String.Format(
				"Could not convert from '{0}' to {1}.",
				value, targetType.FullName);

			throw new ConverterException(message, ex);
		}
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}
}
```
4. 内置转换器,枚举与字符串的转换,`EnumConverter`

```
public class EnumConverter : AbstractTypeConverter
{
	public override bool CanHandleType(Type type)
	{
		return type.IsEnum;
	}

	public override object PerformConversion(String value, Type targetType)
	{
		try
		{
			return Enum.Parse( targetType, value, true );
		}
		catch(ConverterException)
		{
			throw;
		}
		catch(Exception ex)
		{
			String message = String.Format(
				"Could not convert from '{0}' to {1}.",
				value, targetType.FullName);

			throw new ConverterException(message, ex);
		}
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}
}
```

5. 内置转换器 `ListConverter`,主要是 `List`类型

```
public class ListConverter : AbstractTypeConverter
{
	public ListConverter()
	{
	}

	public override bool CanHandleType(Type type)
	{
		return (type == typeof(IList) || type == typeof(ArrayList));
	}

	public override object PerformConversion(String value, Type targetType)
	{
        //因为存在泛型,所以这种事没有办法实现的,只有通过下面的IConfiguration配置或者
        //调用Context 也就是参数用其它转换器来做,递归后 最终数据可以转为对基础数据的支持
		throw new NotImplementedException();
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		ArrayList list = new ArrayList();

		String itemType = configuration.Attributes["type"];
		Type convertTo = typeof(String);

		if (itemType != null)
		{
			convertTo = (Type) Context.Composition.PerformConversion( itemType, typeof(Type) );
		}

		foreach(IConfiguration itemConfig in configuration.Children)
		{
			list.Add( Context.Composition.PerformConversion(itemConfig.Value, convertTo) );
		}

		return list;
	}
}
```

6. 内置转换器 `DictionaryConverter` 主要是针对 `Dictionary` 类型

```
public class DictionaryConverter : AbstractTypeConverter
{
	public override bool CanHandleType(Type type)
	{
		return (type == typeof(IDictionary) || type == typeof(Hashtable));
	}

	public override object PerformConversion(String value, Type targetType)
	{
		throw new NotImplementedException();
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		Hashtable dict = new Hashtable();

		String keyTypeName = configuration.Attributes["keyType"];
		Type defaultKeyType = typeof(String);

		String valueTypeName = configuration.Attributes["valueType"];
		Type defaultValueType = typeof(String);

		if (keyTypeName != null)
		{
			defaultKeyType = (Type) Context.Composition.PerformConversion( keyTypeName, typeof(Type) );
		}
		if (valueTypeName != null)
		{
			defaultValueType = (Type) Context.Composition.PerformConversion( valueTypeName, typeof(Type) );
		}

		foreach(IConfiguration itemConfig in configuration.Children)
		{
			// Preparing the key

			String keyValue = itemConfig.Attributes["key"];

			if (keyValue == null)
			{
				throw new ConverterException("You must provide a key for the dictionary entry");
			}

			Type convertKeyTo = defaultKeyType;

			if (itemConfig.Attributes["keyType"] != null)
			{
				convertKeyTo = (Type) Context.Composition.PerformConversion(
					itemConfig.Attributes["keyType"], typeof(Type) );
			}

			object key = Context.Composition.PerformConversion(keyValue, convertKeyTo);

			// Preparing the value
			Type convertValueTo = defaultValueType;

			if (itemConfig.Attributes["valueType"] != null)
			{
				convertValueTo = (Type) Context.Composition.PerformConversion(
					itemConfig.Attributes["valueType"], typeof(Type) );
			}

			object value;

			if (itemConfig.Children.Count == 0)
			{
				value = Context.Composition.PerformConversion(
					itemConfig, convertValueTo);
			}
			else
			{
				value = Context.Composition.PerformConversion(
					itemConfig.Children[0], convertValueTo);
			}

			dict.Add( key, value );
		}

		return dict;
	}
}
```

7. 内置转换器,对泛型字典的支持 `GenericDictionaryConverter`

```
public class GenericDictionaryConverter : AbstractTypeConverter
{
	public GenericDictionaryConverter()
	{
	}

	public override bool CanHandleType(Type type)
	{
		if (!type.IsGenericType) return false;

		Type genericDef = type.GetGenericTypeDefinition();

		return (genericDef == typeof(IDictionary<,>) || genericDef == typeof(Dictionary<,>));
	}

	public override object PerformConversion(String value, Type targetType)
	{
		throw new NotImplementedException();
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		Type[] argTypes = targetType.GetGenericArguments();

		if (argTypes.Length != 2)
		{
			throw new ConverterException("Expected type with two generic arguments.");
		}

		String keyTypeName = configuration.Attributes["keyType"];
		Type defaultKeyType = argTypes[0];

		String valueTypeName = configuration.Attributes["valueType"];
		Type defaultValueType = argTypes[1];

		if (keyTypeName != null)
		{
			defaultKeyType = (Type) Context.Composition.PerformConversion(keyTypeName, typeof(Type));
		}

		if (valueTypeName != null)
		{
			defaultValueType = (Type) Context.Composition.PerformConversion(valueTypeName, typeof(Type));
		}

		IGenericCollectionConverterHelper collectionConverterHelper =
			(IGenericCollectionConverterHelper)
			Activator.CreateInstance(typeof(DictionaryHelper<,>).MakeGenericType(defaultKeyType, defaultValueType), this);

		return collectionConverterHelper.ConvertConfigurationToCollection(configuration);
	}

	private class DictionaryHelper<TKey, TValue> : IGenericCollectionConverterHelper
	{
		private GenericDictionaryConverter parent;

		public DictionaryHelper(GenericDictionaryConverter parent)
		{
			this.parent = parent;
		}

		public object ConvertConfigurationToCollection(IConfiguration configuration)
		{
			Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

			foreach(IConfiguration itemConfig in configuration.Children)
			{
				// Preparing the key

				String keyValue = itemConfig.Attributes["key"];

				if (keyValue == null)
				{
					throw new ConverterException("You must provide a key for the dictionary entry");
				}

				Type convertKeyTo = typeof(TKey);

				if (itemConfig.Attributes["keyType"] != null)
				{
					convertKeyTo = (Type) parent.Context.Composition.PerformConversion(itemConfig.Attributes["keyType"], typeof(Type));
				}

				if (!typeof(TKey).IsAssignableFrom(convertKeyTo))
				{
					throw new ArgumentException(
						string.Format("Could not create dictionary<{0},{1}> because {2} is not assignmable to key type {0}", typeof(TKey),
						              typeof(TValue), convertKeyTo));
				}

				TKey key = (TKey) parent.Context.Composition.PerformConversion(keyValue, convertKeyTo);

				// Preparing the value

				Type convertValueTo = typeof(TValue);

				if (itemConfig.Attributes["valueType"] != null)
				{
					convertValueTo =
						(Type) parent.Context.Composition.PerformConversion(itemConfig.Attributes["valueType"], typeof(Type));
				}

				if (!typeof(TValue).IsAssignableFrom(convertValueTo))
				{
					throw new ArgumentException(
						string.Format("Could not create dictionary<{0},{1}> because {2} is not assignmable to value type {1}",
						              typeof(TKey), typeof(TValue), convertValueTo));
				}
				TValue value = (TValue) parent.Context.Composition.PerformConversion(itemConfig.Value, convertValueTo);

				dict.Add(key, value);
			}
			return dict;
		}
	}
}
```

8. 内置转换器 `GenericListConverter` 对泛型List的支持

```
public class GenericListConverter : AbstractTypeConverter
{
	public GenericListConverter()
	{
	}

	public override bool CanHandleType(Type type)
	{
		if (!type.IsGenericType) return false;

		Type genericDef = type.GetGenericTypeDefinition();

		return (genericDef == typeof(IList<>)
		        || genericDef == typeof(ICollection<>)
		        || genericDef == typeof(List<>)
		        || genericDef == typeof(IEnumerable<>));
	}

	public override object PerformConversion(String value, Type targetType)
	{
		throw new NotImplementedException();
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		Type[] argTypes = targetType.GetGenericArguments();

		if (argTypes.Length != 1)
		{
			throw new ConverterException("Expected type with one generic argument.");
		}

		String itemType = configuration.Attributes["type"];
		Type convertTo = argTypes[0];

		if (itemType != null)
		{
			convertTo = (Type) Context.Composition.PerformConversion(itemType, typeof(Type));
		}

		IGenericCollectionConverterHelper converterHelper = (IGenericCollectionConverterHelper)
		                                                    Activator.CreateInstance(
		                                                    	typeof(ListHelper<>).MakeGenericType(convertTo),
		                                                    	this);
		return converterHelper.ConvertConfigurationToCollection(configuration);
	}

	private class ListHelper<T> : IGenericCollectionConverterHelper
	{
		private GenericListConverter parent;

		public ListHelper(GenericListConverter parent)
		{
			this.parent = parent;
		}

		public object ConvertConfigurationToCollection(IConfiguration configuration)
		{
			List<T> list = new List<T>();

			foreach(IConfiguration itemConfig in configuration.Children)
			{
				T item = (T) parent.Context.Composition.PerformConversion(itemConfig, typeof(T));
				list.Add(item);
			}

			return list;
		}
	}
}
```

9. 内置转换器对 Array的支持

```
public class ArrayConverter : AbstractTypeConverter
{
	public override bool CanHandleType(Type type)
	{
		return type.IsArray;
	}

	public override object PerformConversion(String value, Type targetType)
	{
		throw new NotImplementedException();
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		int count = configuration.Children.Count;
		Type itemType = targetType.GetElementType();

		Array array = Array.CreateInstance(itemType, count);

		int index = 0;
		foreach(IConfiguration itemConfig in configuration.Children)
		{
			object value = Context.Composition.PerformConversion(itemConfig, itemType);
			array.SetValue(value, index++);
		}

		return array;
	}
}
```
10. 内置转换器对组件转换的支持 `ComponentConverter`

```
public class ComponentConverter : AbstractTypeConverter, IKernelDependentConverter
{
	public override bool CanHandleType(Type type, IConfiguration configuration)
	{
		if (configuration.Value != null)
		{
			return ReferenceExpressionUtil.IsReference(configuration.Value.Trim());
		}
		else
		{
			return CanHandleType(type);
		}
	}

	public override bool CanHandleType(Type type)
	{
		if (Context.Kernel == null) return false;

		return Context.Kernel.HasComponent(type);
	}

	public override object PerformConversion(String value, Type targetType)
	{
		if (!ReferenceExpressionUtil.IsReference(value))
		{
			String message = String.Format("Could not convert expression {0} to type {1}. Expecting a reference override like ${some key}", value, targetType.FullName);
			throw new ConverterException(message);
		}

		String key = ReferenceExpressionUtil.ExtractComponentKey(value);

		DependencyModel dependency = new DependencyModel(DependencyType.ServiceOverride, key, targetType, false);

		return Context.Kernel.Resolver.Resolve(CreationContext.Empty, null, Context.CurrentModel, dependency);
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}
}
```

11. 内置转换器,对指定转换器的支持, `AttributeAwareConverter`,通过标记`ConvertibleAttribute` 设置Type 为一个`ITypeConverter`

```
public class AttributeAwareConverter : AbstractTypeConverter
{
	#region ITypeConverter Member

	public override bool CanHandleType(Type type)
	{
		ITypeConverter converter = TryGetConverterInstance(type);

		if (converter != null)
		{
			return converter.CanHandleType(type);
		}
		else
		{
			return false;
		}
	}

	public override object PerformConversion(string value, Type targetType)
	{
		ITypeConverter converter = GetConverterInstance(targetType);
		return converter.PerformConversion(value, targetType);
	}

	public override object PerformConversion(Castle.Core.Configuration.IConfiguration configuration, Type targetType)
	{
		ITypeConverter converter = GetConverterInstance(targetType);
		return converter.PerformConversion(configuration, targetType);
	}

	#endregion

	private ITypeConverter TryGetConverterInstance(Type type)
	{
		ITypeConverter converter = null;

		ConvertibleAttribute attr = (ConvertibleAttribute)
			Attribute.GetCustomAttribute(type, typeof(ConvertibleAttribute));

		if (attr != null)
		{
			converter = (ITypeConverter) Activator.CreateInstance(attr.ConverterType);
			converter.Context = Context;
		}

		return converter;
	}

	private ITypeConverter GetConverterInstance(Type type)
	{
		ITypeConverter converter = TryGetConverterInstance(type);

		if (converter == null)
		{
			throw new InvalidOperationException("Type " + type.Name + " does not have a Convertible attribute.");
		}

		return converter;
	}
}
```

12. 内置转换器,对`System.ComponentModel` 程序集中的 `System.ComponentModel.TypeConverter`类型转换的支持

```
public class ComponentModelConverter : AbstractTypeConverter
{
	public override bool CanHandleType(Type type)
	{
		// Mono 1.9+ thinks it can convert strings to interface
		if (type.IsInterface)
			return false;

		TypeConverter converter = TypeDescriptor.GetConverter(type);
		return (converter != null && converter.CanConvertFrom(typeof(String)));
	}

	public override object PerformConversion(String value, Type targetType)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(targetType);

		try
		{
			return converter.ConvertFrom(value);
		}
		catch(Exception ex)
		{
			String message = String.Format(
				"Could not convert from '{0}' to {1}",
				value, targetType.FullName);

			throw new ConverterException(message, ex);
		}
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}
}
```
