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
    public class PreconditionFailedException<TBody> : PreconditionFailedException, IWebClientException<TBody>
    {
        public PreconditionFailedException(HttpRequestMessage request,
                                           HttpResponseMessage response,
                                           object body,
                                           Exception innerException)
            : base(request, response, body, innerException)
        {
        }


        protected PreconditionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public new TBody Body
        {
            get { return (TBody)base.Body; }
        }
    }

    [Serializable]
    public class PreconditionFailedException : WebClientException
    {
        public PreconditionFailedException(HttpRequestMessage request,
                                           HttpResponseMessage response,
                                           object body,
                                           Exception innerException)
            : base(request, response, body, innerException)
        {
        }


        protected PreconditionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}