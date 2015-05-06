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

namespace Pomona.Common.Linq
{
    internal class QuerySegmentParenScopeExpression : QuerySegmentExpression
    {
        private readonly QuerySegmentExpression value;


        public QuerySegmentParenScopeExpression(QuerySegmentExpression value)
            : base(value.Type)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            this.value = value;
        }


        public override ReadOnlyCollection<object> Children
        {
            get { return new ReadOnlyCollection<object>(new object[] { this.value }); }
        }

        public QuerySegmentExpression Value
        {
            get { return this.value; }
        }


        public override IEnumerable<string> ToStringSegments()
        {
            // Remove redundant parenthesis
            var valueAsParenScope = this.value as QuerySegmentParenScopeExpression;
            if (valueAsParenScope != null)
                return valueAsParenScope.ToStringSegments();
            return ToStringSegmentsInner();
        }


        private IEnumerable<string> ToStringSegmentsInner()
        {
            yield return "(";
            foreach (var child in this.value.ToStringSegments())
                yield return child;
            yield return ")";
        }
    }
}