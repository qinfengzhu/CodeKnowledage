using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTree
{
    public class FilterConstructor : ExpressionVisitor
    {
        private static readonly Dictionary<ExpressionType, string> _logicalOperators;
        private static readonly Dictionary<Type, Func<object, string>> _typeConverters;
        static FilterConstructor()
        {
            _logicalOperators = new Dictionary<ExpressionType, string>
            {
                [ExpressionType.Not] = "not",
                [ExpressionType.GreaterThan] = "gt",
                [ExpressionType.GreaterThanOrEqual] = "ge",
                [ExpressionType.LessThan] = "lt",
                [ExpressionType.LessThanOrEqual] = "le",
                [ExpressionType.Equal] = "eq",
                [ExpressionType.Not] = "not",
                [ExpressionType.AndAlso] = "and",
                [ExpressionType.OrElse] = "or"
            };

            _typeConverters = new Dictionary<Type, Func<object, string>>
            {
                [typeof(string)] = x => $"'{x}'",
                [typeof(DateTime)] = x => $"datetime'{((DateTime)x).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}'",
                [typeof(bool)] = x => x.ToString().ToLower()
            };
        }

        private StringBuilder _queryStringBuilder;
        private Stack<string> _fieldNames;
        public FilterConstructor()
        {
            _queryStringBuilder = new StringBuilder();
            _fieldNames = new Stack<string>();
        }

        public string GetQuery(LambdaExpression predicate)
        {
            Visit(predicate.Body);
            var result = _queryStringBuilder.ToString();
            _queryStringBuilder.Clear();

            return result;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Not)
                throw new NotSupportedException("Only not(\"!\") unary operator is supported!");

            _queryStringBuilder.Append($"{_logicalOperators[node.NodeType]} ");

            _queryStringBuilder.Append("(");
            Visit(node.Operand);
            _queryStringBuilder.Append(")");

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _queryStringBuilder.Append("(");
            Visit(node.Left);

            _queryStringBuilder.Append($" {_logicalOperators[node.NodeType]} ");

            Visit(node.Right);
            _queryStringBuilder.Append(")");

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _queryStringBuilder.Append(GetValue(node.Value));
            return node;
        }

        private string GetValue(object input)
        {
            var type = input.GetType();
            if (type.IsClass && type != typeof(string))
            {
                var fieldName = _fieldNames.Pop();
                var fieldInfo = type.GetField(fieldName);
                object value;
                if (fieldInfo != null)
                    value = fieldInfo.GetValue(input);
                else
                    value = type.GetProperty(fieldName).GetValue(input);
                return GetValue(value);
            }
            else
            {
                if (_typeConverters.ContainsKey(type))
                    return _typeConverters[type](input);
                else
                    return input.ToString();
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant || node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                _fieldNames.Push(node.Member.Name);
                Visit(node.Expression);
            }
            else
                _queryStringBuilder.Append(node.Member.Name);
            return node;
        }
    }

    public class Order
    {
        public DateTime TheDate { get; set; }
        public string Customer { get; set; }
        public decimal Amount { get; set; }
        public bool Discount { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var date = DateTime.Today;
            var order = new Order { Customer = "Tom", Amount = 1000 };

            Expression<Func<Order, bool>> filter = x => (x.Customer == order.Customer && x.Amount > order.Amount)
                                                        ||
                                                        (x.TheDate == date && !x.Discount);
            var visitor = new FilterConstructor();
            var query = visitor.GetQuery(filter);

            Console.WriteLine(query);
        }
    }
}
