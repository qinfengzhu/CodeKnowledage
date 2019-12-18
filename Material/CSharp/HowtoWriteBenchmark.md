### 怎么写基准测试

1. 先写一个需要测试的功能类,下面这个类为 `MD5VsSHA256`

```
using BenchmarkDotNet.Attributes;
using System;
using System.Security.Cryptography;

namespace YozoBenchmarkTest
{
    public class MD5VsSHA256
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly MD5 md5 = MD5.Create();

        public MD5VsSHA256()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }
        [Benchmark]
        public byte[] Sha256()
        {
            return sha256.ComputeHash(data);
        }
        [Benchmark]
        public byte[] Md5()
        {
            return md5.ComputeHash(data);
        }
    }
}
```

2. 执行性能测试,在 `summary`看到性能的结果

```
using System;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace YozoBenchmarkTest
{
    /// <summary>
    /// 运行Benchmark
    /// </summary>
    [TestFixture]
    public class RunTest
    {
        [Test]
        public void MD5AndSha256Test()
        {
            var summary = BenchmarkRunner.Run<MD5VsSHA256>();

            Assert.AreNotEqual(string.Empty, summary);
        }
    }
}
```
