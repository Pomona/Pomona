#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using Pomona.Common.Internals;

namespace Pomona.Common.Web
{
    [Serializable]
    internal class WebClientException<TBody> : WebClientException, IWebClientException<TBody>
    {
        protected WebClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        internal WebClientException(HttpRequestMessage request,
                                    HttpResponseMessage response,
                                    TBody body = default(TBody),
                                    Exception innerException = null)
            : base(request, response, body, innerException)
        {
        }


        public new TBody Body
        {
            get { return (TBody)base.Body; }
        }
    }

    [Serializable]
    public class WebClientException : Exception
    {
        private static readonly MethodInfo createGenericMethod;


        static WebClientException()
        {
            createGenericMethod = ReflectionHelper
                .GetMethodDefinition<WebClientException>(x => CreateGeneric<object>(null, null, null, null));
        }


        protected WebClientException(HttpRequestMessage request,
                                     HttpResponseMessage response,
                                     object body = null,
                                     Exception innerException = null)
            : base(CreateMessage(request, response, body), innerException)
        {
            Body = body;
            StatusCode = response != null
                ? response.StatusCode
                : HttpStatusCode.NoContent;
            Uri = GetUri(request);
        }


        protected WebClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public object Body { get; }

        public bool HasBody
        {
            get { return Body != null; }
        }

        public HttpStatusCode StatusCode { get; }

        public string Uri { get; }


        public static WebClientException Create(IClientTypeResolver client,
                                                HttpRequestMessage request,
                                                HttpResponseMessage response,
                                                object bodyObject,
                                                Exception innerException)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // String body doesn't create a generic exception, it just puts the string in the message
            if (bodyObject != null && !(bodyObject is string))
            {
                var bodyObjectType = bodyObject.GetType();
                var genericArg = bodyObject is IClientResource
                    ? client.GetMostInheritedResourceInterface(bodyObjectType)
                    : bodyObjectType;

                return (WebClientException)createGenericMethod
                    .MakeGenericMethod(genericArg)
                    .Invoke(null, new[] { request, response, bodyObject, innerException });
            }

            var statusCode = response != null ? response.StatusCode : HttpStatusCode.NoContent;
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestException(request, response, bodyObject, innerException);
                case HttpStatusCode.NotFound:
                    return new ResourceNotFoundException(request, response, bodyObject, innerException);
                case HttpStatusCode.PreconditionFailed:
                    return new PreconditionFailedException(request, response, bodyObject, innerException);
                default:
                    return new WebClientException(request, response, bodyObject, innerException);
            }
        }


        private static WebClientException CreateGeneric<TBody>(HttpRequestMessage request,
                                                               HttpResponseMessage response,
                                                               TBody bodyObject,
                                                               Exception innerException)
        {
            var statusCode = response != null ? response.StatusCode : HttpStatusCode.NoContent;
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestException<TBody>(request, response, bodyObject, innerException);
                case HttpStatusCode.NotFound:
                    return new ResourceNotFoundException<TBody>(request, response, bodyObject, innerException);
                case HttpStatusCode.PreconditionFailed:
                    return new PreconditionFailedException<TBody>(request, response, bodyObject, innerException);
                default:
                    return new WebClientException<TBody>(request, response, bodyObject, innerException);
            }
        }


        private static string CreateMessage(HttpRequestMessage request,
                                            HttpResponseMessage response,
                                            object body)
        {
            StringBuilder message = new StringBuilder("The ");

            if (request != null)
                message.AppendFormat("{0} ", request.Method);

            message.Append("request ");

            string uri = GetUri(request);

            if (!String.IsNullOrWhiteSpace(uri))
                message.AppendFormat("to <{0}> ", uri);

            if (response != null)
            {
                message.AppendFormat("failed with '{0} {1}'",
                                     (int)response.StatusCode,
                                     response.StatusCode);
            }
            else
                message.Append("got no response");

            body = GetBody(body, response);

            var bodyString = body as string;
            if (bodyString == null && body != null)
            {
                var messageProperty = body.GetType().GetProperty("Message");
                if (messageProperty != null && messageProperty.PropertyType == typeof(string))
                    bodyString = (string)messageProperty.GetValue(body, null);
            }

            if (!String.IsNullOrWhiteSpace(bodyString))
            {
                message.Append(": ");
                message.Append(bodyString);
            }
            else
                message.Append('.');

            return message.ToString().Trim();
        }


        private static object GetBody(object body, HttpResponseMessage response)
        {
            if (body != null)
                return body;

            if (response == null || response.Content == null)
                return null;

            try
            {
                return response.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("The response data could not be encoded as an UTF-8 string. {0}", exception);
                return null;
            }
        }


        private static string GetUri(HttpRequestMessage request)
        {
            return (request != null ? (request.RequestUri != null ? request.RequestUri.ToString() : null) : null);
        }
    }
}