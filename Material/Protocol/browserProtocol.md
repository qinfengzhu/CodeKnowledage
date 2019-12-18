### js是如何调用本地的安装程序

1. `Html Code` 部分

```
//协议为customproto 参数为1234
//标准的协议格式 rdm://<action>?<parameter1>=<value>[&<parameter2>=<value>]
<a href="customproto:1234">调用本地程序</a>
```

2. `本地注册表中的骨架` 部分

```
HKEY_CURRENT_USER\Software\Classes
   customproto
      (Default) = "URL:CustomProto Protocol"
      URL Protocol = ""
      DefaultIcon
         (Default) = "customprogram.exe,1"
      shell
         open
            command
               (Default) = "C:\Program Files\customprogram.exe" "%1"
```

3. `注册表的骨架` 部分,`customproto.reg`

```
Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Classes\customproto]
"URL Protocol"="\"\""
@="\"URL:CustomProto Protocol\""

[HKEY_CURRENT_USER\Software\Classes\customproto\DefaultIcon]
@="\"customprogram.exe,1\""

[HKEY_CURRENT_USER\Software\Classes\customproto\shell]

[HKEY_CURRENT_USER\Software\Classes\customproto\shell\open]

[HKEY_CURRENT_USER\Software\Classes\customproto\shell\open\command]
@="\"C:\\Program Files\\customprogram.exe\" \"%1\""
```

4. `本地程序部分`

```
using System;
using System.Collections.Generic;
using System.Text;

namespace customprogram
{
  class CustomProgramProtocol
  {
    static string ProcessInput(string s)
    {
       //这里是业务处理
       return s;
    }

    static void Main(string[] args)
    {
      Console.WriteLine("程序开始执行: \n\t" + Environment.CommandLine);
      Console.WriteLine("\n\n 参数:\n");

      foreach (string s in args)
      {
        Console.WriteLine("\t" + ProcessInput(s));
      }

      Console.WriteLine("\n 按任何键进行继续...");
      Console.ReadKey();
    }
  }
}
```
