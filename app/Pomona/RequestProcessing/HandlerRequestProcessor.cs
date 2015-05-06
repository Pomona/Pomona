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
using System.Linq;
using System.Reflection;

using Pomona.Common;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public abstract class HandlerRequestProcessor : IRouteActionResolver
    {
        public static HandlerRequestProcessor Create(Type type)
        {
            return
                (HandlerRequestProcessor)
                    Activator.CreateInstance(typeof(HandlerRequestProcessor<>).MakeGenericType(type));
        }


        public abstract IEnumerable<RouteAction> Resolve(Route route, HttpMethod method);
    }

    public class HandlerRequestProcessor<THandler> : HandlerRequestProcessor
    {
        public IEnumerable<HandlerMethod> GetHandlerMethods(TypeMapper mapper)
        {
            return typeof(THandler).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                   .Select(x => new HandlerMethod(x, mapper));
        }


        public override IEnumerable<RouteAction> Resolve(Route route, HttpMethod method)
        {
            var handlerMethodInvokers = ResolveHandlerMethods(route, method);
            return handlerMethodInvokers;
        }


        private IEnumerable<RouteAction> ResolveHandlerMethods(Route route, HttpMethod method)
        {
            var resourceType = route.ResultItemType;
            var typeSpec = resourceType;
            return
                GetHandlerMethods((TypeMapper)typeSpec.TypeResolver).Select(
                    x => x.Match(method, route.NodeType, typeSpec)).Where(x => x != null).ToList();
        }
    }
}