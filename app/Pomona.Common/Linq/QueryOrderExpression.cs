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