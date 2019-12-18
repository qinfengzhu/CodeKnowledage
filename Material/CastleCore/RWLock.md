### ReaderWriterLockSlim (.Net 3.5),在.net 2.0中使用(ReaderWriterLock)

1. 作用: 表示用于管理资源访问的锁定状态，可实现多线程读取或进行独占式写入访问. ReaderWriterLockSlim 允许多个线程均处于读取模式，
允许一个线程处于写入模式并独占锁定状态，同时还允许一个具有读取权限的线程处于可升级的读取模式，在此模式下线程无需放弃对资源的读取权限即可升级为写入模式。
2. 写法

//比较古老的写法
```
private object _Lock = new object();

private void Read()
{
    lock (_Lock)
    {
        //具体方法实现
    }
}

private void Write()
{
    lock (_Lock)
    {
        //具体方法实现
    }
}
```
//读写锁分离写法,从美观程度上不如 lock写法简洁

```
private ReaderWriterLockSlim _LockSlim = new ReaderWriterLockSlim();
private void Read()
{
    try
    {
        _LockSlim.EnterReadLock();
        //具体方法实现
    }
    finally
    {
        _LockSlim.ExitReadLock();
    }
}

private void Write()
{
    try
    {
        _LockSlim.EnterWriteLock();
        //具体方法实现
    }
    finally
    {
        _LockSlim.ExitWriteLock();
    }
}
```

3. 优化 `ReaderWriterLockSlim`的写法
//简单包装
```
public class SlimReaderWriterLock
{
    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	public void EnterReadLock()
	{
		locker.EnterReadLock();
	}

	public void EnterWriteLock()
	{
		locker.EnterWriteLock();
	}

	public void EnterUpgradeableReadLock()
	{
		locker.EnterUpgradeableReadLock();
	}

	public void ExitReadLock()
	{
		locker.ExitReadLock();
	}

	public void ExitWriteLock()
	{
		locker.ExitWriteLock();
	}

	public void ExitUpgradeableReadLock()
	{
		locker.ExitUpgradeableReadLock();
	}
}
```
### ReadLock

//读锁
```
public struct ReadLock : IDisposable
{
    private readonly SlimReaderWriterLock locker;

    public ReadLock(SlimReaderWriterLock locker)
    {
        this.locker = locker;
        locker.EnterReadLock();
    }

    public void Dispose()
    {
        locker.ExitReadLock();
    }
}
```
### WriteLock

//写锁
```
public struct WriteLock : IDisposable
{
    private readonly SlimReaderWriterLock locker;

    public WriteLock(SlimReaderWriterLock locker)
    {
        this.locker = locker;
        locker.EnterWriteLock();
    }

    public void Dispose()
    {
        locker.ExitWriteLock();
    }
}
```

### UpgradableLock

//可升级锁
```
public struct UpgradableLock : IDisposable
{
    private readonly SlimReaderWriterLock locker;
    private bool lockWasUpgraded;

    public UpgradableLock(SlimReaderWriterLock locker)
    {
        this.locker = locker;
        locker.EnterUpgradeableReadLock();
        lockWasUpgraded = false;
    }

    public void Upgrade()
    {
        locker.EnterWriteLock();
        lockWasUpgraded = true;
    }

    public void Dispose()
    {
        if (lockWasUpgraded)
        {
            locker.ExitWriteLock();
        }

        locker.ExitUpgradeableReadLock();
    }
}
```
