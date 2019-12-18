### Map与Reduce编程模型

1. MapReduce来源于google

Map为映射,Reduce为归约，主要思想是从函数式编程语言借来的。
`函数式`:函数式编程的一个特点就是，允许把函数本身作为参数传入另一个函数，还允许返回一个函数！
实现一个Map(映射)函数,用来把一组键值对映射成一组新的键值对,指定并发的Reduce(归约)函数。

2. Map与Reduce在Python中

`Map`: map函数接受两个参数,一个是函数,一个是`Iterable`,`Map`将传入的函数依次作用到序列的每一个元素,
并把结果作为新的`Iterator`返回。

```
from functools import reduce

def multi(x):
    return x*x

ir = map(multi,[1,2,3,4,5,6,7,8,9])
list(ir)

```

`Reduce`: reduce 把一个函数作用在一个序列`[x1,y1,z1,......]`上,这个函数必须接受两个参数,
reduce把结果和序列的下一个元素做累积计算.

```
from functools import reduce

def bgm(x,y):
    return x*10+y

# [2,3,4,6]转换为2346
reduce(bgm,[2,3,4,6])

```

把字符串转换为数字的函数,MapReduce实现

```
from functools import reduce

def fn(x,y):
    return x*10+y

def char2num(s):
    digits={'0':0,'1':1,'2':2,'3':3,'4':4,'5':5,'6':6,'7':7,'8':8,'9':9,}
    return digits[s]

# 实现转换
reduce(fn,map(char2num,'2346'))

```

优化实现为

```
DIGITS = {'0': 0, '1': 1, '2': 2, '3': 3, '4': 4, '5': 5, '6': 6, '7': 7, '8': 8, '9': 9}

def str2int(s):
    def fn(x,y):
        return x*10+y
    def char2num(s):
        return DIGITS[s]
    return
```
