### `DefaultComponentModelBuilder` 分析,主要的方法为 `BuildModel` 构建 `ComponentModel`,后续的所有操作基本都围绕 `ComponenetModel`

1. `DefaultComponentModelBuilder` 继承于 `IComponentModelBuilder`接口，而`IComponentModelBuilder`又借助于
`IContributeComponentModelConstruction` 接口对 `ComponentModel` 类中的数据进行补充，也就是完善 `ComponentModel`中的数据

2. `IContributeComponentModelConstruction` 接口的实现,目前有
`GenericInspector`:泛类型补充
`ConfigurationModelInspector`: 配置文件中关键字补充
`ConfigurationParametersInspector`:配置文件中参数补充
`LifestyleModelInspector`: 生命周期补充
`ConstructorDependenciesModelInspector`:构造函数依赖补充
`PropertiesDependenciesModelInspector`:属性依赖补充
`LifecycleModelInspector`:回收依赖补充
`InterceptorInspector` :拦截器依赖补充
`ComponentActivatorInspector` :组件激活器补充，还是读取配置信息
`ComponentProxyInspector` : 代理类补充

```
/// <summary>
/// 实现者必须检查给定信息或参数的组件
/// </summary>
public interface IContributeComponentModelConstruction
{
    /// <summary>
    /// 通常，实现将着眼于模型或服务接口的配置属性，或者寻找某些东西的实现
    /// </summary>
    /// <param name="kernel">Kernel</param>
    /// <param name="model">组件模型</param>
    void ProcessModel(IKernel kernel, ComponentModel model);
}
```

3. `DefaultComponentModelBuilder` 类中的默认构造`ComponentModel`的方法

```
//一般情况下,extendedProperties为null
public ComponentModel BuildModel(String key, Type service, Type classType, IDictionary extendedProperties)
{
    ComponentModel model = new ComponentModel(key, service, classType); //CompnentModel默认的构造函数

    if (extendedProperties != null)
    {
        model.ExtendedProperties = extendedProperties;
    }

    foreach(IContributeComponentModelConstruction contributor in contributors)
    {
        //默认的Contributor进行各个部分处理
        contributor.ProcessModel( kernel, model );
    }

    return model;
}
```

4. `ComponentModel` 默认构造函数

```
//这里的图节点GraphNode比较有意思,部分代码省略
public sealed class ComponentModel : GraphNode
{
	/// <summary>
	/// Constructs a ComponentModel
	/// </summary>
	public ComponentModel(String name, Type service, Type implementation)
	{
		this.name = name;
		this.service = service;
		this.implementation = implementation;
		lifestyleType = LifestyleType.Undefined;
		inspectionBehavior = PropertiesInspectionBehavior.Undefined;
	}

	/// <summary>
	/// Sets or returns the component key
	/// </summary>
	public String Name
	{
		get { return name; }
		set { name = value; }
	}

	/// <summary>
	/// Gets or sets the service exposed.
	/// </summary>
	/// <value>The service.</value>
	public Type Service
	{
		get { return service; }
		set { service = value; }
	}

	/// <summary>
	/// Gets or sets the component implementation.
	/// </summary>
	/// <value>The implementation.</value>
	public Type Implementation
	{
		get { return implementation; }
		set { implementation = value; }
	}
}
```

5. 分析 `GenericInspector`:泛类型补充

```
public class GenericInspector : IContributeComponentModelConstruction
{
    public void ProcessModel(IKernel kernel, ComponentModel model)
    {
        //这里确定服务是否为泛型,而不是实现为泛型
        model.RequiresGenericArguments = model.Service.IsGenericTypeDefinition;
    }
}
```

6. 分析 `ConfigurationModelInspector`:组件配置注册,关于配置的存储见[DefaultConfigurationStore](DefaultConfigurationStore.md),
有且仅当进行了 `kernel.ConfigurationStore.AddComponentConfiguration("componentKey",new MutableConfiguration("key"))`

