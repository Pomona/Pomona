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
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public class ValidateEtagProcessor : IPomonaRequestProcessor
    {
        public PomonaResponse Process(PomonaContext context)
        {
            string ifMatch = null;
            if ((ifMatch = GetIfMatchFromRequest(context)) == null)
                return null;

            return ProcessPatch(context, ifMatch) ?? ProcessPostToChildResourceRepository(context, ifMatch);
        }


        private static PomonaResponse ValidateResourceEtag(string ifMatch, UrlSegment node)
        {
            var resourceType = node.ResultType as ResourceType;
            if (resourceType == null)
                return null;
            var etagProp = resourceType.ETagProperty;
            if (etagProp == null)
                throw new InvalidOperationException("Unable to perform If-Match on entity with no etag.");

            if ((string)etagProp.GetValue(node.Value) != ifMatch)
                throw new ResourcePreconditionFailedException("Etag of entity did not match If-Match header.");
            return null;
        }


        private string GetIfMatchFromRequest(PomonaContext context)
        {
            var ifMatch = context.Headers.IfMatch.FirstOrDefault();
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


        private PomonaResponse ProcessPatch(PomonaContext context, string ifMatch)
        {
            if (context.Method != HttpMethod.Patch)
                return null;
            return ValidateResourceEtag(ifMatch, context.Node);
        }


        private PomonaResponse ProcessPostToChildResourceRepository(PomonaContext context, string ifMatch)
        {
            var node = context.Node;
            var collectionType = node.ResultType as EnumerableTypeSpec;
            if (context.Method != HttpMethod.Post || collectionType == null)
                return null;

            var parentNode = node.Parent;
            if (parentNode != null)
                return ValidateResourceEtag(ifMatch, parentNode);
            return null;
        }
    }
}