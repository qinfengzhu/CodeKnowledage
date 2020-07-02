#### Log4J 的使用

> 简要介绍
Log4J 是 Apache 的一个开源项目（官网 http://jakarta.apache.org/log4j），通过在项目中使用 Log4J，我们可以控制日志信息输出到控制台、文件、GUI 组件、甚至是数据库中。我们可以控制每一条日志的输出格式，通过定义日志的输出级别，可以更灵活的控制日志的输出过程。方便项目的调试。


> Log4J的组成部分

Log4J 主要由 Loggers (日志记录器)、Appenders（输出端）和 Layout（日志格式化器）组成。其中 Loggers 控制日志的输出级别与日志是否输出；Appenders 指定日志的输出方式（输出到控制台、文件等）；Layout 控制日志信息的输出格式。

> 日志级别

Log4J 在 org.apache.log4j.Level 类中定义了OFF、FATAL、ERROR、WARN、INFO、DEBUG、ALL七种日志级别：

| 日志级别 | 描述                                           |
| -------- | ---------------------------------------------- |
| OFF      | 最高日志级别，关闭左右日志                     |
| FATAL    | 将会导致应用程序退出的错误                     |
| ERROR    | 发生错误事件，但仍不影响系统的继续运行         |
| WARN     | 警告，即潜在的错误情形                         |
| INFO     | 一般和在粗粒度级别上，强调应用程序的运行全程   |
| DEBUG    | 一般用于细粒度级别上，对调试应用程序非常有帮助 |
| ALL      | 最低等级，打开所有日志记录                     |


__注意:一般只使用4个级别，优先级从高到低位 ERROR>WARN>INFO>DEBUG__

> Appender(输出端)

| 输出类型                 | 作用                                                                                                               |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------ |
| ConsoleAppender          | 将日志输出到控制台                                                                                                 |
| FileAppender             | 将日志输出到文件                                                                                                   |
| DailyRollingFileAppender | 将日志输出到一个日志文件，并且每天输出到一个新的文件                                                               |
| RollingFileAppender      | 将日志信息输出到一个日志文件，并且制定文件的尺寸，当文件大小达到制定尺寸时，会自动把文件改名，同时产生一个新的文件 |
| JDBCAppender             | 把日志信息保存到数据库中                                                                                           |


Appender 用来指定日志输出到哪个地方，可以同时指定日志的输出目的地。Log4j 常用的输出目的地有以下几种：

> Layout(日志格式化器)

| 格式化器类型  | 作用                                                                                     |
| ------------- | ---------------------------------------------------------------------------------------- |
| HTMLLayout    | 格式化日志输出为HTML表格形式                                                             |
| SimpleLayout  | 简单的日志输出格式化，打印的日志格式为(info - message)                                   |
| PatternLayout | 最强大的格式化器，可以根据自定义格式输出日志，如果没有指定转换格式，就是用默认的转换格式 |


#### 快速入门

```
//maven 依赖包
<dependency>
    <groupId>log4j</groupId>
    <artifactId>log4j</artifactId>
    <version>1.2.17</version>
</dependency>
```

1. 简单的测试代码

```
public class Log4jSimple
{
  public static void main(String[] args)
  {
    Logger logger = Logger.getLogger(Log4jSimple.class);
    //使用默认的配置信息，不需要写log4j.properties(加载项目路径下的log4j.properties配置文件)
    BasicConfigurator.configure();
    //设置日志输出级别为WARN,这将覆盖配置文件中的设置级别，只有日志级别高于WARN的日志才输出
    logger.setLevel(Level.WARN);

    logger.debug("调试级别日志");
    logger.info("信息级别日志");
    logger.warn("警告级别日志")
  }
}
```

2. 格式化器的使用

```
public class Log4jSimple
{
  public static void main(String[] args)
  {
    Logger logger = Logger.getLogger(Log4jSimle.class);
    BasicConfigurator.configure();
    HTMLLayout layout = new HTMLLayout();

    FileAppender appender = new FileAppender(layout,"D:\\out.html",false);
    logger.addAppender(appender);
    logger.setLevel(Level.WARN);

    logger.info("简单的info信息");//这条记录不会被写入
    logger.error("简单的error信息");//这条记录会被写入
  }
}
```

> log4j.properties 配置文件的使用

```
# 控制台输出配置
log4j.appender.Console=org.apache.log4j.ConsoleAppender
log4j.appender.Console.layout=org.apache.log4j.PatternLayout
log4j.appender.Console.layout.ConversionPattern=%d [%t] %-5p [%c] - %m%n

# 文件输出配置
log4j.appender.A = org.apache.log4j.DailyRollingFileAppender
log4j.appender.A.File = D:/log.txt #指定日志的输出路径
log4j.appender.A.Append = true
log4j.appender.A.Threshold = DEBUG
log4j.appender.A.layout = org.apache.log4j.PatternLayout #使用自定义日志格式化器
log4j.appender.A.layout.ConversionPattern = %-d{yyyy-MM-dd HH:mm:ss}  [ %t:%r ] - [ %p ]  %m%n #指定日志的输出格式
log4j.appender.A.encoding=UTF-8 #指定日志的文件编码

#指定日志的输出级别与输出端
log4j.rootLogger=DEBUG,Console,A
```
在 log4j.properties 配置文件中，我们定义了日志输出级别与输出端，在输出端中分别配置日志的输出格式。

*其中的Appender类型*
1 . org.apache.log4j.ConsoleAppender(控制台) ;
2 . org.apache.log4j.FileAppender(文件) ;
3 . org.apache.log4j.DailyRollingFileAppender(按照一定的频度滚动产生日志记录文件 , 默认每天产生一个文件) ;
4 . org.apache.log4j.RollingFileAppender(文件大小到达指定尺寸的时候产生一个新的文件) ;
5 . org.apache.log4j.WriterAppender(将日志信息以流格式发送到指定的位置) ;

