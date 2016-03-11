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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Linq.Queries
{
    public abstract class QueryExpression : Expression
    {
        private static readonly Lazy<QueryExpressionFactory[]> factories;
        private readonly Expression node;


        static QueryExpression()
        {
            factories =
                new Lazy<QueryExpressionFactory[]>(
                    () => QueryExpressionTypes.Select(GetFactory).Where(x => x != null).ToArray());
        }


        public QueryExpression(Expression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            this.node = node;
        }


        public override bool CanReduce
        {
            get { return true; }
        }

        public Type ElementType
        {
            get
            {
                Type[] typeArgs;
                if (!Type.TryExtractTypeArguments(typeof(IQueryable<>), out typeArgs))
                    throw new InvalidOperationException("Type is not a IQueryable<T>, unable to extract element type.");
                return typeArgs[0];
            }
        }

        public Expression Node
        {
            get { return this.node; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { return this.node.Type; }
        }

        private static List<Type> QueryExpressionTypes
        {
            get
            {
                return typeof(QueryExpression).Assembly
                                              .GetTypes()
                                              .Where(x => typeof(QueryExpression).IsAssignableFrom(x) && x.IsPublic && !x.IsAbstract)
                                              .ToList();
            }
        }


        public override Expression Reduce()
        {
            return Node;
        }


        public override string ToString()
        {
            return this.node.ToString();
        }


        public static bool TryWrap(Expression expression, out QueryExpression node)
        {
            node = WrapOrNull(expression);
            return node != null;
        }


        public static bool TryWrap<TExpression>(Expression expression, out TExpression node)
            where TExpression : QueryExpression
        {
            node = WrapOrNull(expression) as TExpression;
            return node != null;
        }


        public static QueryExpression Wrap(Expression expression)
        {
            var qn = WrapOrNull(expression);
            if (qn == null)
                throw new ArgumentException("Expression not recognized as queryable extension method", nameof(expression));
            return qn;
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }


        private static QueryExpressionFactory GetFactory(Type exprType)
        {
            var factoryProp = exprType.GetProperty("Factory",
                                                   BindingFlags.Public | BindingFlags.Static
                                                   | BindingFlags.DeclaredOnly);
            if (factoryProp == null)
                return null;
            return factoryProp.GetValue(null, null) as QueryExpressionFactory;
        }


        private static QueryExpression WrapOrNull(Expression expression)
        {
            return factories.Value.Select(x => x.WrapOrNull(expression)).FirstOrDefault(x => x != null);
        }
    }
}