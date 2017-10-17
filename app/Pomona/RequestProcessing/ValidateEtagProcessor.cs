﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public class ValidateEtagProcessor : IPomonaRequestProcessor
    {
        private string GetIfMatchFromRequest(PomonaContext context)
        {
            var ifMatch = context.RequestHeaders.SafeGet("If-Match")?.FirstOrDefault();
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


        private async Task<PomonaResponse> ProcessPatch(PomonaContext context, string ifMatch)
        {
            if (context.Method != HttpMethod.Patch)
                return null;
            return await ValidateResourceEtag(ifMatch, context.Node);
        }


        private async Task<PomonaResponse> ProcessPostToChildResourceRepository(PomonaContext context, string ifMatch)
        {
            var node = context.Node;
            var collectionType = node.ResultType as EnumerableTypeSpec;
            if (context.Method != HttpMethod.Post || collectionType == null)
                return null;

            var parentNode = node.Parent;
            if (parentNode != null)
                return await ValidateResourceEtag(ifMatch, parentNode);
            return null;
        }


        private static async Task<PomonaResponse> ValidateResourceEtag(string ifMatch, UrlSegment node)
        {
            var resourceType = node.ResultType as ResourceType;
            if (resourceType == null)
                return null;
            var etagProp = resourceType.ETagProperty;
            if (etagProp == null)
                throw new InvalidOperationException("Unable to perform If-Match on entity with no etag.");

            if ((string)etagProp.GetValue(await node.GetValueAsync()) != ifMatch)
                throw new ResourcePreconditionFailedException("Etag of entity did not match If-Match header.");
            return null;
        }


        public async Task<PomonaResponse> Process(PomonaContext context)
        {
            string ifMatch = null;
            if ((ifMatch = GetIfMatchFromRequest(context)) == null)
                return null;

            var pomonaResponse = await ProcessPatch(context, ifMatch) ?? await ProcessPostToChildResourceRepository(context, ifMatch);
            return pomonaResponse;
        }
    }
}