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
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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
        [Explicit("This is a performance test and should only be run when doing performance optimization")]
        public void Serialize_LargeAmountOfObjects()
        {
            var processMeter = new ProcessMeter();
            processMeter.Start();

            var range = Enumerable.Range(0, 1000).Select(i => QueryResult.Create(new List<Hat>
            {
                new Hat(new String('A', 85000)) { Id = i }
            }, 0, 4, "http://prev", "http://next"));

            var serializer = GetSerializer();
            serializer.SerializeToString(range);

            processMeter.Stop();
        }


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

        private PomonaJsonSerializer GetSerializer()
        {
            var serializerFactory = new PomonaJsonSerializerFactory();
            var nancyContext = new NancyContext { Request = new Request("Get", "http://test") };
            var uriResolver = new UriResolver(this.typeMapper, new BaseUriProvider(nancyContext, "/"));
            var contextProvider = new ServerSerializationContextProvider(this.typeMapper,
                                                                         uriResolver,
                                                                         Substitute.For<IResourceResolver>(), new NoContainer());
            var serializer = serializerFactory.GetSerializer(contextProvider);
            return serializer;
        }


        private JObject SerializeAndGetJsonObject<T>(T value)
        {
            var serializer = GetSerializer();
            Console.WriteLine("Serialized object to json:");
            var jsonString = serializer.SerializeToString(value);
            Console.WriteLine(jsonString);

            return (JObject)JToken.Parse(jsonString);
        }


        private class ProcessMeter
        {
            private readonly PropertyInfo[] meterProperties;
            private IDictionary<string, long> before;


            public ProcessMeter()
            {
                var allProperties = typeof(Process).GetProperties();
                
                var meterProperties64 = allProperties
                    .Where(p => p.PropertyType == typeof(long) && p.Name.Contains("64"))
                    .ToArray();

                var meterPropertyNames32 = meterProperties64
                    .Select(p => p.Name.Substring(0, p.Name.Length - 2));

                var meterProperties32 = allProperties
                    .Where(p => meterPropertyNames32.Contains(p.Name));

                this.meterProperties = meterProperties64
                    .Concat(meterProperties32)
                    .OrderBy(p => p.Name)
                    .ToArray();
            }


            public void Start()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var process = Process.GetCurrentProcess();
                this.before = new Dictionary<string, long>();
                foreach (var property in this.meterProperties)
                {
                    var value = property.GetValue(process, null);
                    long longValue;
                    
                    if (value is int)
                        longValue = (int)value;
                    else
                        longValue = (long)value;

                    this.before.Add(property.Name, longValue);
                }
            }


            public void Stop()
            {
                var process = Process.GetCurrentProcess();

                foreach (var property in this.meterProperties)
                {
                    var beforeValue = this.before[property.Name];
                    var value = property.GetValue(process, null);
                    long afterValue;
                    if (value is int)
                        afterValue = (int)value;
                    else
                        afterValue = (long)value;

                    Console.WriteLine();
                    Console.WriteLine(property.Name);
                    Console.WriteLine("- Before:     {0,16:n0} bytes", beforeValue);
                    Console.WriteLine("- After:      {0,16:n0} bytes", afterValue);

                    // It should be possible to do conditional formatting on positive, negative and zero with ';', but I couldn't get it to work in combination with 'n0'. @asbjornu
                    var difference = afterValue - beforeValue;
                    var differencePercent = ((double)(afterValue - beforeValue) / beforeValue) * 100;
                    var differencePercentString = String.Concat('(', differencePercent.ToString("n0"), " %)");

                    if (difference > 0)
                        Console.WriteLine("- Increase:   {0,16:n0} bytes {1,10}", difference, differencePercentString);
                    else if (difference < 0)
                        Console.WriteLine("- Decrease:   {0,16:n0} bytes {1,10}", difference, differencePercentString);
                    else
                        Console.WriteLine("- Difference: {0,16:n0} bytes {1,10}", difference, differencePercentString);
                }
            }
        }
    }
}