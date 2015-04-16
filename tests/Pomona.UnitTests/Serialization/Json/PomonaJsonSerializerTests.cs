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
        public void Deserialize_null_to_non_nullable_value_throws_PomonaSerializationException()
        {
            var ex = Assert.Throws<PomonaSerializationException>(
                () => this.deserializer.DeserializeString("null", options : new DeserializeOptions() { ExpectedBaseType = typeof(decimal) }));
            Assert.That(ex.Message, Is.EqualTo("Deserialized to null, which is not allowed value for casting to type System.Decimal"));
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