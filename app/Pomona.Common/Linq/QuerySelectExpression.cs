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
    internal class QuerySelectExpression : QuerySegmentExpression
    {
        private readonly IEnumerable<KeyValuePair<string, PomonaExtendedExpression>> selectList;
        private ReadOnlyCollection<object> children;


        public QuerySelectExpression(IEnumerable<KeyValuePair<string, PomonaExtendedExpression>> selectList, Type type)
            : base(type)
        {
            this.selectList = selectList;
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
            foreach (var kvp in this.selectList)
            {
                if (i != 0)
                    yield return ",";
                yield return kvp.Value;
                yield return " as ";
                yield return kvp.Key;
                i++;
            }
        }
    }
}

