#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Pomona.Common.ExtendedResources;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;
using Pomona.Profiling;

namespace Pomona.Common
{
    public class RequestDispatcher : IRequestDispatcher
    {
        private readonly IEnumerable<KeyValuePair<string, IEnumerable<string>>> defaultHeaders;
        private readonly ITextSerializerFactory serializerFactory;
        private readonly ClientTypeMapper typeMapper;


        public RequestDispatcher(ClientTypeMapper typeMapper,
                                 IWebClient webClient,
                                 ITextSerializerFactory serializerFactory,
                                 IEnumerable<KeyValuePair<string, IEnumerable<string>>> defaultHeaders = null)
        {
            this.defaultHeaders = defaultHeaders;
            if (typeMapper != null)
                this.typeMapper = typeMapper;
            if (webClient != null)
                WebClient = webClient;
            if (serializerFactory != null)
                this.serializerFactory = serializerFactory;
        }


        private void AddDefaultHeaders(HttpRequestMessage request)
        {
            if (this.defaultHeaders != null)
            {
                foreach (var header in this.defaultHeaders)
                {
                    if (!request.Headers.Contains(header.Key))
                        request.Headers.Add(header.Key, header.Value);
                }
            }
        }


        private object Deserialize(string jsonString,
                                   Type expectedType,
                                   ISerializationContextProvider serializationContextProvider)
        {
            if (expectedType == typeof(JToken))
                return JToken.Parse(jsonString);

            return this.serializerFactory
                       .GetDeserializer(serializationContextProvider)
                       .DeserializeString(jsonString, new DeserializeOptions
                       {
                           ExpectedBaseType = expectedType
                       });
        }


        private async Task<string> SendHttpRequestAsync(ISerializationContextProvider serializationContextProvider,
                                                        string uri,
                                                        string httpMethod,
                                                        object requestBodyEntity,
                                                        TypeSpec requestBodyBaseType,
                                                        RequestOptions options)
        {
            byte[] requestBytes = null;
            HttpResponseMessage response = null;
            if (requestBodyEntity != null)
            {
                requestBytes = this.serializerFactory
                                   .GetSerializer(serializationContextProvider)
                                   .SerializeToBytes(requestBodyEntity, new SerializeOptions
                                   {
                                       ExpectedBaseType = requestBodyBaseType
                                   });
            }
            var request = new HttpRequestMessage(new System.Net.Http.HttpMethod(httpMethod), uri);

            string responseString = null;
            Exception thrownException = null;
            try
            {
                if (options != null)
                    options.ApplyRequestModifications(request);

                AddDefaultHeaders(request);

                const string jsonContentType = "application/json; charset=utf-8";
                request.Headers.Add("Accept", jsonContentType);
                if (requestBytes != null)
                {
                    var requestContent = new ByteArrayContent(requestBytes);
                    requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(jsonContentType);
                    request.Content = requestContent;
                }

                using (Profiler.Step("client: " + request.Method + " " + request.RequestUri))
                {
                    response = await WebClient.SendAsync(request, CancellationToken.None);
                }

                if (response.Content != null)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                    if (responseString.Length == 0)
                        responseString = null;
                }

                if ((int)response.StatusCode >= 400)
                {
                    var gotJsonResponseBody = responseString != null && response.Content.Headers.ContentType.MediaType == "application/json";

                    var responseObject = gotJsonResponseBody
                        ? Deserialize(responseString, null, serializationContextProvider)
                        : null;

                    throw WebClientException.Create(this.typeMapper, request, response, responseObject, null);
                }
            }
            catch (Exception ex)
            {
                thrownException = ex;
                throw;
            }
            finally
            {
                var eh = RequestCompleted;
                if (eh != null)
                {
                    // Since request content has been disposed at this point we recreate it..
                    if (request.Content != null)
                    {
                        var nonDisposedContent = new ByteArrayContent(requestBytes);
                        nonDisposedContent.Headers.CopyHeadersFrom(request.Content.Headers);
                        request.Content = nonDisposedContent;
                    }
                    eh(this, new ClientRequestLogEventArgs(request, response, thrownException));
                }
            }

            return responseString;
        }


        private async Task<object> SendRequestInnerAsync(string uri,
                                                         string httpMethod,
                                                         object body,
                                                         ISerializationContextProvider provider,
                                                         RequestOptions options)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));
            if (options == null)
                options = new RequestOptions();

            if (body is IExtendedResourceProxy)
                throw new ArgumentException("SendRequestInner should never get a body of type IExtendedResourceProxy");

            var response = await SendHttpRequestAsync(provider, uri, httpMethod, body, null, options);
            return response != null ? Deserialize(response, options.ExpectedResponseType, provider) : null;
        }


        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;


        public object SendRequest(string uri,
                                  string httpMethod,
                                  object body,
                                  ISerializationContextProvider provider,
                                  RequestOptions options = null)
        {
            try
            {
                return Task.Run(() => SendRequestAsync(uri, httpMethod, body, provider, options)).Result;
            }
            catch (AggregateException aggregateException)
                when (aggregateException.InnerExceptions.Count == 1 && aggregateException.InnerException is WebClientException)
            {
                throw aggregateException.InnerException;
            }
        }


        public async Task<object> SendRequestAsync(string uri,
                                                   string httpMethod,
                                                   object body,
                                                   ISerializationContextProvider provider,
                                                   RequestOptions options = null)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));
            if (options == null)
                options = new RequestOptions();

            var innerBody = body;
            var proxyBody = body as IExtendedResourceProxy;
            if (proxyBody != null)
                innerBody = proxyBody.WrappedResource;

            // Figure out server side response type
            ExtendedResourceInfo responseExtendedTypeInfo;
            var responseType = options.ExpectedResponseType;
            var innerOptions = options;
            var innerResponseType = options.ExpectedResponseType;
            if (innerResponseType != null)
            {
                if (this.typeMapper.TryGetExtendedTypeInfo(innerResponseType, out responseExtendedTypeInfo))
                {
                    innerResponseType = responseExtendedTypeInfo.ServerType;
                    innerOptions = new RequestOptions(options) { ExpectedResponseType = innerResponseType };
                }
            }

            var innerResult = await SendRequestInnerAsync(uri, httpMethod, innerBody, provider, innerOptions);
            if (innerResponseType == null && proxyBody != null && innerResult != null)
            {
                // Special case: No response type specified, but response has same type as posted body,
                // and the posted body was of an extended type. In this case we will wrap the response
                // to the same type if possible.
                var proxyBodyInfo = proxyBody.UserTypeInfo;
                if (proxyBodyInfo.ServerType.IsInstanceOfType(innerResult))
                {
                    responseType = proxyBodyInfo.ExtendedType;
                    innerResponseType = proxyBodyInfo.ServerType;
                }
            }

            if (responseType != innerResponseType)
                return this.typeMapper.WrapResource(innerResult, innerResponseType, responseType);
            return innerResult;
        }


        public IWebClient WebClient { get; }
    }
}
