#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net.Http;
using System.Runtime.Serialization;

namespace Pomona.Common.Web
{
    [Serializable]
    public class ResourceNotFoundException<TBody> : ResourceNotFoundException, IWebClientException<TBody>
    {
        public ResourceNotFoundException(HttpRequestMessage request,
                                         HttpResponseMessage response,
                                         TBody body = default(TBody),
                                         Exception innerException = null)
            : base(request, response, body, innerException)
        {
        }


        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public new TBody Body
        {
            get { return (TBody)base.Body; }
        }
    }

    [Serializable]
    public class ResourceNotFoundException : WebClientException
    {
        public ResourceNotFoundException(HttpRequestMessage request,
                                         HttpResponseMessage response,
                                         object body = null,
                                         Exception innerException = null)
            : base(request, response, body, innerException)
        {
        }


        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}