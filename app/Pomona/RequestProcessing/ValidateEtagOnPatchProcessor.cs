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
using System.Linq;

using Pomona.Common;

namespace Pomona.RequestProcessing
{
    public class ValidateEtagProcessor : IPomonaRequestProcessor
    {
        public PomonaResponse Process(PomonaRequest request)
        {
            string ifMatch = null;
            if ((ifMatch = GetIfMatchFromRequest(request)) == null)
                return null;

            if (ProcessPatch(request, ifMatch))
                return null;
            ProcessPostToChildResourceRepository(request, ifMatch);
            return null;
        }


        private static bool ValidateResourceEtag(string ifMatch, ResourceNode resourceNode)
        {
            var resourceType = resourceNode.Type;
            var etagProp = resourceType.ETagProperty;
            if (etagProp == null)
                throw new InvalidOperationException("Unable to perform If-Match on entity with no etag.");

            if ((string)etagProp.Getter(resourceNode.Value) != ifMatch)
                throw new ResourcePreconditionFailedException("Etag of entity did not match If-Match header.");
            return true;
        }


        private string GetIfMatchFromRequest(PomonaRequest request)
        {
            var ifMatch = request.Headers.IfMatch.FirstOrDefault();
            if (ifMatch != null)
            {
                ifMatch = ifMatch.Trim();
                if (ifMatch.Length < 2 || ifMatch[0] != '"' || ifMatch[ifMatch.Length - 1] != '"')
                {
                    throw new NotImplementedException(
                        "Only recognized If-Match with quotes around, * not yet supported (TODO).");
                }

                ifMatch = ifMatch.Substring(1, ifMatch.Length - 2);
            }
            return ifMatch;
        }


        private bool ProcessPatch(PomonaRequest request, string ifMatch)
        {
            var resourceNode = request.Node as ResourceNode;
            if (resourceNode == null || request.Method != HttpMethod.Patch)
                return false;
            return ValidateResourceEtag(ifMatch, resourceNode);
        }


        private void ProcessPostToChildResourceRepository(PomonaRequest request, string ifMatch)
        {
            var queryableNode = request.Node as QueryableNode;
            if (request.Method != HttpMethod.Post || queryableNode == null)
                return;

            var parentNode = queryableNode.Parent as ResourceNode;
            if (parentNode != null)
                ValidateResourceEtag(ifMatch, parentNode);
        }
    }
}