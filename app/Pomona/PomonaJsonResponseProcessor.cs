#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Routing;

using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;

namespace Pomona
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class PomonaJsonResponseProcessor : PomonaResponseProcessorBase
    {
        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] { new Tuple<string, MediaRange>("json", new MediaRange("application/json")) };


        public PomonaJsonResponseProcessor(IRouteCacheProvider routeCacheProvider)
            : base(routeCacheProvider)
        {
        }


        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public override IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return extensionMappings; }
        }

        protected override string ContentType
        {
            get { return "application/json; charset=utf-8"; }
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
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.NoMatch,
                    RequestedContentTypeResult = MatchResult.DontCare
                };
            }

            if (IsTextHtmlContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.ExactMatch,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }

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


        protected override ITextSerializerFactory GetSerializerFactory(NancyContext context)
        {
            return new PomonaJsonSerializerFactory();
        }


        private static bool IsExactJsonContentType(MediaRange requestedContentType)
        {
            if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
                return true;

            return requestedContentType.Matches("application/json") || requestedContentType.Matches("text/json");
        }


        private static bool IsWildcardJsonContentType(MediaRange requestedContentType)
        {
            if (!requestedContentType.Type.IsWildcard &&
                !string.Equals("application", requestedContentType.Type, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (requestedContentType.Subtype.IsWildcard)
                return true;

            var subtypeString = requestedContentType.Subtype.ToString();

            return (subtypeString.StartsWith("vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    subtypeString.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}