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
using System.Text;

using Nancy;
using Nancy.Responses.Negotiation;

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public abstract class PomonaResponseProcessorBase : IResponseProcessor
    {
        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public abstract IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get; }

        protected abstract string ContentType { get; }

        public abstract ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context);


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
            string jsonString;

            if (pomonaResponse.Entity == PomonaResponse.NoBodyEntity)
                return new Response { StatusCode = pomonaResponse.StatusCode };

            jsonString =
                GetSerializerFactory(context,
                    context.GetSerializationContextProvider())
                    .GetSerializer().SerializeToString(pomonaResponse.Entity,
                        new SerializeOptions()
                        {
                            ExpandedPaths = pomonaResponse.ExpandedPaths,
                            ExpectedBaseType = pomonaResponse.ResultType
                        });

            if (IsTextHtmlContentType(requestedMediaRange))
            {
                // Wrap in html
                var response = new Response();
                HtmlJsonPrettifier.CreatePrettifiedHtmlJsonResponse(response,
                    string.Empty,
                    jsonString,
                    "http://failfailtodo");
                return response;
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                var response = new Response
                {
                    //Headers = {{"Content-Length", bytes.Length.ToString()}},
                    Contents = s => s.Write(bytes, 0, bytes.Length),
                    ContentType = ContentType,
                    StatusCode = pomonaResponse.StatusCode
                };

                if (pomonaResponse.ResponseHeaders != null)
                {
                    foreach (var kvp in pomonaResponse.ResponseHeaders)
                        response.Headers.Add(kvp);
                }

                // Add etag header
                var transformedResultType = pomonaResponse.ResultType as TransformedType;
                if (transformedResultType != null)
                {
                    var etagProperty = transformedResultType.ETagProperty;
                    if (pomonaResponse.Entity != null && etagProperty != null)
                    {
                        var etagValue = (string)etagProperty.Getter(pomonaResponse.Entity);
                        if (etagValue != null)
                        {
                            // I think defining this as a weak etag will be correct, since we can specify $expand which change data (byte-by-byte).
                            response.Headers["ETag"] = string.Format("W/\"{0}\"", etagValue);
                        }
                    }
                }

                return response;
            }
        }


        protected abstract ITextSerializerFactory GetSerializerFactory(NancyContext context,
            ISerializationContextProvider contextProvider);


        protected bool IsTextHtmlContentType(MediaRange requestedMediaRange)
        {
            return requestedMediaRange.Matches("text/html");
        }
    }
}