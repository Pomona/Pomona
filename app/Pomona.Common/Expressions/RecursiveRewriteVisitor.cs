#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Expressions
{
    public class RecursiveRewriteVisitor : ExpressionVisitor, IRewriteContext
    {
        private readonly ReadOnlyCollection<IExpressionRewriter> rewriters;


        public RecursiveRewriteVisitor(params IExpressionRewriter[] rewriters)
            : this((IEnumerable<IExpressionRewriter>)rewriters)
        {
        }


        public RecursiveRewriteVisitor(IEnumerable<IExpressionRewriter> rewriters)
            : this(rewriters != null ? rewriters.ToList().AsReadOnly() : null)
        {
        }


        public RecursiveRewriteVisitor(ReadOnlyCollection<IExpressionRewriter> rewriters)
        {
            if (rewriters == null)
                throw new ArgumentNullException(nameof(rewriters));
            this.rewriters = rewriters;
        }


        public override Expression Visit(Expression node)
        {
            var visited = node;
            foreach (var rewriter in this.rewriters)
            {
                visited = rewriter.Visit(this, visited);
                if (visited != node)
                {
                    // Visit children
                    visited = base.Visit(visited);
                    return Visit(visited);
                }
            }
            return base.Visit(node);
        }
    }
}

