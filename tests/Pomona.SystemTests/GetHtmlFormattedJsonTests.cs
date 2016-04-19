#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net;
using System.Net.Http;

using CsQuery;
using CsQuery.Output;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;

using HttpMethod = System.Net.Http.HttpMethod;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class GetHtmlFormattedJsonTests : ClientTestsBase
    {
        [Test]
        public void Get_AcceptIsHtmlText_ReturnsJsonAsHtmlDocument()
        {
            var response =
                WebClient.SendSync(new HttpRequestMessage(new HttpMethod("GET"), "http://test/critters")
                {
                    Headers = { { "Accept", "text/html" } }
                });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            CQ dom = response.Content.ReadAsStringAsync().Result;
            var jsonContent = HttpUtility.HtmlDecode(dom["pre"].RenderSelection(new FormatPlainText()));
            Console.WriteLine(jsonContent);
            Assert.DoesNotThrow(() => JObject.Parse(jsonContent));
        }
    }
}