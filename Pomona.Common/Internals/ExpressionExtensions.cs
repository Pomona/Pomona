using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Pomona.Internals;
using Pomona.Common.Linq;

namespace Pomona.Common.Internals
{
    public static class ExpressionExtensions
    {
        private static MethodInfo enumerableExpandMethod;

        static ExpressionExtensions()
        {
            enumerableExpandMethod =
                ReflectionHelper.GetGenericMethodDefinition<IEnumerable<object>>(x => x.Expand(y => 0));
        }

        private static void GetPropertyPath(Expression expr, ParameterExpression thisParam, StringBuilder sb,
                                            bool jsonNameStyle)
        {
            var memberExpr = expr as MemberExpression;
            if (memberExpr == null)
            {
                var methodCallExpr = expr as MethodCallExpression;
                if (methodCallExpr != null && methodCallExpr.Method.MetadataToken == enumerableExpandMethod.MetadataToken)
                {
                    GetPropertyPath(methodCallExpr.Arguments[0], thisParam, sb, jsonNameStyle);
                    var innerLambda = (LambdaExpression) methodCallExpr.Arguments[1];
                    GetPropertyPath(innerLambda.Body, innerLambda.Parameters[0], sb, jsonNameStyle);
                    return;
                }
                throw new ArgumentException("Can only get property path of MemberExpression");
            }
            if (memberExpr.Expression != thisParam)
                GetPropertyPath(memberExpr.Expression, thisParam, sb, jsonNameStyle);

            if (sb.Length > 0)
                sb.Append('.');
            var name = memberExpr.Member.Name;
            name = jsonNameStyle ? name.LowercaseFirstLetter() : name;
            sb.Append(name);
        }

        public static string GetPropertyPath(this LambdaExpression lambdaExpression, bool jsonNameStyle = false)
        {
            if (lambdaExpression == null) throw new ArgumentNullException("lambdaExpression");
            var sb = new StringBuilder();

            var body = lambdaExpression.Body;
            GetPropertyPath(body, lambdaExpression.Parameters[0], sb, jsonNameStyle);
            return sb.ToString();
        }

        public static PropertyInfo ExtractPropertyInfo(this Expression expr)
        {
            if (expr.NodeType == ExpressionType.Lambda)
                expr = ((LambdaExpression) expr).Body;

            while (expr.NodeType == ExpressionType.Convert)
                expr = ((UnaryExpression) expr).Operand;

            var memberExpr = expr as MemberExpression;
            if (memberExpr == null || memberExpr.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("Requires a expression that has a property member access");

            return (PropertyInfo) memberExpr.Member;
        }
    }
}