*其中的Layout类型*
1 . org.apache.log4j.HTMLLayout(以HTML表格形式布局) ; 
2 . org.apache.log4j.PatternLayout(可以灵活的指定布局模式 , 需要配置layout.ConversionPattern属性) ;
3 . org.apache.log4j.SimpleLayout(包含日志信息的级别和信息字符串) ; 
4 . org.apache.log4j.TTCCLayout(包含日志产生的时间 , 线程 , 类别等等信息) ;

```
log4j 采用类似 C 语言的 printf 函数的打印格式格式化日志信息，具体的占位符及其含义如下：

%m   输出代码中指定的日志信息

%p    输出优先级，及 DEBUG、INFO 等

%n    换行符（Windows平台的换行符为 "\n"，Unix 平台为 "\n"）

%r     输出自应用启动到输出该 log 信息耗费的毫秒数

%c    输出打印语句所属的类的全名

%t     输出产生该日志的线程全名

%d    输出服务器当前时间，默认格式为 ISO8601，也可以在后面指定格式。如：%d{yyyy年MM月dd日 HH:mm:ss}

%l     输出日志时间发生的位置，包括类名、发生的线程，以及在代码中的行数，如：Test.main(Test.java:10)

%F    输出日志消息产生时所在的文件名称

%L    输出代码中的行号

%x    输出和当前线程相关的 NDC（嵌套诊断环境）

%%   输出一个 "%" 字符

可以在 % 与字符之间加上修饰符来控制最小宽度、最大宽度和文本的对其方式。如：

%5c    输出category名称，最小宽度是5，category<5，默认的情况下右对齐
%-5c   输出category名称，最小宽度是5，category<5，"-"号指定左对齐,会有空格
%.5c   输出category名称，最大宽度是5，category>5，就会将左边多出的字符截掉，<5不会有空格
%20.30c   category名称<20补空格，并且右对齐，>30字符，就从左边交远销出的字符截掉
```

有 `log4j.properties`文件的时候，日志的输出测试功能代码

```
public class Log4jSimpleTest
{
  public static void main(String[] args)
  {
    Logger logger = Logger.getLogger(Log4jSimpleTest.class);
    logger.debug("简单的debug日志输出")
  }
}
```

> 实际开发过程中给的标准配置为

1. 不同的package的日志输出级别

log4j.logger.packageName=Level

```
log4j.logger.org.springframework=info
log4j.logger.org.apache.catalina=info
log4j.logger.org.apache.commons.digester.Digester=info
log4j.logger.org.apache.catalina.startup.TldConfig=info
log4j.logger.com.bestkf=debug
```

```
#进行了不同日志的分类，分别放到不同的文件夹中
log4j.rootLogger=debug,stdout,info,debug,warn,error

#console
log4j.appender.stdout=org.apache.log4j.ConsoleAppender
log4j.appender.stdout.layout=org.apache.log4j.PatternLayout
log4j.appender.stdout.layout.ConversionPattern= [%d{yyyy-MM-dd HH:mm:ss a}]:%p %l%m%n
#info log
log4j.logger.info=info
log4j.appender.info=org.apache.log4j.DailyRollingFileAppender
log4j.appender.info.DatePattern='_'yyyy-MM-dd'.log'
log4j.appender.info.File=./logs/info.log
log4j.appender.info.Append=true
log4j.appender.info.Threshold=INFO
log4j.appender.info.layout=org.apache.log4j.PatternLayout
log4j.appender.info.layout.ConversionPattern=%d{yyyy-MM-dd HH:mm:ss a} [Thread: %t][ Class:%c >> Method: %l ]%n%p:%m%n
#debug log
log4j.logger.debug=debug
log4j.appender.debug=org.apache.log4j.DailyRollingFileAppender
log4j.appender.debug.DatePattern='_'yyyy-MM-dd'.log'
log4j.appender.debug.File=./logs/debug.log
log4j.appender.debug.Append=true
log4j.appender.debug.Threshold=DEBUG
log4j.appender.debug.layout=org.apache.log4j.PatternLayout
log4j.appender.debug.layout.ConversionPattern=%d{yyyy-MM-dd HH:mm:ss a} [Thread: %t][ Class:%c >> Method: %l ]%n%p:%m%n
#warn log
log4j.logger.warn=warn
log4j.appender.warn=org.apache.log4j.DailyRollingFileAppender
log4j.appender.warn.DatePattern='_'yyyy-MM-dd'.log'
log4j.appender.warn.File=./logs/warn.log
log4j.appender.warn.Append=true
log4j.appender.warn.Threshold=WARN
log4j.appender.warn.layout=org.apache.log4j.PatternLayout
log4j.appender.warn.layout.ConversionPattern=%d{yyyy-MM-dd HH:mm:ss a} [Thread: %t][ Class:%c >> Method: %l ]%n%p:%m%n
#error
log4j.logger.error=error
log4j.appender.error = org.apache.log4j.DailyRollingFileAppender
log4j.appender.error.DatePattern='_'yyyy-MM-dd'.log'
log4j.appender.error.File =./logs/error.log
log4j.appender.error.Append = true
log4j.appender.error.Threshold = ERROR
log4j.appender.error.layout = org.apache.log4j.PatternLayout
log4j.appender.error.layout.ConversionPattern = %d{yyyy-MM-dd HH:mm:ss a} [Thread: %t][ Class:%c >> Method: %l ]%n%p:%m%n
```
