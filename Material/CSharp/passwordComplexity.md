### 默认密码及修改密码要求至少 8 位以上，至少大写字母小写字母、数字、特殊字符三种组合

1. 具体实现代码如下
```
var regex = @"^(?=.*[0-9])(?=.*[a-zA-Z])(?=.*[\`\~\!\@\#\$\%\^\&\*\(\)_\+\-\=\{\}\|\[\]\:\;\'\<\>\?\,\.]).{8,}$";

var passwordRegex = new Regex(regex,RegexOptions.Compiled);

var passwords = new string[] { @"123456" , @"Zz111111" , @"Zz1234!@#$" };
var conditions = new bool[passwords.Length];

int index = 0;
foreach(var password in passwords)
{
    conditions[index] = passwordRegex.IsMatch(password);
    index++;
}

```

2. 拆分具体的细节部分

数字匹配 `(?=.*[0-9])`

大小写字母匹配 `(?=.*[a-zA-Z])`

大写字母匹配 `(?=.*[A-Z])`

小写字母匹配 `(?=.*[a-z])`

特殊符号匹配 `(?=.*[\``\~\!\@\#\$\%\^\&\*\(\)_\+\-\=\{\}\|\[\]\:\;\'\<\>\?\,\.])`

3. 讲解 `(?=.*` 匹配模式
