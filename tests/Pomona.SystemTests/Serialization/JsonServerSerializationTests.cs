#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Pomona.Nancy;

namespace Pomona.SystemTests.Serialization
{
    [TestFixture]
    public class JsonServerSerializationTests
    {
        private TypeMapper typeMapper;


        [Test]
        public void Serialize_AnonymousObject_with_resource_cast_to_object_and_expanded_is_successful()
        {
            var obj = new
            {
                Method = "POST",
                Resource = (object)new Hat("fedora") { Id = 1337 }
            };

            var jobj = SerializeAndGetJsonObject(obj, "resource");
            var expected = JObject.Parse(@"{
  ""method"": ""POST"",
  ""resource"": {
    ""_uri"": ""http://test/hats/1337"",
    ""_type"": ""Hat"",
    ""hatType"": ""fedora"",
    ""style"": null,
    ""id"": 1337
  }
}");

            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }


        [Test]
        public void Serialize_AnonymousObject_with_resource_cast_to_object_is_successful()
        {
            var obj = new
            {
                Method = "POST",
                Resource = (object)new Hat("fedora") { Id = 1337 }
            };

            var jobj = SerializeAndGetJsonObject(obj);
            var expected = JObject.Parse(@"{
  ""method"": ""POST"",
  ""resource"": {
    ""_ref"": ""http://test/hats/1337"",
    ""_type"": ""Hat"",
  }
}");

            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }


        [Test]
        public void Serialize_AnonymousObject_with_resource_is_successful()
        {
            var obj = new
            {
                Method = "POST",
                Resource = new Hat("fedora") { Id = 1337 }
            };

            var jobj = SerializeAndGetJsonObject(obj);
            var expected = JObject.Parse(@"{
  ""method"": ""POST"",
  ""resource"": {
    ""_ref"": ""http://test/hats/1337""
  }
}");

            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }


