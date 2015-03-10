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
using System.Collections.Generic;

using Nancy;

using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.SystemTests.Serialization
{
    [TestFixture]
    public class JsonServerSerializationTests
    {
        private TypeMapper typeMapper;


        [Test]
        public void Serialize_QueryResult_of_resource_is_successful()
        {
            var qr = QueryResult.Create(new List<Hat>() { new Hat("fedora") { Id = 1337 } }, 0, 4,
                                        "http://prev", "http://next");
            var jobj = SerializeAndGetJsonObject(qr);
            var expected = JObject.Parse(@"{
  ""_type"": ""__result__"",
  ""totalCount"": 4,
  ""items"": [
    {
      ""_uri"": ""http://test/hats/1337"",
      ""hatType"": ""fedora"",
      ""style"": null,
      ""id"": 1337
    }
  ],
  ""skip"": 0,
  ""previous"": ""http://prev"",
  ""next"": ""http://next""
}");
            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new TypeMapper(new CritterPomonaConfiguration());
        }

        #endregion

        private JObject SerializeAndGetJsonObject<T>(T value)
        {
            var serializerFactory =
                new PomonaJsonSerializerFactory();
            var nancyContext = new NancyContext();
            nancyContext.Request = new Request("Get", "http://test");
            var contextProvider =
                new ServerSerializationContextProvider(this.typeMapper,
                                                       new UriResolver(this.typeMapper, new BaseUriProvider(nancyContext, "/")),
                                                       Substitute.For<IResourceResolver>(), new NoContainer());
            var serializer = serializerFactory.GetSerializer(contextProvider);
            Console.WriteLine("Serialized object to json:");
            var jsonString = serializer.SerializeToString(value);
            Console.WriteLine(jsonString);

            return (JObject)JToken.Parse(jsonString);
        }
    }
}