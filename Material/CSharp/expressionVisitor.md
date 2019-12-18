### Expression Visitor 分析

1. [Expression Visitor源代码](https://referencesource.microsoft.com/#System.Core/Microsoft/Scripting/Ast/ExpressionVisitor.cs,f0615a823a9efe36)

2. 'ExpressionVistior' 调整特定的'Expression'的内容

```
public class AndAlsoModifier : ExpressionVisitor  
{  
    public Expression Modify(Expression expression)  
    {  
        return Visit(expression);  
    }  

    protected override Expression VisitBinary(BinaryExpression b)  
    {  
        if (b.NodeType == ExpressionType.AndAlso)  
        {  
            Expression left = this.Visit(b.Left);  
            Expression right = this.Visit(b.Right);  

            // Make this binary expression an OrElse operation instead of an AndAlso operation.  
            return Expression.MakeBinary(ExpressionType.OrElse, left, right, b.IsLiftedToNull, b.Method);  
        }  

        return base.VisitBinary(b);  
    }  
}
```

*call*
```
Expression<Func<string, bool>> expr = name => name.Length > 10 && name.StartsWith("G");  
Console.WriteLine(expr);  

AndAlsoModifier treeModifier = new AndAlsoModifier();  
Expression modifiedExpr = treeModifier.Modify((Expression) expr);  

Console.WriteLine(modifiedExpr);  

/*  This code produces the following output:  

    name => ((name.Length > 10) && name.StartsWith("G"))  
    name => ((name.Length > 10) || name.StartsWith("G"))  
*/  
```

3. 使用Expression Tree 实现动态查询,实现`companies.Where(company => (company.ToLower() == "coho winery" || company.Length > 16)).OrderBy(company => company)`

```
// Add a using directive for System.Linq.Expressions.  

string[] companies = { "Consolidated Messenger", "Alpine Ski House", "Southridge Video", "City Power & Light",  
                   "Coho Winery", "Wide World Importers", "Graphic Design Institute", "Adventure Works",  
                   "Humongous Insurance", "Woodgrove Bank", "Margie's Travel", "Northwind Traders",  
                   "Blue Yonder Airlines", "Trey Research", "The Phone Company",  
                   "Wingtip Toys", "Lucerne Publishing", "Fourth Coffee" };  

// The IQueryable data to query.  
IQueryable<String> queryableData = companies.AsQueryable<string>();  

// Compose the expression tree that represents the parameter to the predicate.  
ParameterExpression pe = Expression.Parameter(typeof(string), "company");  

// ***** Where(company => (company.ToLower() == "coho winery" || company.Length > 16)) *****  
// Create an expression tree that represents the expression 'company.ToLower() == "coho winery"'.  
Expression left = Expression.Call(pe, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));  
Expression right = Expression.Constant("coho winery");  
Expression e1 = Expression.Equal(left, right);  

// Create an expression tree that represents the expression 'company.Length > 16'.  
left = Expression.Property(pe, typeof(string).GetProperty("Length"));  
right = Expression.Constant(16, typeof(int));  
Expression e2 = Expression.GreaterThan(left, right);  

// Combine the expression trees to create an expression tree that represents the  
// expression '(company.ToLower() == "coho winery" || company.Length > 16)'.  
Expression predicateBody = Expression.OrElse(e1, e2);  

// Create an expression tree that represents the expression  
// 'queryableData.Where(company => (company.ToLower() == "coho winery" || company.Length > 16))'  
MethodCallExpression whereCallExpression = Expression.Call(  
    typeof(Queryable),  
    "Where",  
    new Type[] { queryableData.ElementType },  
    queryableData.Expression,  
    Expression.Lambda<Func<string, bool>>(predicateBody, new ParameterExpression[] { pe }));  
// ***** End Where *****  

// ***** OrderBy(company => company) *****  
// Create an expression tree that represents the expression  
// 'whereCallExpression.OrderBy(company => company)'  
MethodCallExpression orderByCallExpression = Expression.Call(  
    typeof(Queryable),  
    "OrderBy",  
    new Type[] { queryableData.ElementType, queryableData.ElementType },  
    whereCallExpression,  
    Expression.Lambda<Func<string, string>>(pe, new ParameterExpression[] { pe }));  
// ***** End OrderBy *****  

// Create an executable query from the expression tree.  
IQueryable<string> results = queryableData.Provider.CreateQuery<string>(orderByCallExpression);  

// Enumerate the results.  
foreach (string company in results)  
    Console.WriteLine(company);  
```

4. Expression 转换为本地的 表示方式

4.1  [转换方式参考](https://www.cnblogs.com/linxingxunyan/p/McKay.html)

4.2 [ExpressionTreeVisualizer方便查看表达式树](https://github.com/Feddas/ExpressionTreeVisualizer)

4.3 [Expression Visitor Demo](ExpressionVisitorSimpleDemo.cs)
