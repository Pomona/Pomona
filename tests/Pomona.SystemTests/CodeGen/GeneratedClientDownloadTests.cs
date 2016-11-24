#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net;
using System.Net.Http;

using NUnit.Framework;

using Pomona.Common.Web;

namespace Pomona.SystemTests.CodeGen
{
    [TestFixture]
    public class GeneratedClientDownloadTests : ClientTestsBase
    {
        public override bool UseSelfHostedHttpServer => true;


        [Test]
        public void GetClientDll_ConfigurationHasErrors_Returns500InternalServerError()
        {
            var uri = new Uri(new Uri(Client.BaseUri), "incorrect/Incorrect.Client.dll");
            var response = WebClient.SendSync(new HttpRequestMessage(HttpMethod.Get, uri));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("text/plain"));
        }


        [Test]
        [Category("FailsOnFileShare")]
        public void GetClientDll_ConfigurationIsCorrect_ReturnsClientDll()
        {
            var uri = new Uri(new Uri(Client.BaseUri), "Critters.Client.dll");
            var response = WebClient.SendSync(new HttpRequestMessage(HttpMethod.Get, uri));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Failed to download <{uri}>.");
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("binary/octet-stream"));
            Assert.That(response.Content.Headers.ContentLength, Is.GreaterThan(0));
        }


        [Test]
        public void GetClientNupkg_ConfigurationHasErrors_Returns500InternalServerError()
        {
            var uri = new Uri(new Uri(Client.BaseUri), "incorrect/client.nupkg");
            var response = WebClient.SendSync(new HttpRequestMessage(HttpMethod.Get, uri));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("text/plain"));
        }


        [Test]
        [Category("FailsOnFileShare")]
        public void GetClientNupkg_ConfigurationIsCorrect_ReturnsClientNupkg()
        {
            var uri = new Uri(new Uri(Client.BaseUri), "client.nupkg");
            var response = WebClient.SendSync(new HttpRequestMessage(HttpMethod.Get, uri));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Failed to download <{uri}>.");
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/zip"));
            Assert.That(response.Content.Headers.ContentLength, Is.GreaterThan(0));
        }
    }
}