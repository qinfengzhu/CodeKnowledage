### 正则表达式写法验证

### 正则表达式转NFA

### 转换算法理论

![算法理论-01](LexicalPart1.pdf)
![算法理论-02](LexicalPart2.pdf)
![算法理论-03](LexicalPart3.pdf)
![算法理论-04](LexicalPart4.pdf)

### 转换步骤
1. 正则表达式转NFA步骤

一般正则表达式的字符串表示形式为`中缀表达式`的方式,但计算机比较好理解的是`后缀表达式`,需要用到`逆波兰算法转换`,

后缀表达式需要转换为状态图

2. NFA转换为DFA

消除ε边
状态Table表
Mini化后的状态Table表

3. 字符串读入进行配对


### [主要资料源头出处](http://web.cecs.pdx.edu/~harry/compilers/)
