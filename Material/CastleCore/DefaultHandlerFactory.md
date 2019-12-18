### DefaultHandlerFactory 构建Handler

1. `IHandlerFactory` 接口定义

```
public interface IHandlerFactory
{
    //根据ComponentModel构建IHandler
    IHandler Create(ComponentModel model);

    IHandler CreateForwarding(IHandler target, Type forwardedType);
}
```
