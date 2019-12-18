### Kernel 中的`SubSystem`之`DefaultConfigurationStore`

```
public class DefaultConfigurationStore : AbstractSubSystem, IConfigurationStore
{
    //关于使用HybridDictionary
    //在集合较小时，使用 System.Collections.Specialized.ListDictionary 来实现 IDictionary，
    //然后当集合变大时，切换到System.Collections.Hashtable
	private readonly IDictionary childContainers = new HybridDictionary();
	private readonly IDictionary facilities = new HybridDictionary();
	private readonly IDictionary components = new HybridDictionary();
	private readonly IDictionary bootstrapcomponents = new HybridDictionary();
    //为了提高速度,IConfiguration存两份,一份通过Key来查询,一份直接返回集合
	private readonly ArrayList childContainersList = new ArrayList();
	private readonly ArrayList facilitiesList = new ArrayList();
	private readonly ArrayList componentsList = new ArrayList();
	private readonly ArrayList bootstrapComponentsList = new ArrayList();

	public DefaultConfigurationStore()
	{
	}

	[MethodImpl(MethodImplOptions.Synchronized)] //这里相当于lock(this)锁定改对象，让其进行线程同步
	public void AddFacilityConfiguration(String key, IConfiguration config)
	{
		facilitiesList.Add(config);

		facilities[key] = config;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void AddComponentConfiguration(String key, IConfiguration config)
	{
		componentsList.Add(config);

		components[key] = config;
	}

	/// <summary>
	/// Associates a configuration node with a bootstrap component key
	/// </summary>
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void AddBootstrapComponentConfiguration(string key, IConfiguration config)
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void AddChildContainerConfiguration(String key, IConfiguration config)
	{
		childContainersList.Add(config);
		childContainers[key] = config;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration GetFacilityConfiguration(String key)
	{
		return facilities[key] as IConfiguration;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration GetChildContainerConfiguration(String key)
	{
		return childContainers[key] as IConfiguration;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration GetComponentConfiguration(String key)
	{
	    return components[key] as IConfiguration;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration GetBootstrapComponentConfiguration(string key)
	{
		return bootstrapcomponents[key] as IConfiguration;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration[] GetFacilities()
	{
		return (IConfiguration[]) facilitiesList.ToArray(typeof(IConfiguration));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration[] GetBootstrapComponents()
	{
		return (IConfiguration[]) bootstrapComponentsList.ToArray(typeof(IConfiguration));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration[] GetConfigurationForChildContainers()
	{
		return (IConfiguration[]) childContainersList.ToArray(typeof(IConfiguration));
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public IConfiguration[] GetComponents()
	{
		return (IConfiguration[]) componentsList.ToArray(typeof(IConfiguration));
	}

	public IResource GetResource(String resourceUri, IResource resource)
	{
		if (resourceUri.IndexOf(Uri.SchemeDelimiter) == -1)
		{
			return resource.CreateRelative(resourceUri);
		}

		IResourceSubSystem subSystem = (IResourceSubSystem)
			Kernel.GetSubSystem(SubSystemConstants.ResourceKey);

		return subSystem.CreateResource(resourceUri, resource.FileBasePath);
	}
}
```

### `Configuration`底层的数据结构

```
//配置接口
public interface IConfiguration
{
	String Name { get; }
	String Value { get; }
	ConfigurationCollection Children { get; }
	ConfigurationAttributeCollection Attributes { get; }
	object GetValue(Type type, object defaultValue);
}
//抽象配置接口
public abstract class AbstractConfiguration : IConfiguration
{
	private readonly ConfigurationAttributeCollection attributes = new ConfigurationAttributeCollection();
	private readonly ConfigurationCollection children = new ConfigurationCollection();
	private string internalName;
	private string internalValue;
	public string Name
	{
		get { return internalName; }
		protected set { internalName = value; }
	}

	public string Value
	{
		get { return internalValue; }
		protected set { internalValue = value; }
	}

	public virtual ConfigurationCollection Children
	{
		get { return children; }
	}

	public virtual ConfigurationAttributeCollection Attributes
	{
		get { return attributes; }
	}

	public virtual object GetValue(Type type, object defaultValue)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}

		try
		{
			return Convert.ChangeType(Value, type, System.Threading.Thread.CurrentThread.CurrentCulture);
		}
		catch(InvalidCastException)
		{
			return defaultValue;
		}
	}
}
//配置集合
public class ConfigurationCollection : List<IConfiguration>
{
	public ConfigurationCollection()
	{
	}
	public ConfigurationCollection(IEnumerable<IConfiguration> value) : base(value)
	{
	}

	public IConfiguration this[String name]
	{
		get
		{
			foreach(IConfiguration config in this)
			{
				if (name.Equals(config.Name))
				{
					return config;
				}
			}

			return null;
		}
	}
}
//属性的配置集合
public class ConfigurationAttributeCollection : NameValueCollection
{
    public ConfigurationAttributeCollection()
    {
    }

    protected ConfigurationAttributeCollection(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
```
落实到Kernel上的配置实例
```
public class MutableConfiguration : AbstractConfiguration
{
	public MutableConfiguration(String name) : this(name, null)
	{
	}

	public MutableConfiguration(String name, String value)
	{
		Name = name;
		Value = value;
	}

	public new string Value
	{
		get { return base.Value; }
		set { base.Value = value; }
	}

	public static MutableConfiguration Create(string name)
	{
		return new MutableConfiguration(name);
	}

	public MutableConfiguration Attribute(string name, string value)
	{
		Attributes[name] = value;
		return this;
	}

	public MutableConfiguration CreateChild(string name)
	{
		MutableConfiguration child = new MutableConfiguration(name);
		Children.Add(child);
		return child;
	}

	public MutableConfiguration CreateChild(string name, string value)
	{
		MutableConfiguration child = new MutableConfiguration(name, value);
		Children.Add(child);
		return child;
	}
}
```
