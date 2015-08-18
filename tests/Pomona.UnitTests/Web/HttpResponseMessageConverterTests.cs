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

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

using Pomona.Common.Web;
using Pomona.UnitTests.TestHelpers.Web;

namespace Pomona.UnitTests.Web
{
    [TestFixture]
    public class HttpResponseMessageConverterTests : JsonConverterTestsBase<HttpResponseMessage>
    {
        [Test]
        public void CanConvert_returns_true_for_HttpResponse()
        {
            Assert.IsTrue(Converter.CanConvert(typeof(HttpResponseMessage)));
        }


        [Test]
        public void ReadJson_with_headers_deserializes_request()
        {
            var expected = new HttpResponseMessage(HttpStatusCode.Accepted) { Headers = { { "Accept", "boom" } } };
            var input = "{'statusCode':202,'format':'json','headers':{'Accept':['boom']}}";
            ReadJsonAssertEquals(input, expected);
        }


        [Test]
        public void ReadJson_with_json_body_deserializes_request()
        {
            var expected = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("{foo:'bar'}")
                {
                    Headers = { ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8") }
                }
            };
            var input = "{'statusCode':202,'format':'json',headers:{'Content-Type':'application/json; charset=utf-8'},'body':{'foo':'bar'}}";
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        [Test]
        public void WriteJson_with_headers_serializes_request()
        {
            var expected = JObject.Parse("{'statusCode':202,'headers':{'Boof':'lala'}}");
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            { Headers = { { "Boof", "lala" } } };
            WriteJsonAssertEquals(response, expected);
        }


        [Test]
        public void WriteJson_with_json_body_serializes_request()
        {
            var expected =
                JObject.Parse(
                    "{'statusCode':202,'format':'json',headers:{'Content-Type':'application/json; charset=utf-8'},'body':{'foo':'bar'}}");
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content =
                    new StringContent("{ foo: 'bar' }")
                    {
                        Headers = { ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8") }
                    }
            };

            WriteJsonAssertEquals(response, expected);
        }


        [Test]
        public void WriteJson_with_no_body_serializes_request()
        {
            var expected = JObject.Parse("{'statusCode':202}");
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            WriteJsonAssertEquals(response, expected);
        }


        protected override void AssertObjectEquals(HttpResponseMessage expected, HttpResponseMessage actual)
        {
            Assert.That(expected.StatusCode, Is.EqualTo(actual.StatusCode));
            foreach (var kvp in expected.Headers.Join(actual.Headers, x => x.Key, x => x.Key, (x, y) => new { x, y }))
                Assert.That(kvp.x.Value, Is.EquivalentTo(kvp.y.Value));
            AssertHttpContentEquals(expected.Content, actual.Content);
        }


        protected override JsonConverter CreateConverter()
        {
            return new HttpResponseMessageConverter();
        }
    }
}