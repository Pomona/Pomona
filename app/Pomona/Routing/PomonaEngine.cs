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
using System.Linq;

using Nancy;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;

namespace Pomona.Routing
{
    public class PomonaEngine
    {
        private readonly IContainer container;
        private readonly TypeMapper typeMapper;


        public PomonaEngine(TypeMapper typeMapper, IContainer container)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (container == null)
                throw new ArgumentNullException("container");
            this.typeMapper = typeMapper;
            this.container = container;
        }


        public PomonaResponse Handle(NancyContext context, string modulePath)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (modulePath == null)
                throw new ArgumentNullException("modulePath");
            var session = new PomonaSession(this.typeMapper,
                                            new DefaultRequestProcessorPipeline(),
                                            new PomonaJsonSerializerFactory(),
                                            typeMapper.ActionResolver,
                                            this.container);

            var moduleRelativePath = context.Request.Path.Substring(modulePath.Length);

            var match = typeMapper.RouteResolver.Resolve(session, moduleRelativePath);

            if (match == null)
                    throw new ResourceNotFoundException("Resource not found.");

            var finalSegmentMatch = match.Root.SelectedFinalMatch;
            if (finalSegmentMatch == null)
            {
                // Route conflict resolution:
                var node = match.Root.NextConflict;
                while (node != null)
                {
                    var actualResultType = node.ActualResultType;
                    // Reduce using input type difference
                    var validSelection =
                        node.Children.Where(x => x.Route.InputType.IsAssignableFrom(actualResultType))
                            .SingleOrDefaultIfMultiple();
                    if (validSelection == null)
                        throw new ResourceNotFoundException("No route alternative found due to conflict.");
                    node.SelectedChild = validSelection;
                    node = node.NextConflict;
                }
                finalSegmentMatch = match.Root.SelectedFinalMatch;
            }

            HttpMethod httpMethod =
                (HttpMethod)Enum.Parse(typeof(HttpMethod), context.Request.Method, true);

            return
                session.Dispatch(new PomonaRequest(finalSegmentMatch,
                                                   httpMethod,
                                                   body : context.Request.Body,
                                                   executeQueryable : true,
                                                   query : context.Request.Query,
                                                   headers : context.Request.Headers,
                                                   url : context.Request.Url.ToString()));
        }
    }
}