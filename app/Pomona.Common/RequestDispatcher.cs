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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json.Linq;

using Pomona.Common.ExtendedResources;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;
using Pomona.Profiling;

using HttpHeaders = Pomona.Common.Web.HttpHeaders;

namespace Pomona.Common
{
    public class RequestDispatcher : IRequestDispatcher
    {
        private readonly HttpHeaders defaultHeaders;
        private readonly ITextSerializerFactory serializerFactory;
        private readonly ClientTypeMapper typeMapper;
        private readonly IWebClient webClient;


        public RequestDispatcher(ClientTypeMapper typeMapper,
                                 IWebClient webClient,
                                 ITextSerializerFactory serializerFactory,
                                 HttpHeaders defaultHeaders = null)
        {
            this.defaultHeaders = defaultHeaders;
            if (typeMapper != null)
                this.typeMapper = typeMapper;
            if (webClient != null)
                this.webClient = webClient;
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


        private string SendHttpRequest(ISerializationContextProvider serializationContextProvider,
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
                    response = this.webClient.Send(request).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                if (response.Content != null)
                {
                    responseString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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
                    eh(this, new ClientRequestLogEventArgs(request, response, thrownException));
            }

            return responseString;
        }


        private object SendRequestInner(string uri,
                                        string httpMethod,
                                        object body,
                                        ISerializationContextProvider provider,
                                        RequestOptions options)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (options == null)
                options = new RequestOptions();

            if (body is IExtendedResourceProxy)
                throw new ArgumentException("SendRequestInner should never get a body of type IExtendedResourceProxy");

            var response = SendHttpRequest(provider, uri, httpMethod, body, null, options);
            return response != null ? Deserialize(response, options.ExpectedResponseType, provider) : null;
        }


        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;


        public object SendRequest(string uri,
                                  string httpMethod,
                                  object body,
                                  ISerializationContextProvider provider,
                                  RequestOptions options = null)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
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

            var innerResult = SendRequestInner(uri, httpMethod, innerBody, provider, innerOptions);
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


        public IWebClient WebClient
        {
            get { return this.webClient; }
        }
    }
}