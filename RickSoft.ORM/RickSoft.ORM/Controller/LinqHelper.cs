using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Model;

namespace RickSoft.ORM.Engine.Controller
{
    internal class LinqHelper
    {
        internal static SqlWhereCondition ParseLinqExpression<T>(Expression<Predicate<T>> selector)
            where T : DatabaseObject, new()
        {
            BinaryExpression op = (BinaryExpression)selector.Body;

            ParameterExpression param = selector.Parameters[0];
            
            MemberExpression left = (MemberExpression)op.Left;

            PropertyInfo prop = left.Member as PropertyInfo;

            var enumerator = left.Member.GetCustomAttributes(typeof(DataFieldAttribute)).GetEnumerator();
            enumerator.MoveNext();

            var attr = enumerator.Current as DataFieldAttribute;

            SqlWhereCondition condition = new SqlWhereCondition();
            condition.Operator = QueryBuilder.GetSqlOperator(op.NodeType);
            condition.Column = attr.Column;

            condition.Value = Expression.Lambda(op.Right).Compile().DynamicInvoke();

            //if (op.Right is ConstantExpression)
            //    condition.Value = ((ConstantExpression) op.Right).Value;
            //else
            //    condition.Value = Expression.Lambda(op.Right).Compile().DynamicInvoke();

            return condition;
        }
    }
}
