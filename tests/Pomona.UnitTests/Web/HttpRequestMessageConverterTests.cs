#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;
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
    public class HttpRequestMessageConverterTests : JsonConverterTestsBase<HttpRequestMessage>
    {
        [Test]
        public void CanConvert_returns_true_for_request()
        {
            Assert.IsTrue(Converter.CanConvert(typeof(HttpRequestMessage)));
        }


        [Test]
        public void ReadJson_with_binary_body_deserializes_request()
        {
            var expected = new HttpRequestMessage(HttpMethod.Get, "http://test/lupus")
            {
                Content = new ByteArrayContent(new byte[] { 0xde, 0xad, 0xbe, 0xef })
                {
                    Headers = { { "Content-Type", "image/png" } }
                }
            };
            var input =
                "{'method':'GET','url':'http://test/lupus','headers':{'Content-Type':'image/png'},'format':'binary','body':'3q2+7w=='}";
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        [Test]
        public void ReadJson_with_headers_deserializes_request()
        {
            var expected = new HttpRequestMessage(HttpMethod.Get, "http://test/lupus")
            {
                Headers =
                {
                    { "Nonono", "boom" },
                    { "Baha", new List<string>() { "ha", "hi" } }
                }
            };
            var input = "{'method':'GET','url':'http://test/lupus','format':'json','headers':{'Nonono':'boom','Baha':['ha', 'hi']}}";
            ReadJsonAssertEquals(input, expected);
        }


        [Test]
        public void ReadJson_with_json_body_deserializes_request()
        {
            var expected = new HttpRequestMessage(HttpMethod.Get,
                                                  "http://test/lupus")
            {
                Content =
                    new StringContent("{foo:'bar'}")
                    {
                        Headers = { ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8") }
                    }
            };
            var input =
                "{'method':'GET','url':'http://test/lupus',headers:{'Content-Type':'application/json; charset=utf-8'},'format':'json','body':{'foo':'bar'}}";
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        [Test]
        public void ReadJson_with_text_body_deserializes_request()
        {
            var expected = new HttpRequestMessage(HttpMethod.Get, "http://test/lupus") { Content = new StringContent("Broken science") };
            var input = "{'method':'GET','url':'http://test/lupus','format':'text','body': 'Broken science'}";
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        [Test]
        public void WriteJson_with_binary_body_serializes_request()
        {
            var expected =
                JObject.Parse(
                    "{'method':'GET','url':'http://test/lupus','headers':{'Content-Type':'image/png'},'format':'binary','body':'3q2+7w=='}");
            var request = new HttpRequestMessage(HttpMethod.Get,
                                                 "http://test/lupus")
            {
                Content =
                    new ByteArrayContent(new byte[] { 0xde, 0xad, 0xbe, 0xef })
                    {
                        Headers = { { "Content-Type", "image/png" } }
                    }
            };
            WriteJsonAssertEquals(request, expected);
        }


        [Test]
        public void WriteJson_with_headers_serializes_request()
        {
            var expected =
                JObject.Parse(
                    "{method: 'GET', url: 'http://test/lupus', headers: { 'Nonono': 'lalala', 'Baha' : ['ha','hi'] } }");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test/lupus")
            {
                Headers =
                {
                    { "Nonono", "lalala" },
                    { "Baha", new List<string>() { "ha", "hi" } }
                }
            };
            WriteJsonAssertEquals(request, expected);
        }


        [Test]
        public void WriteJson_with_json_body_serializes_request()
        {
            var expected =
                JObject.Parse(
                    "{method: 'GET', url: 'http://test/lupus', headers: { 'Content-Type': 'application/json; charset=utf-8' }, format: 'json', body: { foo: 'bar' } }");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test/lupus")
            {
                Content = new StringContent("{ foo: 'bar' }")
                {
                    Headers =
                    {
                        ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8")
                    }
                }
            };
            WriteJsonAssertEquals(request, expected);
        }


        [Test]
        public void WriteJson_with_no_body_serializes_request()
        {
            var expected = JObject.Parse("{method: 'GET', url: 'http://test/lupus'}");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test/lupus");
            WriteJsonAssertEquals(request, expected);
        }


        protected override void AssertObjectEquals(HttpRequestMessage expected, HttpRequestMessage actual)
        {
            Assert.That(expected.RequestUri, Is.EqualTo(actual.RequestUri));
            Assert.That(expected.Method, Is.EqualTo(actual.Method));
            foreach (var kvp in expected.Headers.Join(actual.Headers, x => x.Key, x => x.Key, (x, y) => new { x, y }))
                Assert.That(kvp.x.Value, Is.EquivalentTo(kvp.y.Value));
            var expectedContent = expected.Content;
            var actualContent = actual.Content;
            AssertHttpContentEquals(expectedContent, actualContent);
        }


        protected override JsonConverter CreateConverter()
        {
            return new HttpRequestMessageConverter();
        }
    }
}