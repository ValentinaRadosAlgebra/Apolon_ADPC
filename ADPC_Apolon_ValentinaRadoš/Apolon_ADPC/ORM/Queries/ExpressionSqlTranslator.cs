using System.Linq.Expressions;
using System.Reflection;
using Apolon_ADPC.ORM.Attributes;

namespace Apolon_ADPC.ORM.Queries
{
    public static class ExpressionSqlTranslator
    {
        public static string Translate<T>(Expression<Func<T, bool>> expr)
        {
            if (expr.Body is not BinaryExpression body)
                throw new NotSupportedException("Only simple binary expressions are supported");

            string columnName;
            object value;

            if (body.Left is MemberExpression leftMember)//Extract left side (column)
            {
                var prop = typeof(T).GetProperty(leftMember.Member.Name);
                var colAttr = prop?.GetCustomAttribute<ColumnAttribute>();//checks if column
                columnName = colAttr?.Name ?? prop?.Name.ToLower() ?? throw new Exception("Cannot determine column name");
            }
            else
                throw new NotSupportedException("Left side must be a member");

            switch (body.Right)//Extract right side(value)
            {
                case ConstantExpression constExpr: //x => x.Age == 25
                    value = constExpr.Value;
                    break;

                case MemberExpression memberExpr: //x => x.Age == someVariable
                    var objectMember = Expression.Convert(memberExpr, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    value = getterLambda.Compile()();
                    break;

                default:
                    throw new NotSupportedException($"Right side type {body.Right.GetType()} is not supported");
            }

            string formattedValue = value is string ? $"'{value}'" : value.ToString();

            return $"{columnName} = {formattedValue}"; //builds the final SQL WHERE condition string
        }
    }
}
