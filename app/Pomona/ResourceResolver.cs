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
using System.Threading;
using System.Threading.Tasks;

using Nancy;
using Nancy.Routing;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ResourceResolver : IResourceResolver
    {
        private readonly NancyContext context;
        private readonly IRouteResolver routeResolver;
        private readonly ITypeMapper typeMapper;


        public ResourceResolver(ITypeMapper typeMapper, NancyContext context, IRouteResolver routeResolver)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (context == null)
                throw new ArgumentNullException("context");
            if (routeResolver == null)
                throw new ArgumentNullException("routeResolver");
            //if (routeResolver == null) throw new ArgumentNullException("routeResolver");
            this.typeMapper = typeMapper;
            this.context = context;
            this.routeResolver = routeResolver;
        }


        public NancyContext Context
        {
            get { return this.context; }
        }

        public ITypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }

        private Request Request
        {
            get { return this.context.Request; }
        }


        public object ResolveUri(string uriString)
        {
            var uri = new Uri(uriString, UriKind.Absolute);

            var modulePath = uri.AbsolutePath;
            var basePath = Request.Url.BasePath ?? String.Empty;
            if (modulePath.StartsWith(basePath))
                modulePath = modulePath.Substring(basePath.Length);

            var url = Request.Url.Clone();
            url.Path = modulePath;
            url.Query = uri.Query;

            var innerRequest = new Request("GET", url, ip : Context.Request.UserHostAddress);
            var innerContext = new NancyContext
            {
                Culture = Context.Culture,
                CurrentUser = Context.CurrentUser,
                Request = innerRequest,
            };
            Context.Items.Where(x => x.Key.StartsWith("Pomona.")).AddTo(innerContext.Items);

            var routeMatch = this.routeResolver.Resolve(innerContext);
            var route = routeMatch.Route;
            var dynamicDict = routeMatch.Parameters;

            var pomonaResponse =
                (PomonaResponse)((Task<dynamic>)route.Action((dynamic)dynamicDict, CancellationToken.None)).Result;

            if ((int)pomonaResponse.StatusCode >= 400)
                throw new ReferencedResourceNotFoundException(uriString, pomonaResponse);

            return pomonaResponse.Entity;
        }
    }
}