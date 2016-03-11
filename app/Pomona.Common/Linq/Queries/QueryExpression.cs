#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
            Node = node;
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

        public Expression Node { get; }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { return Node.Type; }
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
            return Node.ToString();
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