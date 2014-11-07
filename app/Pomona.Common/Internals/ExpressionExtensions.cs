#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Pomona.Common.Linq;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Internals
{
    public static class ExpressionExtensions
    {
        private static readonly UniqueMemberToken[] expandMethodTokens;


        static ExpressionExtensions()
        {
            expandMethodTokens = new[]
            {
                ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Expand(y => 0)).UniqueToken(),
                ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.Expand(y => 0)).UniqueToken()
            };
        }


        public static PropertyInfo ExtractPropertyInfo(this Expression expr)
        {
            PropertyInfo property;
            if (!expr.TryExtractProperty(out property))
                throw new ArgumentException("Requires a expression that has a property member access");

            return property;
        }


        public static string GetPropertyPath(this LambdaExpression lambdaExpression, bool jsonNameStyle = false)
        {
            if (lambdaExpression == null)
                throw new ArgumentNullException("lambdaExpression");
            var sb = new StringBuilder();

            var body = lambdaExpression.Body;
            GetPropertyPath(body, lambdaExpression.Parameters[0], sb, jsonNameStyle);
            return sb.ToString();
        }


        public static Expression MergePredicateWith(this Expression left, Expression right)
        {
            if (left.NodeType == ExpressionType.Quote && right.NodeType == ExpressionType.Quote)
            {
                return
                    Expression.Quote(MergePredicateWith(((UnaryExpression)left).Operand,
                                                        ((UnaryExpression)right).Operand));
            }
            if (left.NodeType == ExpressionType.Lambda && right.NodeType == ExpressionType.Lambda)
                return MergePredicateWith((LambdaExpression)left, (LambdaExpression)right);
            throw new NotImplementedException();
        }


        public static LambdaExpression MergePredicateWith(this LambdaExpression left, LambdaExpression right)
        {
            var param = Expression.Parameter(left.Parameters[0].Type, left.Parameters[0].Name);
            var leftBody = left.Body.Replace(left.Parameters[0], param);
            var rightBody = right.Body.Replace(right.Parameters[0], param);
            return Expression.Lambda(Expression.AndAlso(leftBody, rightBody), param);
        }


        public static Expression Replace(this Expression expr, Expression find, Expression replace)
        {
            return new FindAndReplaceVisitor(find, replace).Visit(expr);
        }


        public static bool TryExtractProperty(this Expression expression, out PropertyInfo property)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.Quote)
                expression = ((UnaryExpression)expression).Operand;
            if (expression.NodeType == ExpressionType.Lambda)
                return TryExtractProperty(((LambdaExpression)expression).Body, out property);
            property = null;
            return (expression.NodeType == ExpressionType.MemberAccess
                    && ((property) = ((MemberExpression)expression).Member as PropertyInfo) != null);
        }


        public static Expression Visit<TVisitor>(this Expression expression)
            where TVisitor : ExpressionVisitor, new()
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return new TVisitor().Visit(expression);
        }


        private static void GetPropertyPath(Expression expr,
                                            ParameterExpression thisParam,
                                            StringBuilder sb,
                                            bool jsonNameStyle)
        {
            var memberExpr = expr as MemberExpression;
            if (memberExpr == null)
            {
                var methodCallExpr = expr as MethodCallExpression;
                if (methodCallExpr != null && expandMethodTokens.Contains(methodCallExpr.Method.UniqueToken()))
                {
                    GetPropertyPath(methodCallExpr.Arguments[0], thisParam, sb, jsonNameStyle);
                    var innerExpr = methodCallExpr.Arguments[1];
                    if (innerExpr.NodeType == ExpressionType.Quote)
                        innerExpr = ((UnaryExpression)innerExpr).Operand;
                    var innerLambda = (LambdaExpression)innerExpr;
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
    }
}