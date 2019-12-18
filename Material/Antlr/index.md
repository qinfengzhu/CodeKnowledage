### Antlr使用说明

1. 安装

1.1 Java 环境安装
1.2 下载[antlr的jar包](https://www.antlr.org/download/antlr-4.7.2-complete.jar)
1.3 配置 CLassPath `.;C:\Javalib\antlr4-complete.jar;D:\Program Files\Java\jdk1.8.0_172\lib\tools.jar;D:\Program Files\Java\jdk1.8.0_172\lib\dt.jar`
1.4 把下面的`antlr4.bat` 与 `grun.bat` 所在的文件夹加入到 `Path` 环境变量中

antlr4.bat 内容
```
java org.antlr.v4.Tool %*
```

grun.bat 内容
```
java org.antlr.v4.gui.TestRig %*
```

### 关于C#中使用

1. 安装 Visual Studio 的扩展插件工具,工具->扩展与更新->联机  `ANTLR Language Support`

2. 新建项目，Nuget 引入包 `ANTLR4` 与 `ANTLR4 Runtime`

3. 添加新项 `ANTLR4 Combined Grammar` ,取名为 `ArrayInit.g4`

4. `ArrayInit.g4` 内容为

```
grammar ArrayInit;

init : '{' value (',' value)* '}' ;

value : init
      | INT
      ;

INT : [0-9]+ ;
WS  : [ \t\r\n]+ -> skip;
```

5. 对生成`Csharp`的代码，进行测试

```
[TestMethod]
public void ArrayInitTreeTest()
{
    //新建一个CharStream,从标准输入读取数据
    AntlrInputStream input = new AntlrInputStream("{1,{2,3},4}");

    //新建一个词法分析器,处理输入的CharStream
    ArrayInitLexer lexer = new ArrayInitLexer(input);

    //新建一个词法符号的缓冲区,用于存储词法分析器将生成的词法符号
    CommonTokenStream tokens = new CommonTokenStream(lexer);

    //新建一个语法分析器,处理词法符号缓冲区中的内容
    ArrayInitParser parser = new ArrayInitParser(tokens);

    IParseTree tree = parser.init();

    string treeStr = tree.ToStringTree();

    Assert.AreEqual(1, 1);
}
```
