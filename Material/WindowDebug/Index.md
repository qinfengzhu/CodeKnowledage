#### Window Debug 调试

1. 打开 `WinDbg(x64)`, [Windebug工具下载地址](http://www.windbg.org/) ，[网上内存WinDebug分析文章](https://www.cnblogs.com/tianqing/p/7630636.html)

2. `Ctrl+D` 打开 dump的文件(通过任务管理器，右键进行`创建转储文件`)

3. 等待加载完毕，出现  `xxxx   ret`; 然后执行 `.loadby sos clr`,这里的 `SOS.dll` 与 `clr.dll` 一般在`C:\Windows\Microsoft.NET\Framework\v4.0.30319` 目录下面

如果命令成功，那么测试环境好了。如果出现了“Unable to find module 'clr'”这样的错误。请键入g让调试程序运行一会儿，停下来的时候再尝试命令.loadby sos clr，这时一般都会成功。

如果上述方法仍旧不能成功，需要使用sxe命令。执行顺序如下:

```
0:000> sxe ld.clrjit
0:000> g
...
0:000> .loadby sos clr
```

或者 根本错了，要找到 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319`

```
0:000> .load C:\Windows\Microsoft.NET\Framework64\v4.0.30319\SOS.dll
0:000> .load C:\Windows\Microsoft.NET\Framework64\v4.0.30319\clr.dll
```