```
public virtual void ProcessModel(IKernel kernel, ComponentModel model)
{
    IConfiguration config = kernel.ConfigurationStore.GetComponentConfiguration(model.Name) ??
                            kernel.ConfigurationStore.GetBootstrapComponentConfiguration(model.Name);

    model.Configuration = config;
}
//Kernel中
public virtual IConfigurationStore ConfigurationStore
{
    get { return GetSubSystem(SubSystemConstants.ConfigurationStoreKey) as IConfigurationStore; }
    set { AddSubSystem(SubSystemConstants.ConfigurationStoreKey, value); }
}
protected virtual void RegisterSubSystems()
{
    AddSubSystem(SubSystemConstants.ConfigurationStoreKey,
                 new DefaultConfigurationStore());

    AddSubSystem(SubSystemConstants.ConversionManagerKey,
                 new DefaultConversionManager());

    AddSubSystem(SubSystemConstants.NamingKey,
                 new DefaultNamingSubSystem());

    AddSubSystem(SubSystemConstants.ResourceKey,
                 new DefaultResourceSubSystem());
}
```

7. `ConfigurationParametersInspector`:组件配置参数补充,当`ComponentModel.Configuration`不为空的时候

```
public class ConfigurationParametersInspector : IContributeComponentModelConstruction
{
    /// <summary>
    /// 检查与组件关联的配置并相应地填充参数模型集合
    /// </summary>
    public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (model.Configuration == null) return;

		IConfiguration parameters = model.Configuration.Children["parameters"];

		if (parameters == null) return;

		foreach(IConfiguration parameter in parameters.Children)
		{
			String name = parameter.Name;
			String value = parameter.Value;

			if (value == null && parameter.Children.Count != 0)
			{
				IConfiguration parameterValue = parameter.Children[0];
				model.Parameters.Add(name, parameterValue);
			}
			else
			{
				model.Parameters.Add(name, value);
			}
		}

		// Experimental code
		foreach(ParameterModel parameter in model.Parameters)
		{
			if (parameter.Value == null || !ReferenceExpressionUtil.IsReference(parameter.Value))
			{
				continue;
			}

			String newKey = ReferenceExpressionUtil.ExtractComponentKey(parameter.Value);

			// Update dependencies to ServiceOverride			
			model.Dependencies.Add(new DependencyModel(DependencyType.ServiceOverride, newKey, null, false));
		}
	}
}
```

8. `LifestyleModelInspector`:组件模型的中的生命周期已经自定义生命周期Handler的补充,当有`model.Configuration.Attributes["lifestyle"]`有的时候
取自配置,当没有的时候则取自 `Implementation`的`LifestyleAttribute`标记

