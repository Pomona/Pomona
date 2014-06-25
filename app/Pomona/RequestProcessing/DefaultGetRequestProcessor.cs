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

using System;

using Pomona.Common;

namespace Pomona.RequestProcessing
{
    public class DefaultGetRequestProcessor : IPomonaRequestProcessor
    {
        public PomonaResponse Process(PomonaRequest request)
        {
            if (request.Method != HttpMethod.Get)
                return null;

            var resourceNode = request.Node as ResourceNode;
            if (resourceNode != null)
            {
                var parentCollectionNode = resourceNode.Parent as ResourceCollectionNode;
                object value = null;
                if (parentCollectionNode != null)
                {
                    value = parentCollectionNode.GetItemFromQueryableById(resourceNode.Name);
                }
                if (value == null)
                {
                    throw new ResourceNotFoundException("Resource not found.");
                }
                return new PomonaResponse(value, expandedPaths : request.ExpandedPaths);
            }

            var collectionNode = request.Node as ResourceCollectionNode;
            if (collectionNode != null)
            {
                if (request.ExecuteQueryable)
                {
                    if (!request.HasQuery)
                        throw new InvalidOperationException("Can't execute query when HasQuery is false (TODO: CLEAN UP THIS PROPER).");
                    var pomonaQuery = request.ParseQuery();
                    return request.Node.GetQueryExecutor()
                        .ApplyAndExecute(collectionNode.GetAsQueryable(pomonaQuery.OfType), pomonaQuery);
                }
                return new PomonaResponse(collectionNode.GetAsQueryable());
            }
            return null;
        }
    }
}