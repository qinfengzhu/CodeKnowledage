### MVC执行流程之Action Parameter绑定

1. Model绑定从 `ControllerBase` 下的 `ValueProvider` 说起

```
public static class ValueProviderFactories {

    private static readonly ValueProviderFactoryCollection _factories = new ValueProviderFactoryCollection() {
        new ChildActionValueProviderFactory(),//Html.Partial 子视图
        new FormValueProviderFactory(),//Form表单值提供
        new JsonValueProviderFactory(),//Json值提供
        new RouteDataValueProviderFactory(),//路由值提供
        new QueryStringValueProviderFactory(),//Url值提供
        new HttpFileCollectionValueProviderFactory(),//传输文件值提供
    };

    public static ValueProviderFactoryCollection Factories {
        get {
            return _factories;
        }
    }

}
```
2. Filter 提供源 `FilterProviders`

```
public static class FilterProviders {
    static FilterProviders() {
        Providers = new FilterProviderCollection();
        Providers.Add(GlobalFilters.Filters); //全局过滤器
        Providers.Add(new FilterAttributeFilterProvider());//属性过滤器
        Providers.Add(new ControllerInstanceFilterProvider());//
    }

    public static FilterProviderCollection Providers {
        get;
        private set;
    }
}
```

3. 参数使用开始点 `ControllerActionInvoker` 中 `InvokeAction(ControllerContext controllerContext,string actionName)`

```
//获取参数字典
IDictionary<string, object> parameters = GetParameterValues(controllerContext, actionDescriptor);
```

下一步观看 `GetParameterValues` 方法
```
//ControllerActionInvoker,actionDescriptor的类型为ReflectedActionDescriptor
protected virtual IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
    Dictionary<string, object> parametersDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    ParameterDescriptor[] parameterDescriptors = actionDescriptor.GetParameters();

    foreach (ParameterDescriptor parameterDescriptor in parameterDescriptors) {
        parametersDict[parameterDescriptor.ParameterName] = GetParameterValue(controllerContext, parameterDescriptor);
    }
    return parametersDict;
}
```
下一步观看 `ReflectedActionDescriptor` 的 `GetParameters`

```
public override ParameterDescriptor[] GetParameters() {
    ParameterDescriptor[] parameters = LazilyFetchParametersCollection();
    // need to clone array so that user modifications aren't accidentally stored
    return (ParameterDescriptor[])parameters.Clone();
}
private ParameterDescriptor[] LazilyFetchParametersCollection() {
    return DescriptorUtil.LazilyFetchOrCreateDescriptors<ParameterInfo, ParameterDescriptor>(
        ref _parametersCache /* cacheLocation */,
        MethodInfo.GetParameters /* initializer */,
        parameterInfo => new ReflectedParameterDescriptor(parameterInfo, this) /* converter */);
}
```

到这里，我们就可以看到最终的参数解析归 `ReflectedParameterDescriptor`，下一步看 `ReflectedParameterDescriptor`

```
//构造函数
public ReflectedParameterDescriptor(ParameterInfo parameterInfo, ActionDescriptor actionDescriptor) {
    if (parameterInfo == null) {
        throw new ArgumentNullException("parameterInfo");
    }
    if (actionDescriptor == null) {
        throw new ArgumentNullException("actionDescriptor");
    }

    ParameterInfo = parameterInfo;
    _actionDescriptor = actionDescriptor;
    _bindingInfo = new ReflectedParameterBindingInfo(parameterInfo);
}
//默认值  DefaultValueAttribute,参数前方加Attribute标记
public override object DefaultValue {
    get {
        object value;
        if (ParameterInfoUtil.TryGetDefaultValue(ParameterInfo, out value)) {
            return value;
        }
        else {
            return base.DefaultValue;
        }
    }
}
```

下一步看 `ReflectedParameterBindingInfo`,反射参数绑定信息

