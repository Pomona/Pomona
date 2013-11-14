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

using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Responses.Negotiation;
using Pomona.Common.Serialization.Csv;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaCsvResponseProcessor : PomonaResponseProcessorBase
    {
        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] {new Tuple<string, MediaRange>("csv", MediaRange.FromString("text/plain"))};

        public PomonaCsvResponseProcessor(TypeMapper typeMapper)
            : base(new PomonaCsvSerializerFactory(), typeMapper)
        {
        }

        protected override string ContentType
        {
            get { return "text/plain"; }
        }

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .Csv)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public override IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
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
        public override ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model as PomonaResponse == null)
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.NoMatch,
                        RequestedContentTypeResult = MatchResult.DontCare
                    };

            //if (IsTextHtmlContentType(requestedMediaRange))
            //    return new ProcessorMatch
            //        {
            //            ModelResult = MatchResult.ExactMatch,
            //            RequestedContentTypeResult = MatchResult.ExactMatch
            //        };

            if (IsExactCsvContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
            }

            return new ProcessorMatch
                {
                    ModelResult = MatchResult.ExactMatch,
                    RequestedContentTypeResult = MatchResult.NoMatch
                };
        }


        private static bool IsExactCsvContentType(MediaRange requestedContentType)
        {
            if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            return requestedContentType.Matches("text/plain");
        }
    }
}