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
using System.IO;
using System.Linq;
using System.Text;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Routing;

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona
{
    /// <summary>
    /// Default response processor base class for Pomona.
    /// </summary>
    public abstract class PomonaResponseProcessorBase : IResponseProcessor
    {
        private readonly IRouteCacheProvider routeCacheProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="PomonaResponseProcessorBase"/> class.
        /// </summary>
        /// <param name="routeCacheProvider">The route cache provider.</param>
        /// <exception cref="System.ArgumentNullException">routeCacheProvider</exception>
        protected PomonaResponseProcessorBase(IRouteCacheProvider routeCacheProvider)
        {
            if (routeCacheProvider == null)
                throw new ArgumentNullException("routeCacheProvider");

            this.routeCacheProvider = routeCacheProvider;
        }


        /// <summary>
        /// Gets the HTTP content type.
        /// </summary>
        /// <value>
        /// The HTTP content type.
        /// </value>
        protected abstract string ContentType { get; }

        protected abstract ITextSerializerFactory GetSerializerFactory(NancyContext context);


        protected bool IsTextHtmlContentType(MediaRange requestedMediaRange)
        {
            return requestedMediaRange.Matches("text/html");
        }


        private string GetHtmlLinks(NancyContext context)
        {
            var routeCache = this.routeCacheProvider.GetCache();
            if (routeCache == null)
                return String.Empty;

            StringBuilder linkBuilder = new StringBuilder();

            var routesWithMetadata = routeCache
                .Select(r => r.Value)
                .SelectMany(r => r.Select(t => t.Item2))
                .Where(r => r.Metadata != null && r.Metadata.Has<PomonaRouteMetadata>());

            foreach (var route in routesWithMetadata)
            {
                var metadata = route.Metadata.Retrieve<PomonaRouteMetadata>();
                var rel = String.Concat("http://pomona.io/rel/", metadata.Relation);
                var contentType = metadata.ContentType;
                var methods = metadata.Method.ToString().ToUpperInvariant();
                var href = context.Request.Url.BasePath + route.Path;

                linkBuilder.AppendFormat("<link rel=\"{0}\" type=\"{1}\" methods=\"{2}\" href=\"{3}\">{4}",
                                         rel,
                                         contentType,
                                         methods,
                                         href,
                                         Environment.NewLine);
            }

            return linkBuilder.ToString();
        }


        private ITextSerializer GetSerializer(NancyContext context)
        {
            return GetSerializerFactory(context)
                .GetSerializer(context.GetPomonaSession().GetInstance<ISerializationContextProvider>());
        }


        /// <summary>
        /// Determines whether this instance can process the specified requested media range.
        /// </summary>
        /// <param name="requestedMediaRange">The requested media range.</param>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public abstract ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context);


        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public abstract IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get; }


        /// <summary>
        /// Process the response
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A response</returns>
        public virtual Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var pomonaResponse = (PomonaResponse)model;

            if (pomonaResponse.Entity == PomonaResponse.NoBodyEntity)
                return new Response { StatusCode = pomonaResponse.StatusCode };

            var serializer = GetSerializer(context);
            var serializeOptions = new SerializeOptions
            {
                ExpandedPaths = pomonaResponse.ExpandedPaths,
                ExpectedBaseType = pomonaResponse.ResultType
            };

            if (IsTextHtmlContentType(requestedMediaRange))
            {
                // Wrap in html
                var response = new Response();
                var htmlLinks = GetHtmlLinks(context);
                var jsonString = serializer.SerializeToString(pomonaResponse.Entity, serializeOptions);

                HtmlJsonPrettifier.CreatePrettifiedHtmlJsonResponse(response,
                                                                    htmlLinks,
                                                                    jsonString,
                                                                    "http://failfailtodo");
                return response;
            }
            else
            {
                var response = new Response
                {
                    //Headers = {{"Content-Length", bytes.Length.ToString()}},
                    Contents = stream =>
                    {
                        using (var streamWriter = new NonClosingStreamWriter(stream))
                        {
                            serializer.Serialize(streamWriter, pomonaResponse.Entity, serializeOptions);
                        }
                    },
                    ContentType = ContentType,
                    StatusCode = pomonaResponse.StatusCode
                };

                if (pomonaResponse.ResponseHeaders != null)
                {
                    foreach (var kvp in pomonaResponse.ResponseHeaders)
                        response.Headers.Add(kvp);
                }

                // Add etag header
                var resourceType = pomonaResponse.ResultType as ResourceType;
                if (resourceType == null)
                    return response;

                var etagProperty = resourceType.ETagProperty;
                if (pomonaResponse.Entity == null || etagProperty == null)
                    return response;

                var etagValue = (string)etagProperty.GetValue(pomonaResponse.Entity);
                if (etagValue != null)
                {
                    // I think defining this as a weak etag will be correct, since we can specify $expand which change data (byte-by-byte).
                    response.Headers["ETag"] = String.Format("W/\"{0}\"", etagValue);
                }

                return response;
            }
        }


        private class NonClosingStreamWriter : StreamWriter
        {
            public NonClosingStreamWriter(Stream stream)
                : base(stream, new UTF8Encoding(false))
            {
            }


            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    Flush();

                base.Dispose(false);
            }
        }
    }
}