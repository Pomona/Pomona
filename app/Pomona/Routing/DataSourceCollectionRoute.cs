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

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class DataSourceCollectionRoute : Route, ILiteralRoute
    {
        private readonly DataSourceRootRoute parent;
        private readonly ResourceType resultItemType;
        private readonly TypeSpec resultType;


        public DataSourceCollectionRoute(DataSourceRootRoute parent, ResourceType resultItemType)
            : base(0, parent)
        {
            if (resultItemType == null)
                throw new ArgumentNullException(nameof(resultItemType));
            this.parent = parent;
            this.resultItemType = resultItemType;
            this.resultType = parent.TypeMapper.FromType(typeof(IEnumerable<>).MakeGenericType(resultItemType));
        }


        public override HttpMethod AllowedMethods
        {
            get { return (this.resultItemType.PostAllowed ? HttpMethod.Post : 0) | HttpMethod.Get; }
        }

        public override TypeSpec InputType
        {
            get { return this.parent.TypeMapper.FromType(typeof(void)); }
        }

        public override TypeSpec ResultItemType
        {
            get { return this.resultItemType; }
        }

        public override TypeSpec ResultType
        {
            get { return this.resultType; }
        }


        protected override IEnumerable<Route> LoadChildren()
        {
            return new GetByIdRoute(this.resultItemType, this, this.resultItemType.AllowedMethods | HttpMethod.Post).WrapAsArray();
        }


        protected override bool Match(string pathSegment)
        {
            return String.Equals(pathSegment, MatchValue, StringComparison.InvariantCultureIgnoreCase);
        }


        protected override string PathSegmentToString()
        {
            return this.resultItemType.UrlRelativePath;
        }


        public string MatchValue
        {
            get { return this.resultItemType.UrlRelativePath; }
        }
    }
}