#region License

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

#endregion

using System;
using System.Reflection;
using Pomona.Common.Internals;
using Pomona.Internals;

namespace Pomona.Common.Web
{
    internal class WebClientException<TBody> : WebClientException, IWebClientException<TBody>
    {
        internal WebClientException(WebClientRequestMessage request, WebClientResponseMessage response, TBody body,
                                    Exception innerException) : base(request, response, body, innerException)
        {
        }

        public new TBody Body
        {
            get { return (TBody) base.Body; }
        }
    }

    public class WebClientException : Exception
    {
        private static readonly MethodInfo createGenericMethod =
            ReflectionHelper.GetMethodDefinition<WebClientException>(
                x => CreateGeneric<object>(null, null, null, null));

        private readonly object body;

        private readonly WebClientRequestMessage request;
        private readonly WebClientResponseMessage response;

        protected WebClientException(WebClientRequestMessage request, WebClientResponseMessage response,
                                     object body, Exception innerException)
            : base(response != null ? response.StatusCode.ToString() : "Response missing", innerException)
        {
            this.request = request;
            this.response = response;
            this.body = body;
        }

        public bool HasBody
        {
            get { return body != null; }
        }

        public object Body
        {
            get { return body; }
        }

        public HttpStatusCode StatusCode
        {
            get { return response != null ? response.StatusCode : HttpStatusCode.EmptyResponse; }
        }

        private static WebClientException CreateGeneric<TBody>(WebClientRequestMessage request,
                                                               WebClientResponseMessage response,
                                                               TBody bodyObject, Exception innerException)
        {
            var statusCode = response != null ? response.StatusCode : HttpStatusCode.EmptyResponse;
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

        public static WebClientException Create(IPomonaClient client, WebClientRequestMessage request,
                                                WebClientResponseMessage response,
                                                object bodyObject, Exception innerException)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (bodyObject != null)
            {
                var bodyObjectType = bodyObject.GetType();
                var genericArg = bodyObject is IClientResource
                                     ? client.GetMostInheritedResourceInterface(bodyObjectType)
                                     : bodyObjectType;
                return
                    (WebClientException)
                    createGenericMethod
                        .MakeGenericMethod(genericArg)
                        .Invoke(null, new[] {request, response, bodyObject, innerException});
            }

            var statusCode = response != null ? response.StatusCode : HttpStatusCode.EmptyResponse;
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new BadRequestException(request, response, null, innerException);
                case HttpStatusCode.NotFound:
                    return new ResourceNotFoundException(request, response, null, innerException);
                case HttpStatusCode.PreconditionFailed:
                    return new PreconditionFailedException(request, response, null, innerException);
                default:
                    return new WebClientException(request, response, null, innerException);
            }
        }
    }
}