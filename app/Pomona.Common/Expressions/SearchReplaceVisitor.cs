#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
                throw new ArgumentNullException(nameof(matchers));
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

