#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.Queries
{
    public abstract class QueryMethodExpression : QueryExpression
    {
        private QueryExpression source;


        protected QueryMethodExpression(MethodCallExpression node,
                                        QueryExpression source)
            : base(node)
        {
            if (source != null)
            {
                if (source.Node != node.Arguments[0])
                    throw new ArgumentException("Node of source and first argument of MethodCall do not match.");
            }
            this.source = source;
        }


        public new MethodCallExpression Node => (MethodCallExpression)base.Node;

        public QueryExpression Source
        {
            get
            {
                if (this.source == null)
                    this.source = Wrap(Arguments[0]);
                return this.source;
            }
        }

        protected ReadOnlyCollection<Expression> Arguments => Node.Arguments;


        protected static Expression ConvertAndQuote(LambdaExpression origExpr, Type elementType)
        {
            var origParam = origExpr.Parameters.First();
            if (origParam.Type != elementType)
            {
                if (!origParam.Type.IsAssignableFrom(elementType))
                    throw new ArgumentException("Incompatible lambda expr, cannot rewrite", nameof(origExpr));
                var newParam = Parameter(elementType, origParam.Name);
                var newBody = origExpr.Body.Replace(origParam, newParam);
                if (!origExpr.ReturnType.IsAssignableFrom(newBody.Type))
                    newBody = Convert(newBody, origExpr.ReturnType);
                return Lambda(GetFuncType(elementType, origExpr.ReturnType), newBody, newParam);
            }
            return origExpr;
        }
    }
}
