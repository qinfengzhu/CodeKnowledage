### 简单cmd命令,并且获取执行后返回的输出内容


1. Demo 关于对ST 版本的检测
```
[TestFixture]
/// <summary>
/// SAS 工具转换测试
/// </summary>
public class STVersionLicenseTest
{
    [Test]
    public void STExistTest()
    {

    }
    StringBuilder outPutString = new StringBuilder();
    [Test]
    public void LicenseTest()
    {
        //Stat/Transfer - Command Processor (c) 1986-2018 Circle Systems, Inc.www.stattransfer.com Version 14.1.1002.0604 - 64 Bit WindowsStatus:	Trial Mode - Missing license file   
        using (var p = new Process())
        {
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = @"/c st -version"; // 其中 /c 的重要性
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.OutputDataReceived += (a, b) => AppendString(b.Data);
            p.ErrorDataReceived += (a, b) => AppendString(b.Data);
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
        }
        Assert.IsNotNullOrEmpty(outPutString.ToString());
    }
    private void AppendString(string data)
    {
        outPutString.Append(data);
    }
}
```
