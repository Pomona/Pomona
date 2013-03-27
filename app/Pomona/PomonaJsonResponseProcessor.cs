﻿// ----------------------------------------------------------------------------
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nancy;
using Nancy.Responses.Negotiation;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using ISerializer = Pomona.Common.Serialization.ISerializer;

namespace Pomona
{
    public class PomonaJsonResponseProcessor : IResponseProcessor
    {
        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] {new Tuple<string, MediaRange>("json", MediaRange.FromString("application/json"))};

        private readonly ISerializer serializer;
        private readonly PomonaJsonSerializerFactory serializerFactory;

        public PomonaJsonResponseProcessor()
        {
            serializerFactory = new PomonaJsonSerializerFactory();
            serializer = serializerFactory.GetSerialier();
        }

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return extensionMappings; }
        }

        /// <summary>
        /// Determines whether the the processor can handle a given content type and model
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A ProcessorMatch result that determines the priority of the processor</returns>
        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model as PomonaResponse == null)
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.NoMatch,
                        RequestedContentTypeResult = MatchResult.DontCare
                    };

            if (IsTextHtmlContentType(requestedMediaRange))
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };

            if (IsExactJsonContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
            }

            if (IsWildcardJsonContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.NonExactMatch
                    };
            }

            return new ProcessorMatch
                {
                    ModelResult = MatchResult.ExactMatch,
                    RequestedContentTypeResult = MatchResult.NoMatch
                };
        }

        /// <summary>
        /// Process the response
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A response</returns>
        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var pomonaResponse = (PomonaResponse) model;

            string jsonString;
            using (var strWriter = new StringWriter())
            {
                var pq = (PomonaQuery) pomonaResponse.Query;
                var serializationContext = new ServerSerializationContext(pq.ExpandedPaths, false,
                                                                          pomonaResponse.Session);
                serializer.Serialize(serializationContext, pomonaResponse.Entity, strWriter);
                jsonString = strWriter.ToString();
            }

            if (IsTextHtmlContentType(requestedMediaRange))
            {
                // Wrap in html
                var response = new Response();
                HtmlJsonPrettifier.CreatePrettifiedHtmlJsonResponse(response, string.Empty, jsonString, "http://failfailtodo");
                return response;
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                return new Response
                {
                    Contents = s => s.Write(bytes, 0, bytes.Length),
                    ContentType = "application/json"
                };
            }
        }

        private bool IsTextHtmlContentType(MediaRange requestedMediaRange)
        {
            return requestedMediaRange.Matches("text/html");
        }

        private static bool IsExactJsonContentType(MediaRange requestedContentType)
        {
            if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            return requestedContentType.Matches("application/json") || requestedContentType.Matches("text/json");
        }

        private static bool IsWildcardJsonContentType(MediaRange requestedContentType)
        {
            if (!requestedContentType.Type.IsWildcard &&
                !string.Equals("application", requestedContentType.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            var subtypeString = requestedContentType.Subtype.ToString();

            return (subtypeString.StartsWith("vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    subtypeString.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}