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

using System.Linq;

using Pomona.Common.Linq;
using Pomona.Common.Linq.NonGeneric;
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    internal class QueryableActionResult<T> : WrappedQueryableBase<T>, IQueryableActionResult<T>
    {
        private readonly QueryableProjection defaultProjection;
        private readonly TypeSpec resultType;


        internal QueryableActionResult(IQueryable<T> innerQueryable,
                                       TypeSpec resultType,
                                       QueryableProjection defaultProjection)
            : base(innerQueryable)
        {
            this.resultType = resultType;
            this.defaultProjection = defaultProjection;
        }


        public QueryableProjection DefaultProjection
        {
            get { return this.defaultProjection; }
        }

        public bool IsScalar
        {
            get { return this.resultType != null && this.resultType.IsCollection; }
        }

        public TypeSpec ResultType
        {
            get { return this.resultType; }
        }
    }
}