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

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;

namespace Pomona.UnitTests.Serialization.Json
{
    [TestFixture]
    public class PomonaJsonDeserializerTests
    {
        private PomonaJsonDeserializer deserializer;
        private TypeMapper typeMapper;


        [Test]
        public void Deserialize_empty_object_array_is_successful()
        {
            var array = this.deserializer.DeserializeString<object[]>("[]");
            Assert.That(array.Length, Is.EqualTo(0));
        }


        [Test]
        public void Deserialize_null_to_non_nullable_value_throws_PomonaSerializationException()
        {
            var ex = Assert.Throws<PomonaSerializationException>(
                () => this.deserializer.DeserializeString("null", options : new DeserializeOptions() { ExpectedBaseType = typeof(decimal) }));
            Assert.That(ex.Message, Is.EqualTo("Deserialized to null, which is not allowed value for casting to type System.Decimal"));
        }


        [Test]
        public void Deserialize_object_array_in_object_array_is_successful()
        {
            var array = this.deserializer.DeserializeString<object[]>(@"[[{""_type"": ""Int32"",""value"":1},""bah""]]");
            Assert.That(array.Length, Is.EqualTo(1));
            Assert.That(array, Is.InstanceOf<object[]>());
            var innerArray = (object[])array[0];
            Assert.That(innerArray[0], Is.EqualTo(1));
            Assert.That(innerArray[1], Is.EqualTo("bah"));
        }


        [Test]
        public void Deserialize_object_array_with_integer_is_successful()
        {
            var array = this.deserializer.DeserializeString<object[]>(@"[{""_type"":""Int32"",""value"":1337}]");
            Assert.That(array.Length, Is.EqualTo(1));
            Assert.That(array[0], Is.EqualTo(1337));
        }


        [Test]
        public void Deserialize_string_to_bool_value_throws_PomonaSerializationException()
        {
            var ex = Assert.Throws<PomonaSerializationException>(
                () =>
                    this.deserializer.DeserializeString("\"blahrg\"", options : new DeserializeOptions() { ExpectedBaseType = typeof(bool) }));

            // This will wrap a JsonSerializationException for now.
            Assert.That(ex.Message, Is.StringStarting("Error converting value \"blahrg\" to type 'System.Boolean'."));
            Assert.That(ex.InnerException, Is.InstanceOf<JsonSerializationException>());
        }


        [Test]
        public void Deserialize_typed_enumerable_in_anonymous_object_successful()
        {
            var obj = DeserializeAnonymous(new { Items = (IEnumerable<string>)null }, @"{""items"":[""foo"",""bar""]}");
            Assert.That(obj.Items.Count(), Is.EqualTo(2));
            Assert.That(obj.Items.ElementAt(0), Is.EqualTo("foo"));
            Assert.That(obj.Items.ElementAt(1), Is.EqualTo("bar"));
        }


        [Test]
        public void Deserialize_using_TypeMapper_QueryResult_when_specifying_expected_type_is_successful()
        {
            var qr =
                this.deserializer.DeserializeString<QueryResult<int>>(
                    @"{""_type"": ""__result__"",""totalCount"": 12,""count"": 7,""items"": [1,2,3,4,5,6,7]}");
            Assert.That(qr, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 7 }));
            Assert.That(qr.TotalCount, Is.EqualTo(12));
        }


        public T DeserializeAnonymous<T>(T anonTemplate, string serialized)
        {
            return this.deserializer.DeserializeString<T>(serialized);
        }


        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new Config().CreateSessionFactory().TypeMapper;
            this.deserializer =
                new PomonaJsonDeserializer(new ServerSerializationContextProvider(this.typeMapper, Substitute.For<IUriResolver>(),
                                                                                  Substitute.For<IResourceResolver>(), new NoContainer()));
        }


        public class Config : PomonaConfigurationBase
        {
        }
    }
}