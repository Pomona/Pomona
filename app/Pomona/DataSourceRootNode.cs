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
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Queries;
using Pomona.RequestProcessing;

namespace Pomona
{
    public class DataSourceRootNode : PathNode
    {
        private static readonly Func<Type, IPomonaDataSource, IQueryable> queryMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<IQueryable>(x => x.Query<object>());

        private readonly IPomonaDataSource dataSource;
        private HttpMethod allowedMethods;


        public DataSourceRootNode(ITypeMapper typeMapper, IPomonaDataSource dataSource,string rootPath)
            : base(typeMapper, null, rootPath, PathNodeType.Custom)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.dataSource = dataSource;
        }


        public override bool Exists
        {
            get { return true; }
        }

        public override bool IsLoaded
        {
            get { return true; }
        }

        public override object Value
        {
            get { return this.dataSource; }
        }

        public override HttpMethod AllowedMethods
        {
            get { return HttpMethod.Get; }
        }



        public override PathNode GetChildNode(string name, IPomonaContext context, IRequestProcessorPipeline pipeline)
        {
            var type = GetRootResourceBaseTypes()
                .FirstOrDefault(x => string.Equals(x.UriRelativePath, name, StringComparison.InvariantCultureIgnoreCase));

            if (type == null)
                throw new ResourceNotFoundException("Unable to locate root resource.");

            return CreateNode(TypeMapper,
                this,
                name,
                x => queryMethodInvoker(type, this.dataSource),
                TypeMapper.GetClassMapping(typeof(ICollection<>).MakeGenericType(type.Type)));
        }


        internal IEnumerable<ResourceType> GetRootResourceBaseTypes()
        {
            return ((TypeMapper)TypeMapper).TransformedTypes.OfType<ResourceType>()
                .Where(x => x.IsUriBaseType && x.IsRootResource);
        }


        public override IQueryExecutor GetQueryExecutor()
        {
            return this.dataSource as IQueryExecutor ?? base.GetQueryExecutor();
        }


        protected override IQueryableResolver GetQueryableResolver()
        {
            return new DataSourceQueryableResolver(this.dataSource);
        }


        protected override IPomonaRequestProcessor OnGetRequestProcessor(PomonaRequest request)
        {
            return new DataSourceRequestProcessor(this.dataSource);
        }


        protected override TypeSpec OnGetType()
        {
            return null;
        }
    }
}