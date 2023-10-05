#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pomona.Common.Linq
{
    internal class QuerySegmentListExpression : QuerySegmentExpression
    {
        private readonly object[] children;


        public QuerySegmentListExpression(IEnumerable<object> children, Type type)
            : base(type)
        {
            if (children == null)
                throw new ArgumentNullException(nameof(children));
            this.children = children as object[] ?? children.ToArray();
        }


        public override ReadOnlyCollection<object> Children => new ReadOnlyCollection<object>(this.children);


        public override IEnumerable<string> ToStringSegments()
        {
            return ToStringSegmentsRecursive(this.children);
        }
    }
}