```
public class LifestyleModelInspector : IContributeComponentModelConstruction
{
	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (!ReadLifestyleFromConfiguration(model))
		{
			ReadLifestyleFromType(model);
		}
	}

	protected virtual bool ReadLifestyleFromConfiguration(ComponentModel model)
	{
		if (model.Configuration != null)
		{
			String lifestyle = model.Configuration.Attributes["lifestyle"];

			if (lifestyle != null)
			{
				try
				{
					LifestyleType type = (LifestyleType)
						Enum.Parse(typeof(LifestyleType), lifestyle, true);

					model.LifestyleType = type;

				}
				catch(Exception)
				{
					String message = String.Format(
						"Could not convert the specified attribute value " +
						"{0} to a valid LifestyleType enum type", lifestyle);

					throw new ConfigurationErrorsException(message);
				}

				if (model.LifestyleType == LifestyleType.Pooled)
				{
					ExtractPoolConfig(model);
				}
				else if(model.LifestyleType == LifestyleType.Custom)
				{
					ExtractCustomConfig(model);
				}


				return true;
			}
		}

		return false;
	}

	private static void ExtractPoolConfig(ComponentModel model)
	{
		String initial = model.Configuration.Attributes["initialPoolSize"];
		String maxSize = model.Configuration.Attributes["maxPoolSize"];

		if (initial != null)
		{
			model.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize] = Convert.ToInt32(initial);
		}
		if (maxSize != null)
		{
			model.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize] = Convert.ToInt32(maxSize);
		}
	}

	private static void ExtractCustomConfig(ComponentModel model)
	{
		String customLifestyleType = model.Configuration.Attributes["customLifestyleType"];

		if(customLifestyleType != null)
		{
			try
			{
				model.CustomLifestyle = Type.GetType(customLifestyleType, true, false);
			}
			catch(Exception)
			{
				String message = String.Format(
					"The Type {0} specified  in the customLifestyleType attribute could not be loaded.", customLifestyleType);

				throw new ConfigurationErrorsException(message);
			}
		}
		else
		{
			const string message = @"The attribute 'customLifestyleType' must be specified in conjunction with the 'lifestyle' attribute set to ""custom"".";

			throw new ConfigurationErrorsException(message);
		}
	}

	protected virtual void ReadLifestyleFromType(ComponentModel model)
	{
		object[] attributes = model.Implementation.GetCustomAttributes(
			typeof(LifestyleAttribute), true );

		if (attributes.Length != 0)
		{
			LifestyleAttribute attribute = (LifestyleAttribute)
				attributes[0];

			model.LifestyleType = attribute.Lifestyle;

			if (model.LifestyleType == LifestyleType.Custom)
			{
				CustomLifestyleAttribute custom = (CustomLifestyleAttribute)
					attribute;
				model.CustomLifestyle = custom.LifestyleHandlerType;
			}
			else if (model.LifestyleType == LifestyleType.Pooled)
			{
				PooledAttribute pooled = (PooledAttribute) attribute;
				model.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize] = pooled.InitialPoolSize;
				model.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize] = pooled.MaxPoolSize;
			}
		}
	}
}
```

9. `ConstructorDependenciesModelInspector`:组件模型的构造委托补充,依赖于Kernel的SubSystem中的[DefaultConversionManager](DefaultConversionManager.md)

```
public class ConstructorDependenciesModelInspector : IContributeComponentModelConstruction
{
	[NonSerialized]
	private IConversionManager converter;

	public ConstructorDependenciesModelInspector()
	{
	}

	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (converter == null)
		{
			converter = (IConversionManager)
			            kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
		}

		Type targetType = model.Implementation;

		ConstructorInfo[] constructors =
			targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

		foreach(ConstructorInfo constructor in constructors)
		{
			// We register each public constructor
			// and let the ComponentFactory select an
			// eligible amongst the candidates later
			model.Constructors.Add(CreateConstructorCandidate(model, constructor));
		}
	}

	protected virtual ConstructorCandidate CreateConstructorCandidate(ComponentModel model, ConstructorInfo constructor)
	{
		ParameterInfo[] parameters = constructor.GetParameters();

		DependencyModel[] dependencies = new DependencyModel[parameters.Length];

		for(int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameter = parameters[i];

			Type paramType = parameter.ParameterType;

			// This approach is somewhat problematic. We should use
			// another strategy to differentiate types and classify dependencies
			if (converter.IsSupportedAndPrimitiveType(paramType))
			{
				dependencies[i] = new DependencyModel(
					DependencyType.Parameter, parameter.Name, paramType, false);
			}
			else
			{
				ParameterModel modelParameter = model.Parameters[parameter.Name];

				if (modelParameter != null && ReferenceExpressionUtil.IsReference(modelParameter.Value))
				{
					String key = ReferenceExpressionUtil.ExtractComponentKey(modelParameter.Value);

					dependencies[i] = new DependencyModel(
						DependencyType.ServiceOverride, key, paramType, false);					
				}
				else
				{
					dependencies[i] = new DependencyModel(
						DependencyType.Service, parameter.Name, paramType, false);
				}
			}
		}

		return new ConstructorCandidate(constructor, dependencies);
	}
}
```

