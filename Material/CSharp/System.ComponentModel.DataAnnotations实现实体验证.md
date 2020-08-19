#### 如何使用 `System.ComponentModel.DataAnnotations` 进行业务实体验证

1. 添加引用 `Sytstem.ComponentModel.DataAnnotations` 程序集

2. 写实体

```
public class PersonEntity
{
    [Required(ErrorMessage ="{0}必须填写")]
    [DisplayName("姓名")]
    public string Name { get; set; }

    [Required(ErrorMessage = "{0} 必须填写")]
    [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "邮件格式不正确")]
    public string Email { get; set; }

    [Required(ErrorMessage = "{0} 必须填写")]
    [Range(1, 100, ErrorMessage = "超出范围")]
    public int Age { get; set; }

    [Required(ErrorMessage = "{0} 必须填写")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "{0}输入长度不正确")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "{0} 必须填写")]
    [Range(typeof(decimal), "1000.00", "2000.99")]
    public decimal Salary { get; set; }
}
```

3. 调用验证

```
var personValue = new PersonEntity();
person.Name = "";
person.Email = "qqxx.com";
person.Phone = "15544488";
person.Salary = 5000;


var validationContext = new ValidationContext(personValue);
var results = new List<ValidationResult>();
var isValid = Validator.TryValidateObject(value, validationContext, results, true);

//有没有验证通过看  isValid
//如果有错误信息看  results
```
