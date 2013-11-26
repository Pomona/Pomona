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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
                ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Expand(y => 0));
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


        private class ExpressionToCsharpVisitor : ExpressionVisitor
        {
            private StringBuilder sb = new StringBuilder();


            private void AppendType(Type type)
            {
                if (type.IsGenericType)
                {
                    if (type.IsGenericTypeDefinition)
                        throw new NotImplementedException();
                    sb.Append(type.Name.Split('`')[0]);
                    sb.Append("<");
                    bool first = true;
                    foreach (var genarg in type.GetGenericArguments())
                    {
                        if (first)
                            first = false;
                        else
                        {
                            sb.Append(", ");
                        }
                        AppendType(genarg);
                    }
                    sb.Append(">");
                }
                else
                {
                    sb.Append(type.Name);
                }
            }


            private static Dictionary<ExpressionType, string> infixMap = new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Add, "+" },
                { ExpressionType.Subtract, "-" },
                { ExpressionType.Multiply, "*" },
                { ExpressionType.Divide, "/" },
                { ExpressionType.AndAlso, "&&" },
                { ExpressionType.OrElse, "||" },
                { ExpressionType.And, "&" },
                { ExpressionType.Or, "|" },
                { ExpressionType.ExclusiveOr, "^" },
                { ExpressionType.Equal, "==" },
                { ExpressionType.GreaterThan, ">" },
                { ExpressionType.GreaterThanOrEqual, ">=" },
                { ExpressionType.LessThan, "<" },
                { ExpressionType.LessThanOrEqual, "<=" },
            };

            protected override Expression VisitBinary(BinaryExpression node)
            {
                string opSymbol;
                if (infixMap.TryGetValue(node.NodeType, out opSymbol))
                {
                    sb.Append("(");
                    sb.Append(Visit(node.Left));
                    sb.AppendFormat(" {0} ", opSymbol);
                    sb.Append(Visit(node.Right));
                    sb.Append(")");
                    return node;
                }
                return base.VisitBinary(node);
            }


            public override Expression Visit(Expression node)
            {
                var oldSbLength = sb.Length;
                var result = base.Visit(node);
                if (oldSbLength == sb.Length)
                {
                    Console.WriteLine("UNHANDLED NODE!!" + node.NodeType);
                }
                return result;
            }


            protected override Expression VisitParameter(ParameterExpression node)
            {
                sb.Append(node);
                return base.VisitParameter(node);
            }


            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                sb.Append("(");
                VisitArguments(node.Parameters);
                sb.Append(")");
                sb.Append(" => ");
                sb.Append("(");
                Visit(node.Body);
                sb.Append(")");
                return node;
            }


            protected override Expression VisitMember(MemberExpression node)
            {
                var propInfo = node.Member as PropertyInfo;
                if (propInfo != null)
                {
                    if ((propInfo.GetGetMethod(true) ?? propInfo.GetSetMethod(true)).IsStatic)
                    {
                        AppendType(propInfo.DeclaringType);
                    }
                    else
                    {
                        Visit(node.Expression);
                    }
                    sb.Append(".");
                    sb.Append(propInfo.Name);
                    return node;
                }

                var fieldInfo = node.Member as FieldInfo;
                if (fieldInfo != null)
                {
                    if (fieldInfo.IsStatic)
                    {
                        AppendType(fieldInfo.DeclaringType);
                    }
                    else
                    {
                        Visit(node.Expression);
                    }
                    sb.Append(".");
                    sb.Append(fieldInfo.Name);
                    return node;
                }

                throw new InvalidOperationException("Don't know what to do with member that is neither a property nor a field.");
            }


            protected override Expression VisitNew(NewExpression node)
            {
                sb.Append("new ");
                sb.Append(node.Constructor.DeclaringType.Name);
                sb.Append("(");

                VisitArguments(node.Arguments);
                sb.Append(")");
                return node;
            }


            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.IsStatic)
                {
                    AppendType(node.Method.DeclaringType);
                }
                else
                {
                    Visit(node.Object);
                }
                sb.Append(".");
                sb.Append(node.Method.Name);
                sb.Append("(");

                IEnumerable<Expression> args = node.Arguments;
                VisitArguments(args);
                sb.Append(")");
                return node;
            }


            private void VisitArguments(IEnumerable<Expression> args)
            {
                bool first = true;
                foreach (var arg in args)
                {
                    if (first)
                        first = false;
                    else
                        this.sb.Append(", ");
                    Visit(arg);
                }
            }


            public override string ToString()
            {
                return sb.ToString();
            }
        }

        public static string ToCsharpString(this Expression expression)
        {
            var visitor = new ExpressionToCsharpVisitor();
            visitor.Visit(expression);
            return visitor.ToString();
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