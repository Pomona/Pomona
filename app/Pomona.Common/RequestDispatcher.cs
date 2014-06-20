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
using System.Collections;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public class RequestDispatcher : IRequestDispatcher
    {
        private readonly ITextSerializerFactory serializerFactory;
        private readonly ClientTypeMapper typeMapper;
        private readonly IWebClient webClient;


        public RequestDispatcher(ClientTypeMapper typeMapper,
            IWebClient webClient,
            ITextSerializerFactory serializerFactory)
        {
            if (typeMapper != null)
                this.typeMapper = typeMapper;
            if (webClient != null)
                this.webClient = webClient;
            if (serializerFactory != null)
                this.serializerFactory = serializerFactory;
        }


        public IWebClient WebClient
        {
            get { return this.webClient; }
        }

        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;


        public object SendRequest(
            ISerializationContextProvider provider,
            string uri,
            object body,
            string httpMethod,
            RequestOptions options,
            Type responseBaseType = null)
        {
            var bodyAsExtendedProxy = body as IExtendedResourceProxy;
            if (bodyAsExtendedProxy != null)
            {
                return SendExtendedResourceRequest(provider,
                    uri,
                    bodyAsExtendedProxy,
                    httpMethod,
                    options,
                    responseBaseType);
            }

            var response = SendHttpRequest(provider, uri, httpMethod, body, null, options);
            return response != null ? Deserialize(response, responseBaseType, provider) : null;
        }


        protected virtual object SendExtendedResourceRequest(
            ISerializationContextProvider serializationContextProvider,
            string uri,
            IExtendedResourceProxy body,
            string httpMethod,
            RequestOptions options,
            Type responseBaseType = null)
        {
            var info = body.UserTypeInfo;
            var serverTypeResult = SendRequest(serializationContextProvider,
                uri,
                body.WrappedResource,
                httpMethod,
                options,
                null);
            if (serverTypeResult == null)
                return null;

            var expectedResponseType = options != null ? options.ExpectedResponseType : null;

            if ((expectedResponseType == null || expectedResponseType == info.ServerType) &&
                info.ServerType.IsInstanceOfType(serverTypeResult))
            {
                return this.typeMapper.WrapResource(serverTypeResult,
                    info.ServerType,
                    info.ExtendedType);
            }

            ExtendedResourceInfo responseExtendedInfo;
            if (expectedResponseType != null
                && this.typeMapper.TryGetExtendedTypeInfo(expectedResponseType, out responseExtendedInfo))
            {
                return this.typeMapper.WrapResource(serverTypeResult,
                    responseExtendedInfo.ServerType,
                    responseExtendedInfo.ExtendedType);
            }

            return serverTypeResult;
        }


        private object Deserialize(string jsonString,
            Type expectedType,
            ISerializationContextProvider serializationContextProvider)
        {
            // TODO: Clean up this mess, we need to get a uniform container type for all results! [KNS]
            var jToken = JToken.Parse(jsonString);

            if (expectedType == typeof(JToken))
                return jToken;

            var jObject = jToken as JObject;
            if (jObject != null)
            {
                JToken typeValue;
                if (jObject.TryGetValue("_type", out typeValue))
                {
                    if (typeValue.Type == JTokenType.String && (string)((JValue)typeValue).Value == "__result__")
                    {
                        JToken itemsToken;
                        if (!jObject.TryGetValue("items", out itemsToken))
                            throw new InvalidOperationException("Got result object, but lacking items");

                        var totalCount = (int)jObject.GetValue("totalCount");

                        var deserializedItems = Deserialize(itemsToken.ToString(),
                            expectedType,
                            serializationContextProvider);
                        return QueryResult.Create((IEnumerable)deserializedItems,
                            /* TODO */ 0,
                            totalCount,
                            "http://todo");
                    }
                }
            }

            return this.serializerFactory.GetDeserializer(serializationContextProvider).DeserializeString(jsonString,
                new DeserializeOptions() { ExpectedBaseType = expectedType });
        }


        private string SendHttpRequest(
            ISerializationContextProvider serializationContextProvider,
            string uri,
            string httpMethod,
            object requestBodyEntity,
            TypeSpec requestBodyBaseType,
            RequestOptions options)
        {
            byte[] requestBytes = null;
            WebClientResponseMessage response = null;
            if (requestBodyEntity != null)
            {
                requestBytes =
                    this.serializerFactory.GetSerializer(serializationContextProvider).SerializeToBytes(
                        requestBodyEntity,
                        new SerializeOptions() { ExpectedBaseType = requestBodyBaseType });
            }
            var request = new WebClientRequestMessage(uri, requestBytes, httpMethod);

            string responseString = null;
            Exception thrownException = null;
            try
            {
                if (options != null)
                    options.ApplyRequestModifications(request);

                request.Headers.Add("Accept", "application/json");
                response = this.webClient.Send(request);
                responseString = (response.Data != null && response.Data.Length > 0)
                    ? Encoding.UTF8.GetString(response.Data)
                    : null;

                if ((int)response.StatusCode >= 400)
                {
                    var gotJsonResponseBody = responseString != null &&
                                              response.Headers.GetValues("Content-Type")
                                                  .Any(x => x.StartsWith("application/json"));

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
    }
}