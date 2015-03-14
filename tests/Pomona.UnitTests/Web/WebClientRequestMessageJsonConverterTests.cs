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

using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

using Pomona.Common.Web;
using Pomona.UnitTests.TestHelpers.Web;

namespace Pomona.UnitTests.Web
{
    [TestFixture]
    public class WebClientRequestMessageJsonConverterTests : JsonConverterTestsBase<WebClientRequestMessage>
    {
        [Test]
        public void CanConvert_returns_true_for_WebClientRequestMessage()
        {
            Assert.IsTrue(Converter.CanConvert(typeof(WebClientRequestMessage)));
        }


        [Test]
        public void ReadJson_with_binary_body_deserializes_request()
        {
            var expected = new WebClientRequestMessage("http://test/lupus", new byte[] { 0xde, 0xad, 0xbe, 0xef }, "GET");
            var input = "{'method':'GET','url':'http://test/lupus','format':'binary','body':'3q2+7w=='}";
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        [Test]
        public void ReadJson_with_headers_deserializes_request()
        {
            var expected = new WebClientRequestMessage("http://test/lupus", null, "GET", new HttpHeaders
                                                                                         {
                                                                                             { "Accept", "boom" },
                                                                                             { "Baha", new List<string>() { "ha", "hi" } }
                                                                                         });
            var input = "{'method':'GET','url':'http://test/lupus','format':'json','headers':{'Accept':'boom','Baha':['ha', 'hi']}}";
            ReadJsonAssertEquals(input, expected);
        }


        [Test]
        public void ReadJson_with_json_body_deserializes_request()
        {
            var expected = new WebClientRequestMessage("http://test/lupus", Encoding.UTF8.GetBytes("{foo:'bar'}"), "GET",
                                                       new HttpHeaders() { ContentType = "application/json; charset=utf-8" });
            var input =
                "{'method':'GET','url':'http://test/lupus',headers:{'Content-Type':'application/json; charset=utf-8'},'format':'json','body':{'foo':'bar'}}";
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        [Test]
        public void ReadJson_with_text_body_deserializes_request()
        {
            var expected = new WebClientRequestMessage("http://test/lupus", Encoding.UTF8.GetBytes("Broken science"), "GET");
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
            var webClientRequestMessage = new WebClientRequestMessage("http://test/lupus", new byte[] { 0xde, 0xad, 0xbe, 0xef }, "GET",
                                                                      new HttpHeaders() { { "Content-Type", "image/png" } });
            WriteJsonAssertEquals(webClientRequestMessage, expected);
        }


        [Test]
        public void WriteJson_with_headers_serializes_request()
        {
            var expected =
                JObject.Parse(
                    "{method: 'GET', url: 'http://test/lupus', headers: { 'Content-Type': 'application/json', 'Baha' : ['ha','hi'] } }");
            var webClientRequestMessage = new WebClientRequestMessage("http://test/lupus", null, "GET",
                                                                      new HttpHeaders
                                                                      {
                                                                          { "Content-Type", "application/json" },
                                                                          { "Baha", new List<string>() { "ha", "hi" } }
                                                                      });
            WriteJsonAssertEquals(webClientRequestMessage, expected);
        }


        [Test]
        public void WriteJson_with_json_body_serializes_request()
        {
            var expected =
                JObject.Parse(
                    "{method: 'GET', url: 'http://test/lupus', headers: { 'Content-Type': 'application/json; charset=utf-8' }, format: 'json', body: { foo: 'bar' } }");
            var webClientRequestMessage = new WebClientRequestMessage("http://test/lupus", Encoding.UTF8.GetBytes("{ foo: 'bar' }"), "GET",
                                                                      new HttpHeaders()
                                                                      {
                                                                          {
                                                                              "Content-Type",
                                                                              "application/json; charset=utf-8"
                                                                          }
                                                                      });
            WriteJsonAssertEquals(webClientRequestMessage, expected);
        }


        [Test]
        public void WriteJson_with_no_body_serializes_request()
        {
            var expected = JObject.Parse("{method: 'GET', url: 'http://test/lupus'}");
            var webClientRequestMessage = new WebClientRequestMessage("http://test/lupus", null, "GET");
            WriteJsonAssertEquals(webClientRequestMessage, expected);
        }


        protected override void AssertObjectEquals(WebClientRequestMessage expected, WebClientRequestMessage actual)
        {
            Assert.That(expected.Uri, Is.EqualTo(actual.Uri));
            Assert.That(expected.Method, Is.EqualTo(actual.Method));
            foreach (var kvp in expected.Headers.Join(actual.Headers, x => x.Key, x => x.Key, (x, y) => new { x, y }))
                Assert.That(kvp.x.Value, Is.EquivalentTo(kvp.y.Value));
            if (expected.Headers.MediaType == "application/json" && expected.Data != null && actual.Headers.MediaType == "application/json"
                && actual.Data != null)
            {
                var expectedJson = JToken.Parse(Encoding.UTF8.GetString(expected.Data));
                var actualJson = JToken.Parse(Encoding.UTF8.GetString(actual.Data));

                Assert.That(JToken.DeepEquals(expectedJson, actualJson),
                            string.Format("Expected:\r\n{0}\r\nActual:\r\n{1}\r\n", expectedJson, actualJson));
            }
            else
                Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }


        protected override JsonConverter CreateConverter()
        {
            return new WebClientRequestMessageConverter();
        }
    }
}