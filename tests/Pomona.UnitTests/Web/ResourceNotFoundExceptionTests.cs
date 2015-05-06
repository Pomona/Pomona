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

using NUnit.Framework;

using Pomona.Common.Web;

using RNFException = Pomona.Common.Web.ResourceNotFoundException;

namespace Pomona.UnitTests.Web
{
    [TestFixture]
    public class ResourceNotFoundExceptionTests
    {
        [Test]
        public void Constructor_WithNoRequest_ReturnsExpectedMessage()
        {
            var response = new HttpResponse(HttpStatusCode.NotFound);
            var exception = new RNFException(null, response);
            Console.WriteLine(exception);

            Assert.That(exception.Message,
                        Is.EqualTo("The request failed with '404 NotFound'."));
        }


        [Test]
        public void Constructor_WithNoRequestAndEmptyUri_ReturnsExpectedMessage()
        {
            var response = new HttpResponse(HttpStatusCode.NotFound);
            var exception = new RNFException(null, response);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The request failed with '404 NotFound'."));
        }


        [Test]
        public void Constructor_WithNoRequestAndNoResponse_ReturnsExpectedMessage()
        {
            var exception = new RNFException(null, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The request got no response."));
        }


        [Test]
        public void Constructor_WithNoResponse_ReturnsExpectedMessage()
        {
            var request = new HttpRequest("http://example.com/", method : "GET");
            var exception = new RNFException(request, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The GET request to <http://example.com/> got no response."));
        }


        [Test]
        public void Constructor_WithNoResponseEmptyUriAndNoMethod_ReturnsExpectedMessage()
        {
            var request = new HttpRequest("");
            var exception = new RNFException(request, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The request got no response."));
        }


        [Test]
        public void Constructor_WithNoResponseNoUriAndEmptyMethod_ReturnsExpectedMessage()
        {
            var request = new HttpRequest(null, method : "");
            var exception = new RNFException(request, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The request got no response."));
        }


        [Test]
        public void Constructor_WithNoResponseNoUriAndNoMethod_ReturnsExpectedMessage()
        {
            var request = new HttpRequest(null);
            var exception = new RNFException(request, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The request got no response."));
        }


        [Test]
        public void Constructor_WithRequestAndResponse_ReturnsExpectedMessage()
        {
            var request = new HttpRequest("http://example.com/", method : "GET");
            var response = new HttpResponse(HttpStatusCode.NotFound);
            var exception = new RNFException(request, response);
            Console.WriteLine(exception);

            Assert.That(exception.Message,
                        Is.EqualTo("The GET request to <http://example.com/> failed with '404 NotFound'."));
        }


        [Test]
        public void Constructor_WithRequestAndResponseAndBodyWithMessage_ReturnsExpectedMessage()
        {
            var request = new HttpRequest("http://example.com/", method : "GET");
            var response = new HttpResponse(HttpStatusCode.NotFound);
            var exception = new RNFException(request, response, new
            {
                Message = "HALP!"
            });
            Console.WriteLine(exception);

            Assert.That(exception.Message,
                        Is.EqualTo("The GET request to <http://example.com/> failed with '404 NotFound': HALP!"));
        }
    }
}