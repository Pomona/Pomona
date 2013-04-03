using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Responses.Negotiation;
using Pomona.Common.Serialization.Xml;

namespace Pomona
{
    public class PomonaXmlResponseProcessor : PomonaResponseProcessorBase
    {
        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] { new Tuple<string, MediaRange>("xml", MediaRange.FromString("application/xml")) };

        public PomonaXmlResponseProcessor()
            : base(new PomonaXmlSerializerFactory())
        {
        }

        protected override string ContentType
        {
            get { return "application/xml"; }
        }

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .Xml)
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

            if (IsExactXmlContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
            }

            if (IsWildcardXmlContentType(requestedMediaRange))
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


        private static bool IsExactXmlContentType(MediaRange requestedContentType)
        {
            if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            return requestedContentType.Matches("application/xml") || requestedContentType.Matches("text/xml");
        }

        private static bool IsWildcardXmlContentType(MediaRange requestedContentType)
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
                    subtypeString.EndsWith("+xml", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}