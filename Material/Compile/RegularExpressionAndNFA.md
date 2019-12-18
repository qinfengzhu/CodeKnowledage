### 正则表达式与nfa

1. 代表空字符的NFA

![代表空字符的NFA](images/nfa-empty.png)

2. 代表常量字符串的NFA，比如字符`a`

![常量字符NFA](images/nfa-character.png)

3. 代表组合的NFA,比如`a|b`

![代表组合的NFA](images/nfa-union.png)

4. 代表连接的NFA,比如`ab`

![代表连接的NFA](images/nfa-concate.png)

5. 代表克林姆闭包的NFA，比如`a*`

![代表克林姆闭包的NFA](images/nfa-kleene.png)
