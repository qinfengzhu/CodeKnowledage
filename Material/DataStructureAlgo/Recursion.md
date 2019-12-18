### 递归调用与尾递归优化

1. 递归调用很常见的有

```
def fact(n):
    if n = 1:
        return 1
    return n*fact(n-1)  
```

递归调用缺陷就是调用栈太多，影响性能,如果`Return` 语句不包含表达式,编译器或者解释器就会对代码进行优化，
使递归本身无论调用多少次,都只占用一个栈帧,不会出现栈溢出的情况

2. 优化递归的，尾递归

```
def fact(n):
    return fact_iter(n,1)

def fact_iter(num,base)
    if num==1:
        return base
    return fact_iter(num-1,num*product)

```
