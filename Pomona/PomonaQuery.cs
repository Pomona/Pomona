#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Linq.Expressions;

using Pomona.Client;

namespace Pomona
{
    /// <summary>
    /// A default implementation of IPomonaQuery, only simple querying.
    /// </summary>
    public class PomonaQuery : IPomonaQuery
    {
        private readonly TransformedType targetType;


        public PomonaQuery(TransformedType targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            this.targetType = targetType;
        }


        public LambdaExpression FilterExpression { get; set; }

        public LambdaExpression OrderByExpression { get; set; }

        public int Skip { get; set; }
        public SortOrder SortOrder { get; set; }
        public int Top { get; set; }

        #region IPomonaQuery Members

        public IEnumerable<string> ExpandedPaths { get; set; }

        public TransformedType TargetType
        {
            get { return this.targetType; }
        }

        public Uri Url { get; set; }

        #endregion
    }
}