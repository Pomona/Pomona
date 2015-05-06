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

using Pomona.Common.Web;

namespace Pomona.Common
{
    public class ClientRequestLogEventArgs : EventArgs
    {
        private readonly HttpRequest request;
        private readonly HttpResponse response;
        private readonly Exception thrownException;


        public ClientRequestLogEventArgs(HttpRequest request,
                                         HttpResponse response,
                                         Exception thrownException)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            this.request = request;
            this.response = response;
            this.thrownException = thrownException;
        }


        public string Method
        {
            get { return this.request.Method; }
        }

        public HttpRequest Request
        {
            get { return this.request; }
        }

        public HttpResponse Response
        {
            get { return this.response; }
        }

        public Exception ThrownException
        {
            get { return this.thrownException; }
        }

        public string Uri
        {
            get { return this.request.Uri; }
        }


        public override string ToString()
        {
            return string.Format("Request:\r\n{0}\r\nResponse:\r\n{1}\r\n", Request, Response);
        }
    }
}