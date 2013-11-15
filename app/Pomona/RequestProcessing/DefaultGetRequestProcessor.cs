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

using Pomona.Common;
using Pomona.Queries;

namespace Pomona.RequestProcessing
{
    public class DefaultGetRequestProcessor : IPomonaRequestProcessor
    {
        private readonly IQueryExecutor queryExecutor;


        public DefaultGetRequestProcessor(IQueryExecutor queryExecutor)
        {
            if (queryExecutor == null)
                throw new ArgumentNullException("queryExecutor");
            this.queryExecutor = queryExecutor;
        }


        public PomonaResponse Process(PomonaRequest request)
        {
            if (request.Method != HttpMethod.Get)
                return null;

            var queryableNode = request.Node as QueryableNode;
            if (queryableNode != null)
            {
                var pomonaQuery = request.ParseQuery();
                return this.queryExecutor.ApplyAndExecute(queryableNode.GetAsQueryable(pomonaQuery.OfType), pomonaQuery);
            }
            var resourceNode = request.Node as ResourceNode;
            if (resourceNode != null)
                return new PomonaResponse(resourceNode.Value, expandedPaths : request.ExpandedPaths);
            return null;
        }
    }
}