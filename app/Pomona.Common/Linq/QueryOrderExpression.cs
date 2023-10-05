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
    internal class QueryOrderExpression : QuerySegmentExpression
    {
        private readonly IEnumerable<Tuple<PomonaExtendedExpression, SortOrder>> selectors;
        private ReadOnlyCollection<object> children;


        public QueryOrderExpression(IEnumerable<Tuple<PomonaExtendedExpression, SortOrder>> selectors, Type type)
            : base(type)
        {
            this.selectors = selectors.ToList();
        }


        public override ReadOnlyCollection<object> Children
        {
            get
            {
                if (this.children == null)
                    this.children = new ReadOnlyCollection<object>(GetChildren().ToList());
                return this.children;
            }
        }


        public override IEnumerable<string> ToStringSegments()
        {
            return ToStringSegmentsRecursive(GetChildren());
        }


        private IEnumerable<object> GetChildren()
        {
            int i = 0;
            foreach (var kvp in this.selectors)
            {
                if (i != 0)
                    yield return ",";
                yield return kvp.Item1;
                if (kvp.Item2 == SortOrder.Descending)
                    yield return " desc";
                i++;
            }
        }
    }
}

