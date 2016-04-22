#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;
using System.Linq;
using System.Net.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace Pomona.UnitTests.TestHelpers.Web
{
    [TestFixture]
    public abstract class JsonConverterTestsBase<TObject>
    {
        public JsonConverter Converter { get; private set; }


        [SetUp]
        public void SetUp()
        {
            Converter = CreateConverter();
        }


        protected static void AssertHttpContentEquals(HttpContent expected, HttpContent actual)
        {
            if (expected != null && actual != null)
            {
                foreach (var kvp in expected.Headers
                                            .Join(actual.Headers, x => x.Key, x => x.Key, (x, y) => new { x, y }))
                    Assert.That(kvp.x.Value, Is.EquivalentTo(kvp.y.Value));

                if (expected.Headers.ContentType.MediaType == "application/json")
                {
                    var expectedJson = JToken.Parse(expected.ReadAsStringAsync().Result);
                    var actualJson = JToken.Parse(actual.ReadAsStringAsync().Result);

                    Assert.That(JToken.DeepEquals(expectedJson, actualJson),
                                $"Expected:\r\n{expectedJson}\r\nActual:\r\n{actualJson}\r\n");
                }
                else
                {
                    var expectedBytes = expected.ReadAsByteArrayAsync().Result;
                    var actualBytes = actual.ReadAsByteArrayAsync().Result;
                    Assert.That(actualBytes, Is.EqualTo(expectedBytes));
                }
            }
            else
                Assert.That(expected, Is.EqualTo(actual));
        }


        protected abstract void AssertObjectEquals(TObject expected, TObject actual);
        protected abstract JsonConverter CreateConverter();


        protected TObject ReadJson(JToken token)
        {
            var jsonSerializer = new JsonSerializer() { Converters = { Converter } };
            using (var jsonReader = new JTokenReader(token))
            {
                return (TObject)Converter.ReadJson(jsonReader, typeof(TObject), null, jsonSerializer);
            }
        }


        protected void ReadJsonAssertEquals(string input, TObject expected)
        {
            var result = ReadJson(JToken.Parse(input));
            AssertObjectEquals(expected, result);
        }


        protected void WriteJsonAssertEquals(TObject obj, JObject expected)
        {
            var result = WriteJson(obj);

            using (var sw = new StringWriter())
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    jw.QuoteChar = '\'';
                    result.WriteTo(jw);
                    Console.WriteLine("\"{0}\"", sw.ToString().Replace("\"", "\\\""));
                }
            }

            Assert.That(JToken.DeepEquals(result, expected), $"Expected:\r\n{expected}\r\nActual:\r\n{result}\r\n");
        }


        private JObject WriteJson(TObject value)
        {
            var jsonSerializer = new JsonSerializer() { Converters = { Converter } };
            using (var jsonWriter = new JTokenWriter())
            {
                Converter.WriteJson(jsonWriter, value, jsonSerializer);
                return (JObject)jsonWriter.Token;
            }
        }
    }
}