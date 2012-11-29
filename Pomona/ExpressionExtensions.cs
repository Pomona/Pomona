using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona
{
    public static class ExpressionExtensions
    {
         public static PropertyInfo ExtractPropertyInfo(this Expression expr)
         {
             if (expr.NodeType == ExpressionType.Lambda)
                expr = ((LambdaExpression)expr).Body;

             while (expr.NodeType == ExpressionType.Convert)
                 expr = ((UnaryExpression)expr).Operand;

             var memberExpr = expr as MemberExpression;
             if (memberExpr == null || memberExpr.Member.MemberType != MemberTypes.Property)
                 throw new ArgumentException("Requires a expression that has a property member access");

             return (PropertyInfo)memberExpr.Member;
         }
    }
}