10. `PropertiesDependenciesModelInspector` : 组件模型中的属性依赖补充,主要看Model.InspectionBehavior的值

```
public class PropertiesDependenciesModelInspector : IContributeComponentModelConstruction
{
	[NonSerialized]
	private IConversionManager converter;
	public PropertiesDependenciesModelInspector()
	{
	}

	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (converter == null)
		{
			converter = (IConversionManager)
				kernel.GetSubSystem( SubSystemConstants.ConversionManagerKey );
		}

		InspectProperties(model);
	}

	protected virtual void InspectProperties(ComponentModel model)
	{
		if (model.InspectionBehavior == PropertiesInspectionBehavior.Undefined)
		{
			model.InspectionBehavior = GetInspectionBehaviorFromTheConfiguration(model.Configuration);
		}

		if (model.InspectionBehavior == PropertiesInspectionBehavior.None)
		{
			// Nothing to be inspected
			return;
		}

		BindingFlags bindingFlags;

		if (model.InspectionBehavior == PropertiesInspectionBehavior.DeclaredOnly)
		{
			bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		}
		else // if (model.InspectionBehavior == PropertiesInspectionBehavior.All) or Undefined
		{
			bindingFlags = BindingFlags.Public | BindingFlags.Instance;
		}

		Type targetType = model.Implementation;

		PropertyInfo[] properties = targetType.GetProperties(bindingFlags);

		foreach(PropertyInfo property in properties)
		{
			if (!property.CanWrite || property.GetSetMethod() == null)
			{
				continue;
			}

			ParameterInfo[] indexerParams = property.GetIndexParameters();

			if (indexerParams != null && indexerParams.Length != 0)
			{
				continue;
			}

			if (property.IsDefined(typeof(DoNotWireAttribute), true))
			{
				continue;
			}

			DependencyModel dependency;

			Type propertyType = property.PropertyType;

			// All these dependencies are simple guesses
			// So we make them optional (the 'true' parameter below)

			if ( converter.IsSupportedAndPrimitiveType(propertyType) )
			{
				dependency = new DependencyModel(DependencyType.Parameter, property.Name, propertyType, true);
			}
			else if (propertyType.IsInterface || propertyType.IsClass)
			{
				dependency = new DependencyModel(DependencyType.Service, property.Name, propertyType, true);
			}
			else
			{
				// What is it?!
				// Awkward type, probably.

				continue;
			}

			model.Properties.Add( new PropertySet(property, dependency) );
		}
	}

	private PropertiesInspectionBehavior GetInspectionBehaviorFromTheConfiguration(IConfiguration config)
	{
		if (config == null || config.Attributes["inspectionBehavior"] == null)
		{
			// return default behavior
			return PropertiesInspectionBehavior.All;
		}

		String enumStringVal = config.Attributes["inspectionBehavior"];

		try
		{
			return (PropertiesInspectionBehavior)
				Enum.Parse(typeof(PropertiesInspectionBehavior), enumStringVal, true);
		}
		catch(Exception)
		{
			String[] enumNames = Enum.GetNames(typeof(PropertiesInspectionBehavior));

			String message = String.Format("Error on properties inspection. " +
				"Could not convert the inspectionBehavior attribute value into an expected enum value. " +
				"Value found is '{0}' while possible values are '{1}'",
					enumStringVal, String.Join(",", enumNames));

			throw new KernelException(message);
		}
	}
}
```

11. `LifecycleModelInspector`: 组件模型中对生命周期阶段的补充,`LifecycleSteps` 属性

```
public class LifecycleModelInspector : IContributeComponentModelConstruction
{
	public LifecycleModelInspector()
	{
	}

	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (model == null)
		{
			throw new ArgumentNullException("model");
		}
		if (typeof (IInitializable).IsAssignableFrom(model.Implementation))
		{
			model.LifecycleSteps.Add( LifecycleStepType.Commission, InitializationConcern.Instance );
		}
		if (typeof (ISupportInitialize).IsAssignableFrom(model.Implementation))
		{
			model.LifecycleSteps.Add( LifecycleStepType.Commission, SupportInitializeConcern.Instance );
		}
		if (typeof (IDisposable).IsAssignableFrom(model.Implementation))
		{
			model.LifecycleSteps.Add( LifecycleStepType.Decommission, DisposalConcern.Instance );
		}
	}
}
```