        [Test]
        public void Serialize_dictionary_in_object_array_is_successful()
        {
            var obj = new { items = new object[] { new Dictionary<string, string>() { { "foo", "bar" } } } };

            var jobj = SerializeAndGetJsonObject(obj);
            var expected = JObject.Parse(@"{""items"":[{""foo"":""bar""}]}");

            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }


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
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, range, null);
            }

            processMeter.Stop();
        }


        [Test]
        public void Serialize_object_array_in_object_array_is_successful()
        {
            var obj = new
            {
                nested = new object[]
                {
                    new object[] { 1, "foo", new object[] { "kra" } }
                }
            };

            var jobj = SerializeAndGetJsonObject(obj);
            var expected = JObject.Parse(@"{
  ""nested"": [
    [
      {
        ""_type"": ""Int32"",
        ""value"": 1
      },
      ""foo"",
      [
        ""kra""
      ]
    ]
  ]
}
");

            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
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


        [Test]
        public void Serialize_QueryResult_with_DebugInfo_is_successful()
        {
            var qr = QueryResult.Create(new List<Hat>() { new Hat("fedora") { Id = 1337 } }, 0, 4,
                                        "http://prev", "http://next");
            qr.DebugInfo["haha"] = "hihi";
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
  ""next"": ""http://next"",
  ""debugInfo"": {
    ""haha"": ""hihi""
  }
}");
            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }


        [Test(Description = "It might be useful to serialize unknown and anonymous classes the same way.")]
        [Category("TODO")]
        public void Serialize_Unkown_class_is_successful()
        {
            var obj = new SerializeMe
            {
                Name = "Chuck Norris"
            };

            var jobj = SerializeAndGetJsonObject(obj);
            var expected = JObject.Parse(@"{
  ""method"": ""POST"",
  ""resource"": {
    ""_ref"": ""http://test/hats/1337"",
    ""_type"": ""Hat"",
  }
}");

            Assert.That(jobj.ToString(), Is.EqualTo(expected.ToString()));
        }


        [Test(Description = "It might be useful to serialize unknown and anonymous classes the same way.")]
        [Category("TODO")]
        public void Serialize_Unkown_object_is_successful()
        {
            object obj = new SerializeMe
            {
                Name = "Chuck Norris"
            };

            var jobj = SerializeAndGetJsonObject(obj);
            var expected = JObject.Parse(@"{
  ""method"": ""POST"",
  ""resource"": {
    ""_ref"": ""http://test/hats/1337"",
    ""_type"": ""Hat"",
  }
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


        private JObject SerializeAndGetJsonObject<T>(T value, string expandedPaths = null)
        {
            var serializer = GetSerializer();
            Console.WriteLine("Serialized object to json:");
            string jsonString;
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, value, new SerializeOptions
                {
                    ExpandedPaths = expandedPaths
                });
                jsonString = stringWriter.ToString();
                Console.WriteLine(jsonString);
            }

            return (JObject)JToken.Parse(jsonString);
        }


        private class ProcessMeter
        {
            private readonly PropertyInfo[] meterProperties;
            private readonly IDictionary<PerformanceCounter, CounterSample> performanceCounters;
            private IDictionary<string, long> before;
            private long survivedMemorySizeBefore;
            private long totalAllocatedMemorySizeBefore;
            private long totalProcessorTimeBefore;


            static ProcessMeter()
            {
                AppDomain.MonitoringIsEnabled = true;
            }


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

                var instanceName = Process.GetCurrentProcess().ProcessName;
                this.performanceCounters = new Dictionary<PerformanceCounter, CounterSample>
                {
                    { new PerformanceCounter(".NET CLR Memory", "Large Object Heap Size", instanceName, true), CounterSample.Empty }
                };
            }


            public void Start()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                this.survivedMemorySizeBefore = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize;
                this.totalAllocatedMemorySizeBefore = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
                this.totalProcessorTimeBefore = AppDomain.CurrentDomain.MonitoringTotalProcessorTime.Ticks;

                foreach (var kv in this.performanceCounters.ToDictionary(kv => kv.Key, kv => kv.Value))
                {
                    var performanceCounter = kv.Key;
                    var sample = performanceCounter.NextSample();
                    this.performanceCounters[performanceCounter] = sample;
                }

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

                var survivedMemorySizeAfter = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize;
                var totalAllocatedMemorySizeAfter = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
                var totalProcessorTimeAfter = AppDomain.CurrentDomain.MonitoringTotalProcessorTime.Ticks;

                var survivedMemorySizeDifference = new Difference(this.survivedMemorySizeBefore, survivedMemorySizeAfter);
                var totalAllocatedMemorySizeDifference = new Difference(this.totalAllocatedMemorySizeBefore, totalAllocatedMemorySizeAfter);
                var totalProcessorTimeDifference = new Difference(this.totalProcessorTimeBefore, totalProcessorTimeAfter);

                survivedMemorySizeDifference.Write("SurvivedMemorySize");
                totalAllocatedMemorySizeDifference.Write("TotalAllocatedMemorySize");
                totalProcessorTimeDifference.Write("TotalProcessorTime");

                foreach (var kv in this.performanceCounters)
                {
                    var performanceCounter = kv.Key;
                    var beforeValue = kv.Value.RawValue;
                    var afterValue = performanceCounter.NextSample().RawValue;
                    var difference = new Difference(beforeValue, afterValue);

                    difference.Write(performanceCounter.CounterName);
                }

                foreach (var property in this.meterProperties)
                {
                    var beforeValue = this.before[property.Name];
                    var value = property.GetValue(process, null);
                    long afterValue;
                    if (value is int)
                        afterValue = (int)value;
                    else
                        afterValue = (long)value;

                    var difference = new Difference(beforeValue, afterValue);
                    difference.Write(property.Name);
                }
            }


            private struct Difference : IFormattable
            {
                private readonly long after;
                private readonly long before;
                private readonly long difference;
                private readonly string percentDifferenceString;


                public Difference(long before, long after)
                {
                    this.before = before;
                    this.after = after;
                    this.difference = after - before;
                    var differencePercent = ((double)(this.after - this.before) / this.before) * 100;
                    this.percentDifferenceString = String.Concat('(', differencePercent.ToString("n0"), " %)");
                }


                public override string ToString()
                {
                    return this.percentDifferenceString;
                }


                public void Write(string header)
                {
                    Console.WriteLine();
                    Console.WriteLine(header);
                    Console.WriteLine("- Before:     {0,16:n0} bytes", this.before);
                    Console.WriteLine("- After:      {0,16:n0} bytes", this.after);

                    if (this.difference > 0)
                        Console.WriteLine("- Increase:   {0,16:n0} bytes {1,10}", this.difference, this.percentDifferenceString);
                    else if (this.difference < 0)
                        Console.WriteLine("- Decrease:   {0,16:n0} bytes {1,10}", this.difference, this.percentDifferenceString);
                    else
                        Console.WriteLine("- Difference: {0,16:n0} bytes {1,10}", this.difference, this.percentDifferenceString);
                }


                public string ToString(string format, IFormatProvider formatProvider)
                {
                    return this.difference.ToString(format, formatProvider);
                }
            }
        }

        private class SerializeMe
        {
            public string Name { get; set; }
        }
    }
}