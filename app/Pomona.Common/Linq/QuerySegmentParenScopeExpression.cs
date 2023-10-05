#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pomona.Common.Linq
{
    internal class QuerySegmentParenScopeExpression : QuerySegmentExpression
    {
        public QuerySegmentParenScopeExpression(QuerySegmentExpression value)
            : base(value.Type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            Value = value;
        }


        public override ReadOnlyCollection<object> Children => new ReadOnlyCollection<object>(new object[] { Value });

        public QuerySegmentExpression Value { get; }


        public override IEnumerable<string> ToStringSegments()
        {
            // Remove redundant parenthesis
            var valueAsParenScope = Value as QuerySegmentParenScopeExpression;
            if (valueAsParenScope != null)
                return valueAsParenScope.ToStringSegments();
            return ToStringSegmentsInner();
        }


        private IEnumerable<string> ToStringSegmentsInner()
        {
            yield return "(";
            foreach (var child in Value.ToStringSegments())
                yield return child;
            yield return ")";
        }
    }
}
