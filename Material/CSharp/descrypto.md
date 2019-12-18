### 对称加密DES


1. DES加密,加密模式:ECB,填充:PKCS7,输出:Hex

```
public string desKey = "bestkf";  //对称加密的Key

//加密
public string Encrypt(string password)
{
    DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
    desProvider.Mode = CipherMode.ECB;
    desProvider.Padding = PaddingMode.PKCS7;
    desProvider.Key = Encoding.UTF8.GetBytes(desKey);
    using (MemoryStream stream = new MemoryStream())
    {
        using (CryptoStream cs = new CryptoStream(stream, desProvider.CreateEncryptor(), CryptoStreamMode.Write))
        {
            byte[] data = Encoding.UTF8.GetBytes(password);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            var ba = stream.ToArray();
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            var encryptedPassword = hex.ToString();

            return encryptedPassword;
        }
    }
}

//解密
public string Decrypt(string encryptedPassword)
{
    DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
    desProvider.Mode = CipherMode.ECB;
    desProvider.Padding = PaddingMode.PKCS7;
    desProvider.Key = Encoding.UTF8.GetBytes(desKey);
    using (MemoryStream stream = new MemoryStream())
    {
        using (CryptoStream cs = new CryptoStream(stream, desProvider.CreateDecryptor(), CryptoStreamMode.Write))
        {
            var length = encryptedPassword.Length / 2;
            byte[] inputByteArray = new byte[length];
            for (int x = 0; x < length; x++)
            {
                int i = (Convert.ToInt32(encryptedPassword.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            var password = Encoding.UTF8.GetString(stream.ToArray());
            return password;
        }
    }
}
```
