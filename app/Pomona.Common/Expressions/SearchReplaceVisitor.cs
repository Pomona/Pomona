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

using Pomona.Common.Internals;

namespace Pomona.Common.Expressions
{
    public class SearchReplaceVisitor : ExpressionVisitor
    {
        private readonly List<TreePatternMatcher> matchers;


        private SearchReplaceVisitor(IEnumerable<TreePatternMatcher> matchers)
        {
            if (matchers == null)
                throw new ArgumentNullException("matchers");
            this.matchers = matchers.ToList();
        }


        public static Builder Create()
        {
            return new Builder(null, null);
        }


        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            foreach (var matcher in this.matchers)
                node = matcher.MatchAndRewrite(node);
            return base.Visit(node);
        }

        #region Nested type: Builder

        public class Builder
        {
            protected Func<TreePatternMatcher> matcher;
            protected Builder next;


            public Builder(Builder next, Func<TreePatternMatcher> matcher)
            {
                this.next = next;
                this.matcher = matcher;
            }


            public SearchReplaceVisitor Build()
            {
                return
                    new SearchReplaceVisitor(
                        this.WalkTree(x => x.next).Select(x => x.matcher).Where(x => x != null).Select(x => x()));
            }


            public Builder<T> For<T>()
            {
                return new Builder<T>(this, null);
            }
        }

        public class Builder<T> : Builder
        {
            public Builder(Builder next, Func<TreePatternMatcher> matcher)
                : base(next, matcher)
            {
            }


            public Builder<T> Replace<TRet>(Expression<Func<T, TRet>> searchFor, Expression<Func<T, TRet>> replaceWith)
            {
                return new Builder<T>(this, () => TreePatternMatcher.FromLambda(searchFor, replaceWith));
            }


            public Builder<T> Replace<TRet>(Expression<Func<T, IMatchContext, TRet>> searchFor,
                                            Expression<Func<T, IMatchContext, TRet>> replaceWith)
            {
                return new Builder<T>(this, () => TreePatternMatcher.FromLambda(searchFor, replaceWith));
            }
        }

        #endregion
    }
}