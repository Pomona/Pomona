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

using System;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public class ClientRequestLogEventArgs : EventArgs
    {
        private readonly WebClientRequestMessage request;
        private readonly WebClientResponseMessage response;

        private readonly Exception thrownException;

        public ClientRequestLogEventArgs(WebClientRequestMessage request, WebClientResponseMessage response,
                                         Exception thrownException)
        {
            if (request == null) throw new ArgumentNullException("request");
            this.request = request;
            this.response = response;
            this.thrownException = thrownException;
        }

        public WebClientRequestMessage Request
        {
            get { return request; }
        }

        public WebClientResponseMessage Response
        {
            get { return response; }
        }

        public Exception ThrownException
        {
            get { return thrownException; }
        }

        public string Uri
        {
            get { return request.Uri; }
        }

        public string Method
        {
            get { return request.Method; }
        }
    }
}