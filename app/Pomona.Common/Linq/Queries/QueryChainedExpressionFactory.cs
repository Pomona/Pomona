#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Linq.Queries
{
    internal abstract class QueryChainedExpressionFactory<TExpression> : QueryExpressionFactory
        where TExpression : QueryChainedExpression
    {
        private readonly ReadOnlyCollection<MethodInfo> mappedMethods;
        private Func<MethodCallExpression, QueryExpression, TExpression> constructor;


        protected QueryChainedExpressionFactory(IEnumerable<MethodInfo> methods = null)
        {
            if (methods == null)
            {
                var methodField = typeof(TExpression).GetField("Method",
                                                               BindingFlags.Static | BindingFlags.Public
                                                               | BindingFlags.DeclaredOnly);
                if (methodField == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Unable to get default matching method for {0}, should exist a public static field Method of type MethodInfo declared on type.",
                            GetType()));
                }
                MethodInfo method = (MethodInfo)methodField.GetValue(null);
                if (method == null)
                    throw new InvalidOperationException("No method returned, wtf?");
                this.mappedMethods = new ReadOnlyCollection<MethodInfo>(new[] { method });
            }
            else
                this.mappedMethods = new List<MethodInfo>(methods).AsReadOnly();
        }


        public virtual IEnumerable<MethodInfo> MappedMethods
        {
            get { return this.mappedMethods; }
        }

        protected Func<MethodCallExpression, QueryExpression, TExpression> Constructor
        {
            get { return this.constructor ?? (this.constructor = FindConstructor()); }
        }


        public override sealed bool TryWrapNode(Expression node, out QueryExpression wrapper)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            wrapper = null;
            var methodCallExpr = node as MethodCallExpression;
            if (methodCallExpr == null || !this.mappedMethods.Any(x => methodCallExpr.Method.IsGenericInstanceOf(x)))
                return false;
            wrapper = OnWrapNode(methodCallExpr);
            return wrapper != null;
        }


        protected virtual TExpression Create(MethodCallExpression node, QueryExpression source)
        {
            return Constructor(node, source);
        }


        protected virtual QueryExpression OnWrapNode(MethodCallExpression node)
        {
            return Constructor(node, null);
        }


        private static Func<MethodCallExpression, QueryExpression, TExpression> FindConstructor()
        {
            Type[] paramTypes = { typeof(MethodCallExpression), typeof(QueryExpression) };
            var ctor = typeof(TExpression).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                                                          null,
                                                          paramTypes,
                                                          null);
            if (ctor == null)
            {
                throw new MissingMethodException(
                    string.Format(
                        "Unable to locate private constructor with signature .ctor({0}, {1}) on type {2}",
                        paramTypes[0],
                        paramTypes[1],
                        typeof(TExpression)));
            }

            var nodeParam = Expression.Parameter(paramTypes[0]);
            var sourceParam = Expression.Parameter(paramTypes[1]);

            return Expression.Lambda<Func<MethodCallExpression, QueryExpression, TExpression>>(
                Expression.New(ctor, nodeParam, sourceParam),
                nodeParam,
                sourceParam).Compile();
        }
    }
}