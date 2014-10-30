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


        public new MethodCallExpression Node
        {
            get { return (MethodCallExpression)base.Node; }
        }

        public QueryExpression Source
        {
            get
            {
                if (this.source == null)
                    this.source = Wrap(Arguments[0]);
                return this.source;
            }
        }

        protected ReadOnlyCollection<Expression> Arguments
        {
            get { return Node.Arguments; }
        }


        protected static Expression ConvertAndQuote(LambdaExpression origExpr, Type elementType)
        {
            var origParam = origExpr.Parameters.First();
            if (origParam.Type != elementType)
            {
                if (!origParam.Type.IsAssignableFrom(elementType))
                    throw new ArgumentException("Incompatible lambda expr, cannot rewrite", "origExpr");
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