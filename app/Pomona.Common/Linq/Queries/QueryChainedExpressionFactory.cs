#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
                        $"Unable to get default matching method for {GetType()}, should exist a public static field Method of type MethodInfo declared on type.");
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
                    $"Unable to locate private constructor with signature .ctor({paramTypes[0]}, {paramTypes[1]}) on type {typeof(TExpression)}");
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