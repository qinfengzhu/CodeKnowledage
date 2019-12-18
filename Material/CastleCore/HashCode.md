### 类中多个标识String，HashCode函数计算

```
public override int GetHashCode()
{
    int result = dependencyKey.GetHashCode(); //dependencyKey 为String类型
    result += 37 ^ targetType.GetHashCode(); //targetType为Type 类型
    result += 37 ^ isOptional.GetHashCode(); //isOptional为bool类型
    result += 37 ^ dependencyType.GetHashCode();//dependencyType为自定义类型
    return result;
}
```
(并使用常数 31, 33, 37, 39 和 41 作为乘子)
为啥取37异或，有的项目取的是31* ，都是取一个不大不小的质子数。
java中31 是可以被优化的:31 * i = (i << 5) - i

```
public int HasCode(object a[])
{
    if(a==null)
        return 0;
    int result = 1;
    for(object element in a)
        result = 31 * result + (element == null? 0 :element.GetHashCode());
    return result;
}
```
