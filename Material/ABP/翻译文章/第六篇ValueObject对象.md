#### Value-Objects (值对象)

“表示没有概念标识的域的描述方面的对象称为值对象。”（Eric Evans）。

与具有身份（id）的实体相反，值对象没有它的标识。如果两个实体的身份不同，即使这些实体的所有其他属性相同，它们也被视为不同的对象/实体。认为两个不同的人有相同的名字，姓氏和年龄，但他们是不同的人，如果他们的身份号码是不同的。但是，对于一个地址（这是一个经典值对象）类，如果两个地址具有相同的国家，城市，街道号…等等，它们被认为是同一个地址。

在领域驱动设计（DDD）中，值对象是另一种类型的域对象，它可以包含业务逻辑，是域的重要组成部分。

> Value Object Base Class (值对象的基类)

在ABP框架中，可以基于  `ValueObject<T>` 泛型基类创建值对象类型，例如

```
public class Address : ValueObject<Address>
{
    public Guid CityId { get; private set; } //A reference to a City entity.

    public string Street { get; private set; }

    public int Number { get; private set; }

    public Address(Guid cityId, string street, int number)
    {
        CityId = cityId;
        Street = street;
        Number = number;
    }
}
```

值类型继承基础类重载相等运算符（以及其他相关的操作和方法）比较两个值类型，假设所有的特性都是相同的他们都是相同的。

```
var address1 = new Address(new Guid("21C67A65-ED5A-4512-AA29-66308FAAB5AF"), "Baris Manco Street", 42);
var address2 = new Address(new Guid("21C67A65-ED5A-4512-AA29-66308FAAB5AF"), "Baris Manco Street", 42);

Assert.Equal(address1, address2);
Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
Assert.True(address1 == address2);
Assert.False(address1 != address2);
```
即使它们是内存中不同的对象，它们对于我们的域也是相同的,(他们比较的是各个属性的值是否相等)

> 最佳实践

如果没有很好的理由将值对象设计为可变的，那么就要设计一个不可变的值对象（如上面的地址）。
构成一个值对象的属性应该构成一个概念整体。例如，cityid，街道和号码不应该被一个人员实体分离。此外，这使实体更简单。
