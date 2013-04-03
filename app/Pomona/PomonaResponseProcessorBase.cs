using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nancy;
using Nancy.Responses.Negotiation;
using Pomona.Common.Serialization;
using ISerializer = Pomona.Common.Serialization.ISerializer;

namespace Pomona
{
    public abstract class PomonaResponseProcessorBase : IResponseProcessor
    {
        private readonly ISerializer serializer;
        private readonly ISerializerFactory serializerFactory;

        protected PomonaResponseProcessorBase(ISerializerFactory serializerFactory)
        {
            this.serializerFactory = serializerFactory;
            serializer = serializerFactory.GetSerialier();
        }

        protected abstract string ContentType { get; }

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
            var pomonaResponse = (PomonaResponse) model;

            string jsonString;
            using (var strWriter = new StringWriter())
            {
                var pq = (PomonaQuery) pomonaResponse.Query;
                var serializationContext = new ServerSerializationContext(pq.ExpandedPaths, false,
                                                                          pomonaResponse.Session);
                serializer.Serialize(serializationContext, pomonaResponse.Entity, strWriter, pq.ResultType);
                jsonString = strWriter.ToString();
            }

            if (IsTextHtmlContentType(requestedMediaRange))
            {
                // Wrap in html
                var response = new Response();
                HtmlJsonPrettifier.CreatePrettifiedHtmlJsonResponse(response, string.Empty, jsonString,
                                                                    "http://failfailtodo");
                return response;
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                return new Response
                    {
                        Contents = s => s.Write(bytes, 0, bytes.Length),
                        ContentType = ContentType
                    };
            }
        }

        public abstract ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context);

        protected bool IsTextHtmlContentType(MediaRange requestedMediaRange)
        {
            return requestedMediaRange.Matches("text/html");
        }
    }
}