```
//获取自定义的ModelBinder
public override IModelBinder Binder {
    get {
        IModelBinder binder = ModelBinders.GetBinderFromAttributes(_parameterInfo,
            () => String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedParameterBindingInfo_MultipleConverterAttributes,
                _parameterInfo.Name, _parameterInfo.Member));

        return binder;
    }
}
//ModelBinder 中
internal static IModelBinder GetBinderFromAttributes(ICustomAttributeProvider element, Func<string> errorMessageAccessor) {
    CustomModelBinderAttribute[] attrs = (CustomModelBinderAttribute[])element.GetCustomAttributes(typeof(CustomModelBinderAttribute), true /* inherit */);
    return GetBinderFromAttributesImpl(attrs, errorMessageAccessor);
}
private static IModelBinder GetBinderFromAttributesImpl(CustomModelBinderAttribute[] attrs, Func<string> errorMessageAccessor) {
    // this method is used to get a custom binder based on the attributes of the element passed to it.
    // it will return null if a binder cannot be detected based on the attributes alone.

    if (attrs == null) {
        return null;
    }

    switch (attrs.Length) {
        case 0:
            return null;

        case 1:
            IModelBinder binder = attrs[0].GetBinder();
            return binder;

        default:
            string errorMessage = errorMessageAccessor();
            throw new InvalidOperationException(errorMessage);
    }
}
```
回到 `ControllerActionInvoker` 中的 `GetParameterValues`
```
protected virtual IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
    Dictionary<string, object> parametersDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    //获取参数的描述,带有自定义的ModelBinders
    ParameterDescriptor[] parameterDescriptors = actionDescriptor.GetParameters();

    foreach (ParameterDescriptor parameterDescriptor in parameterDescriptors) {
        //根据ModelBinder获取值,如果没有自定义匹配的ModelBinder就采用DefaultModelBinder
        parametersDict[parameterDescriptor.ParameterName] = GetParameterValue(controllerContext, parameterDescriptor);
    }
    return parametersDict;
}

//获取正确的ModelBinder，并且对参数进行解析赋值
protected virtual object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor) {
    // collect all of the necessary binding properties
    Type parameterType = parameterDescriptor.ParameterType;
    //这里是获取正确的ModelBinder的地方
    IModelBinder binder = GetModelBinder(parameterDescriptor);
    IValueProvider valueProvider = controllerContext.Controller.ValueProvider;
    string parameterName = parameterDescriptor.BindingInfo.Prefix ?? parameterDescriptor.ParameterName;
    Predicate<string> propertyFilter = GetPropertyFilter(parameterDescriptor);

    // finally, call into the binder
    ModelBindingContext bindingContext = new ModelBindingContext() {
        FallbackToEmptyPrefix = (parameterDescriptor.BindingInfo.Prefix == null), // only fall back if prefix not specified
        ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, parameterType),
        ModelName = parameterName,
        ModelState = controllerContext.Controller.ViewData.ModelState,
        PropertyFilter = propertyFilter,
        ValueProvider = valueProvider
    };

    object result = binder.BindModel(controllerContext, bindingContext);
    return result ?? parameterDescriptor.DefaultValue;
}
//取ModelBinder的方法
private IModelBinder GetModelBinder(ParameterDescriptor parameterDescriptor) {
    // look on the parameter itself, then look in the global table
    //这里的Binders 即为 ModelBinders.Binders
    return parameterDescriptor.BindingInfo.Binder ?? Binders.GetBinder(parameterDescriptor.ParameterType);
}
```

下看 `ModelBinderDictionary` 中 `GetBinder`

```

public IModelBinder GetBinder(Type modelType) {
    return GetBinder(modelType, true /* fallbackToDefault */);
}

public virtual IModelBinder GetBinder(Type modelType, bool fallbackToDefault) {
    if (modelType == null) {
        throw new ArgumentNullException("modelType");
    }
    //关注DefaultBinder，即为DefaultModelBinder
    return GetBinder(modelType, (fallbackToDefault) ? DefaultBinder : null);
}
//Binder 的查找顺序
private IModelBinder GetBinder(Type modelType, IModelBinder fallbackBinder) {

    // Try to look up a binder for this type. We use this order of precedence:
    // 1. Binder returned from provider
    // 2. Binder registered in the global table
    // 3. Binder attribute defined on the type
    // 4. Supplied fallback binder

    IModelBinder binder = _modelBinderProviders.GetBinder(modelType);
    if (binder != null) {
        return binder;
    }

    if (_innerDictionary.TryGetValue(modelType, out binder)) {
        return binder;
    }

    binder = ModelBinders.GetBinderFromAttributes(modelType,
        () => String.Format(CultureInfo.CurrentCulture, MvcResources.ModelBinderDictionary_MultipleAttributes, modelType.FullName));

    return binder ?? fallbackBinder;
}
```
好了，到这里基本就找到了 `ModelBinder`,根据`ModelBindingContext` 来进行 `BindModel`

```
//ModelBindingContext 包含哪些东西
ModelBindingContext bindingContext = new ModelBindingContext() {
    FallbackToEmptyPrefix = (parameterDescriptor.BindingInfo.Prefix == null), // only fall back if prefix not specified
    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, parameterType),
    ModelName = parameterName,
    ModelState = controllerContext.Controller.ViewData.ModelState,
    PropertyFilter = propertyFilter,
    ValueProvider = valueProvider //值提供源 又与 ValueProviderFactories 关联
};
//ValueProviderFactories 提供了
public static class ValueProviderFactories {
    private static readonly ValueProviderFactoryCollection _factories = new ValueProviderFactoryCollection() {
        new ChildActionValueProviderFactory(),//Html.Partial 子视图
        new FormValueProviderFactory(),//Form表单值提供
        new JsonValueProviderFactory(),//Json值提供
        new RouteDataValueProviderFactory(),//路由值提供
        new QueryStringValueProviderFactory(),//Url值提供
        new HttpFileCollectionValueProviderFactory(),//传输文件值提供
    };

    public static ValueProviderFactoryCollection Factories {
        get {
            return _factories;
        }
    }
}
//ValueProviderFactories 转换为 ValueProviderCollection
public IValueProvider GetValueProvider(ControllerContext controllerContext) {
    var valueProviders = from factory in _serviceResolver.Current
                         let valueProvider = factory.GetValueProvider(controllerContext)
                         where valueProvider != null
                         select valueProvider;

    return new ValueProviderCollection(valueProviders.ToList());
}
```