12. `InterceptorInspector`:组件模型上的`Interceptors`转换为模型上的`Dependencies`的补充

```
public class InterceptorInspector : IContributeComponentModelConstruction
{
	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		CollectFromAttributes(model);
		CollectFromConfiguration(model);
	}

	protected virtual void CollectFromConfiguration(ComponentModel model)
	{
		if (model.Configuration == null) return;

		IConfiguration interceptors = model.Configuration.Children["interceptors"];

		if (interceptors == null) return;

		foreach(IConfiguration interceptor in interceptors.Children)
		{
			String value = interceptor.Value;

			if (!ReferenceExpressionUtil.IsReference(value))
			{
				String message = String.Format(
					"The value for the interceptor must be a reference " +
					"to a component (Currently {0})",
					value);

				throw new ConfigurationErrorsException(message);
			}

			InterceptorReference interceptorRef =
				new InterceptorReference( ReferenceExpressionUtil.ExtractComponentKey(value) );

			model.Interceptors.Add(interceptorRef);
			model.Dependencies.Add( CreateDependencyModel(interceptorRef) );
		}
	}

	protected virtual void CollectFromAttributes(ComponentModel model)
	{
		if (!model.Implementation.IsDefined( typeof(InterceptorAttribute), true ))
		{
			return;
		}

		object[] attributes = model.Implementation.GetCustomAttributes(true);

		foreach(object attribute in attributes)
		{
			if (attribute is InterceptorAttribute)
			{
				InterceptorAttribute attr = (attribute as InterceptorAttribute);

				AddInterceptor(
					attr.Interceptor,
					model.Interceptors );

				model.Dependencies.Add(
					CreateDependencyModel(attr.Interceptor) );
			}
		}
	}

	protected DependencyModel CreateDependencyModel(InterceptorReference interceptor)
	{
		return new DependencyModel(DependencyType.Service, interceptor.ComponentKey,
			interceptor.ServiceType, false);
	}

	protected void AddInterceptor(InterceptorReference interceptorRef, InterceptorReferenceCollection interceptors)
	{
		interceptors.Add( interceptorRef );
	}
}
```

13. `ComponentActivatorInspector`: 组件模型上的 `CustomComponentActivator`自定义激活器的补充

```
public class ComponentActivatorInspector : IContributeComponentModelConstruction
{
	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		if (!ReadComponentActivatorFromConfiguration(model))
		{
			ReadComponentActivatorFromType(model);
		}
	}

	protected virtual bool ReadComponentActivatorFromConfiguration(ComponentModel model)
	{
		if (model.Configuration != null)
		{
			string componentActivatorType = model.Configuration.Attributes["componentActivatorType"];

			if (componentActivatorType == null)
			{
				return false;
			}

			try
			{
				Type customComponentActivator = Type.GetType(componentActivatorType, true, false);

				ValidateComponentActivator(customComponentActivator);

				model.CustomComponentActivator = customComponentActivator;
			}
			catch(Exception ex)
			{
				string message =
					String.Format("The Type '{0}' specified  in the componentActivatorType attribute could not be loaded.",
					              componentActivatorType);

				throw new ConfigurationErrorsException(message, ex);
			}
		}

		return false;
	}

	protected virtual void ReadComponentActivatorFromType(ComponentModel model)
	{
		object[] attributes = model.Implementation.GetCustomAttributes(typeof(ComponentActivatorAttribute), true);

		if (attributes.Length != 0)
		{
			ComponentActivatorAttribute attribute = (ComponentActivatorAttribute) attributes[0];

			ValidateComponentActivator(attribute.ComponentActivatorType);

			model.CustomComponentActivator = attribute.ComponentActivatorType;
		}
	}

	protected virtual void ValidateComponentActivator(Type customComponentActivator)
	{
		if (!typeof(IComponentActivator).IsAssignableFrom(customComponentActivator))
		{
			string message =
				String.Format(
					"The Type '{0}' specified  in the componentActivatorType attribute must implement Castle.MicroKernel.IComponentActivator",
					customComponentActivator.FullName);
			throw new InvalidOperationException(message);
		}
	}
}
```

