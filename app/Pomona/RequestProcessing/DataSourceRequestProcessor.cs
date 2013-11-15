#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using System.Reflection;

using Nancy;

using Pomona.Common;
using Pomona.Internals;

namespace Pomona.RequestProcessing
{
    public class DataSourceRequestProcessor : IPomonaRequestProcessor
    {
        private readonly IPomonaDataSource dataSource;

        private readonly MethodInfo postMethod =
            ReflectionHelper.GetMethodDefinition<DataSourceRequestProcessor>(x => x.Post<object>(null, null));


        public DataSourceRequestProcessor(IPomonaDataSource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.dataSource = dataSource;
        }


        public virtual PomonaResponse Process(PomonaRequest request)
        {
            var queryableNode = request.Node as QueryableNode;
            if (request.Method == HttpMethod.Post && queryableNode != null
                && queryableNode.ItemResourceType.IsRootResource)
            {
                var form = request.Bind();
                return
                    (PomonaResponse)
                        this.postMethod.MakeGenericMethod(form.GetType()).Invoke(this, new[] { form, request });
            }
            return null;
        }


        public PomonaResponse Post<T>(T form, PomonaRequest request)
            where T : class
        {
            return new PomonaResponse(this.dataSource.Post(form), HttpStatusCode.Created, request.ExpandedPaths);
        }
    }
}