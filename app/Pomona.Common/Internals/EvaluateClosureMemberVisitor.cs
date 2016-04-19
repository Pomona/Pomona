#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public class EvaluateClosureMemberVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == null)
                return Expression.Constant(node.Member.GetPropertyOrFieldValue(null));

            var nodeExpression = Visit(node.Expression);
            if (nodeExpression.NodeType == ExpressionType.Constant)
            {
                var target = ((ConstantExpression)nodeExpression).Value;

                var propInfo = node.Member as PropertyInfo;
                if (propInfo != null)
                    return Expression.Constant(propInfo.GetValue(target, null), propInfo.PropertyType);
                var fieldInfo = node.Member as FieldInfo;
                if (fieldInfo != null)
                    return Expression.Constant(fieldInfo.GetValue(target), fieldInfo.FieldType);
            }
            return base.VisitMember(node);
        }
    }
}