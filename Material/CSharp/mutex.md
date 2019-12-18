### 并发资源的处理

1. 线程的生命周期

`创建线程` :
  第一步,定义一个线程入口点的方法;

```

//冒泡排序
private void SortAscending()
{
   for (int i= 0;i < NumbersToSort.Length ;i++)
   {
     for (int j=0;j<i;j++)
     {
       if (NumbersToSort[i] < NumbersToSort[j])
       {
         Swap(ref NumbersToSort[i],ref NumbersToSort[j]);
       }
     }
   }
 }

 //两个数交换
 private void Swap(ref long First,ref long Second)
 {
     long TempNumber = First;
     First = Second;
     Second = TempNumber;
 }

```

  第二步，声明并且创建一个线程启动委托，该委托被用于关联线程的入口点；(如果线程与一个实例方法相关联，它将可以访问所有包含在类实例中的实例变量。)

```
ThreadStart ThreadStartDelegate;
ThreadStartDelegate = new ThreadStart(SortAscending);
```

  第三步,创建一个线程实例，并且把启动线程的委托传入到线程构造函数中去。

```
Thread exampleThread;
exampleThread = new Thread(ThreadStartDelegate);
```

`启动线程` :

```
exampleThread.Start();
```

`结束线程` :

```
exampleThread.Abort(); //会有ThreadAbortException异常
```

### 并发控制

1. 线程安全意味着仅使用原子操作，使用原子操作确保一个单元的操作不会被打断。

2. 线程安全最有效的方法是使用同步代码，`Lock`;`SyncLock`;`Monitor`的`Enter`与`Exit`方法。

3. `Monitor` 还提供了其他的方法，允许更高级别的控制多线程。它们允许以某种方式暂停线程的执行，其他线程可以在需要做
额外工作时发出信号。

4. C# 提供关键字 `volatile`，作用于变量,简单地说就是防止编译器对代码进行优化,但是也不能确保线程安全。

```
XBYTE[2]=0x55;
XBYTE[2]=0x56;
XBYTE[2]=0x57;
XBYTE[2]=0x58;
```
对外部硬件而言，上述四条语句分别表示不同的操作，会产生四种不同的动作，但是编译器却会对上述四条语句进行优化，认为只有`XBYTE[2]=0x58`（即忽略前三条语句，只产生一条机器代码）。如果键入volatile，则编译器会逐一地进行编译并产生相应的机器代码（产生四条代码）

5. `Hashtable` 是多线程读取安全，单个写入线程安全(`Reader-Writer Lock`，所以可以单线程更新)。
`ArrayList` 是多线程安全读取,没有线程更新的安全。
`Queue` 底层使用的是`Hashtable`,如果`Queue`需要多线程安全，则调用 `Synchronized` 方法,即`Queue.Synchronized(xx)`静态方法


### 并发的控制

1. 比如`x+=1;` 这种的原子操作，是非线程安全的。怎么处理让操作符安全呢， `Interlocked` 类提供了 `Interlocked.Increment(ref intOrlong)` 或者 `Interlocked.Decrement(ref intOrlong)`;
`Interlocked.Exchange(a,b)`  如果(a 是int;b 也是 int)以原子操作的形式，将 32 位有符号整数设置为指定的值并返回原始值。

2. `Lock` 类似于 `Monitor.Enter(obj); try{xxxxx} finnally{Monitor.Exit(obj);}`

3. `Wait Handler`（句柄控制类）;`ManualResetEvent` 一次发所有信号,手动的去打开关闭信号,`AutoResetEvent` 一次打开一个信号，自动关闭信号.,`Mutex` 一个同步基元，也可用于`进程间`同步，`Semaphore` 限制可同时访问某一资源或资源池的线程数

4. `ReaderWriterLock`  定义支持单个写线程和多个读线程的锁 ,`ReaderWriterLockSlim` 表示用于管理资源访问的锁定状态，可实现多线程读取或进行独占式写入访问,与 `ReaderWriterLock` 的区别，这个锁可以由读锁升级为写锁。

5. `ThreadPool` 提供一个线程池，该线程池可用于执行任务、发送工作项、处理异步 I/O、代表其他线程等待以及处理计时器。主要用于异步

6. `ThreadStaticAttribute`  指示各线程的静态字段值是否唯一,主要对字段的标记。

7. `CountdownEvent`  表示在计数变为零时处于有信号状态的同步基元。

7. `Delegates`,`TreadStart`,`Timer`,`TimerCallback`,`WaitCallback`
