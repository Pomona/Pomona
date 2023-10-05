#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Net;
using System.Net.Http;

using NUnit.Framework;

using RNFException = Pomona.Common.Web.ResourceNotFoundException;

namespace Pomona.UnitTests.Web
{
    [TestFixture]
    public class ResourceNotFoundExceptionTests
    {
        [Test]
        public void Constructor_WithNoRequest_ReturnsExpectedMessage()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var exception = new RNFException(null, response);
            Console.WriteLine(exception);

            Assert.That(exception.Message,
                        Is.EqualTo("The request failed with '404 NotFound'."));
        }


        [Test]
        public void Constructor_WithNoRequestAndEmptyUri_ReturnsExpectedMessage()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
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
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");
            var exception = new RNFException(request, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The GET request to <http://example.com/> got no response."));
        }


        [Test]
        public void Constructor_WithNoResponseAndNoUriReturnsExpectedMessage()
        {
            var request = new HttpRequestMessage();
            var exception = new RNFException(request, null);
            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.EqualTo("The GET request got no response."));
        }


        [Test]
        public void Constructor_WithRequestAndResponse_ReturnsExpectedMessage()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var exception = new RNFException(request, response);
            Console.WriteLine(exception);

            Assert.That(exception.Message,
                        Is.EqualTo("The GET request to <http://example.com/> failed with '404 NotFound'."));
        }


        [Test]
        public void Constructor_WithRequestAndResponseAndBodyWithMessage_ReturnsExpectedMessage()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
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

