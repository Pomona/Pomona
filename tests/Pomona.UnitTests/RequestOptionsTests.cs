#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net.Http;

using NUnit.Framework;

using Pomona.Common;

using HttpMethod = System.Net.Http.HttpMethod;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class RequestOptionsTests
    {
        [Test]
        public void AppendQueryParameter_ToUrlWithExistingQueryString_ResultsInCorrectUrl()
        {
            AssertRequestOptionCall(x => x.AppendQueryParameter("foo", "bar mann"),
                                    rm => Assert.That(rm.RequestUri.ToString(), Is.EqualTo("http://whatever/?blob=knabb&foo=bar+mann")),
                                    "http://whatever/?blob=knabb");
        }


        [Test]
        public void AppendQueryParameter_ToUrlWithNoQueryString_ResultsInCorrectUrl()
        {
            AssertRequestOptionCall(x => x.AppendQueryParameter("foo", "bar mann"),
                                    rm => Assert.That(rm.RequestUri.ToString(), Is.EqualTo("http://whatever/?foo=bar+mann")));
        }


        private void AssertRequestOptionCall(Action<IRequestOptions> options,
                                             Action<HttpRequestMessage> assertions,
                                             string uri = "http://whatever")
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var requestOptions = new RequestOptions();
            options(requestOptions);
            requestOptions.ApplyRequestModifications(requestMessage);
            assertions(requestMessage);
        }
    }
}