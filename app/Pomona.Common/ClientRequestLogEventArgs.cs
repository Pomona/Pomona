#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net.Http;

namespace Pomona.Common
{
    public class ClientRequestLogEventArgs : EventArgs
    {
        public ClientRequestLogEventArgs(HttpRequestMessage request,
                                         HttpResponseMessage response,
                                         Exception thrownException)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            Request = request;
            Response = response;
            ThrownException = thrownException;
        }


        public string Method
        {
            get { return Request.Method.ToString(); }
        }

        public HttpRequestMessage Request { get; }

        public HttpResponseMessage Response { get; }

        public Exception ThrownException { get; }

        public string Uri
        {
            get { return Request.RequestUri.ToString(); }
        }


        public override string ToString()
        {
            return string.Format("Request:\r\n{0}\r\nResponse:\r\n{1}\r\n", Request, Response);
        }
    }
}