14. `ComponentProxyInspector`: 组件模型上的 `ExtendedProperties["proxy.options"]`的数据补充

```
public class ComponentProxyInspector : IContributeComponentModelConstruction
{
	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		ReadProxyBehavior(kernel, model);
	}

	protected virtual void ReadProxyBehavior(IKernel kernel, ComponentModel model)
	{
		ComponentProxyBehaviorAttribute proxyBehaviorAtt = GetProxyBehaviorFromType(model.Implementation);

		if (proxyBehaviorAtt == null)
		{
			proxyBehaviorAtt = new ComponentProxyBehaviorAttribute();
		}

		string useSingleInterfaceProxyAttrib = model.Configuration != null ? model.Configuration.Attributes["useSingleInterfaceProxy"] : null;
		string marshalByRefProxyAttrib = model.Configuration != null ? model.Configuration.Attributes["marshalByRefProxy"] : null;

		ITypeConverter converter = (ITypeConverter)kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);

		if (useSingleInterfaceProxyAttrib != null)
		{
			try
			{
				proxyBehaviorAtt.UseSingleInterfaceProxy = (bool)
					converter.PerformConversion(useSingleInterfaceProxyAttrib, typeof(bool));
			}
			catch(ConverterException ex)
			{
				throw new ConfigurationErrorsException("Could not convert attribute " +
					"'useSingleInterfaceProxy' to bool. Value is " + useSingleInterfaceProxyAttrib, ex);
			}
		}

		if (marshalByRefProxyAttrib != null)
		{
			try
			{
				proxyBehaviorAtt.UseMarshalByRefProxy = (bool)
					converter.PerformConversion(marshalByRefProxyAttrib, typeof(bool));
			}
			catch(ConverterException ex)
			{
				throw new ConfigurationErrorsException("Could not convert attribute " +
					"'marshalByRefProxy' to bool. Value is " + marshalByRefProxyAttrib, ex);
			}
		}

		ApplyProxyBehavior(proxyBehaviorAtt, model);
	}

	protected virtual ComponentProxyBehaviorAttribute GetProxyBehaviorFromType(Type implementation)
	{
		object[] attributes = implementation.GetCustomAttributes(
			typeof(ComponentProxyBehaviorAttribute), true);

		if (attributes.Length != 0)
		{
			return (ComponentProxyBehaviorAttribute) attributes[0];
		}

		return null;
	}

	private static void ApplyProxyBehavior(ComponentProxyBehaviorAttribute behavior, ComponentModel model)
	{
		if (behavior.UseSingleInterfaceProxy || behavior.UseMarshalByRefProxy)
		{
			EnsureComponentRegisteredWithInterface(model);
		}

		ProxyOptions options = ProxyUtil.ObtainProxyOptions(model, true);

		options.UseSingleInterfaceProxy = behavior.UseSingleInterfaceProxy;
		options.UseMarshalByRefAsBaseClass = behavior.UseMarshalByRefProxy;
		options.AddAdditionalInterfaces(behavior.AdditionalInterfaces);
	}

	private static void EnsureComponentRegisteredWithInterface(ComponentModel model)
	{
		if (!model.Service.IsInterface)
		{
			String message = String.Format("The class {0} requested a single interface proxy, " +
			                               "however the service {1} does not represent an interface",
			                               model.Implementation.FullName, model.Service.FullName);

			throw new ComponentRegistrationException(message);
		}
	}
}
```
