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
                throw new ArgumentNullException("rewriters");
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