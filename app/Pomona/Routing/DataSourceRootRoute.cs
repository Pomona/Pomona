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

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class DataSourceRootRoute : Route
    {
        private readonly Type dataSource;
        private readonly TypeMapper typeMapper;


        public DataSourceRootRoute(TypeMapper typeMapper, Type dataSource)
            : base(0, null)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            var dataSourceInterface = typeof(IPomonaDataSource);
            dataSource = dataSource ?? dataSourceInterface;

            if (!dataSourceInterface.IsAssignableFrom(dataSource))
            {
                throw new ArgumentException(string.Format("dataSourceType must be castable to {0}",
                                                          dataSourceInterface.FullName));
            }

            this.typeMapper = typeMapper;
            this.dataSource = dataSource;
        }


        internal Type DataSource
        {
            get { return this.dataSource; }
        }

        public override HttpMethod AllowedMethods
        {
            get { return HttpMethod.Get; }
        }

        public override TypeSpec InputType
        {
            get { return this.typeMapper.FromType(typeof(void)); }
        }

        public override TypeSpec ResultType
        {
            get { return this.typeMapper.FromType(typeof(IDictionary<string, object>)); }
        }

        internal TypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }


        protected override IEnumerable<Route> LoadChildren()
        {
            return GetRootResourceBaseTypes().Select(x => new DataSourceCollectionRoute(this, x));
        }


        protected override bool Match(string pathSegment)
        {
            throw new NotSupportedException("Root route only supports MatchChildren()");
        }


        protected override string PathSegmentToString()
        {
            return string.Empty;
        }


        internal IEnumerable<ResourceType> GetRootResourceBaseTypes()
        {
            return this.typeMapper.TransformedTypes.OfType<ResourceType>()
                .Where(x => x.IsUriBaseType && x.ParentResourceType == null);
        }
    }
}