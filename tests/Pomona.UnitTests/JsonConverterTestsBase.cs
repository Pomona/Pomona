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
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace Pomona.UnitTests.TestHelpers.Web
{
    [TestFixture]
    public abstract class JsonConverterTestsBase<TObject>
    {
        public JsonConverter Converter { get; private set; }
        protected abstract JsonConverter CreateConverter();


        [SetUp]
        public void SetUp()
        {
            this.Converter = CreateConverter();
        }


        protected TObject ReadJson(JToken token)
        {
            var jsonSerializer = new JsonSerializer() { Converters = { this.Converter } };
            using (var jsonReader = new JTokenReader(token))
            {
                return (TObject)this.Converter.ReadJson(jsonReader, typeof(TObject), null, jsonSerializer);
            }
        }


        private JObject WriteJson(TObject value)
        {
            var jsonSerializer = new JsonSerializer() { Converters = { this.Converter } };
            using (var jsonWriter = new JTokenWriter())
            {
                this.Converter.WriteJson(jsonWriter, value, jsonSerializer);
                return (JObject)jsonWriter.Token;
            }
        }


        protected abstract void AssertObjectEquals(TObject expected, TObject actual);


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

            Assert.That(JToken.DeepEquals(result, expected), string.Format("Expected:\r\n{0}\r\nActual:\r\n{1}\r\n", expected, result));
        }
    }
}