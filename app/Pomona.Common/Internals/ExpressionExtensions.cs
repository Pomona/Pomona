// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Pomona.Common.Linq;
using Pomona.Internals;

namespace Pomona.Common.Internals
{
    public static class ExpressionExtensions
    {
        private static readonly MethodInfo enumerableExpandMethod;

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
                if (methodCallExpr != null &&
                    methodCallExpr.Method.UniqueToken() == enumerableExpandMethod.UniqueToken())
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