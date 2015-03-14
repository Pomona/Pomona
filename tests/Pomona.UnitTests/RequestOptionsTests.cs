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

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class RequestOptionsTests
    {
        private void AssertRequestOptionCall(Action<IRequestOptions> options,
            Action<HttpRequest> assertions,
            string uri = "http://whatever")
        {
            var requestMessage = new HttpRequest(uri,
                null,
                "GET");
            var requestOptions = new RequestOptions();
            options(requestOptions);
            requestOptions.ApplyRequestModifications(requestMessage);
            assertions(requestMessage);
        }


        [Test]
        public void AppendQueryParameter_ToUrlWithExistingQueryString_ResultsInCorrectUrl()
        {
            AssertRequestOptionCall(x => x.AppendQueryParameter("foo", "bar mann"),
                rm => Assert.That(rm.Uri, Is.EqualTo("http://whatever?blob=knabb&foo=bar+mann")),
                "http://whatever?blob=knabb");
        }


        [Test]
        public void AppendQueryParameter_ToUrlWithNoQueryString_ResultsInCorrectUrl()
        {
            AssertRequestOptionCall(x => x.AppendQueryParameter("foo", "bar mann"),
                rm => Assert.That(rm.Uri, Is.EqualTo("http://whatever?foo=bar+mann")));
        }
    }
}