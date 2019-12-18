### Filter思想

1. Python中直接有Filter 函数，Filter主要是对集合进行过滤

```
def isOdd(n):
    return n%2==1
# 结果 1,3,5,7
list(filter(isOdd,[1,2,3,4,5,6,7]))

```
