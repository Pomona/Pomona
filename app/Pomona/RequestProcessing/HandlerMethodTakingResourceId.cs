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

using Nancy.Routing;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingResourceId : HandlerMethodInvoker<object>
    {
        public HandlerMethodTakingResourceId(HandlerMethod method)
            : base(method)
        {
        }


        protected override object OnGetArgument(HandlerParameter parameter, PomonaRequest request, object state)
        {
            var node = request.Node;
            var resourceResultType = node.Route.ResultItemType as ResourceType;
            if (resourceResultType != null)
            {
                var primaryIdType = resourceResultType.PrimaryId.PropertyType;
                if (parameter.Type == primaryIdType)
                {
                    object parsedId;
                    if (!node.PathSegment.TryParse(primaryIdType, out parsedId))
                    {
                        throw new HandlerMethodInvocationException(request, this, "Unable to parse id from url segment");
                    }
                    return parsedId;
                }
            }
            return base.OnGetArgument(parameter, request, state);
        }
    }
}