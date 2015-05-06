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

using Critters.Client;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;

namespace Pomona.SystemTests.Serialization
{
    [TestFixture]
    public class JsonSerializationTests
    {
        private readonly ClientTypeMapper clientTypeMapper = new ClientTypeMapper(new Type[] { typeof(IOrderItem) });
        private PomonaJsonDeserializer deserializer;


        [Test]
        public void DateTimeWithoutUTCMarkAtEndDeserializesCorrectly()
        {
            var obj =
                this.deserializer.DeserializeString<IStringToObjectDictionaryContainer>(
                    "{ map : { blah : { _type: 'DateTime', value: '1995-06-08T22:00:00' } } }");
            Assert.That((DateTime)obj.Map.SafeGet("blah"),
                        Is.EqualTo(new DateTime(1995, 06, 08, 22, 00, 00, DateTimeKind.Local)));
        }


        [Test]
        public void DateTimeWithUTCMarkAtEndDeserializesCorrectly()
        {
            var obj =
                this.deserializer.DeserializeString<IStringToObjectDictionaryContainer>(
                    "{ map : { blah : { _type: 'DateTime', value: '1995-06-08T22:00:00Z' } } }");
            Assert.That((DateTime)obj.Map.SafeGet("blah"),
                        Is.EqualTo(new DateTime(1995, 06, 08, 22, 00, 00, DateTimeKind.Utc)));
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var factory = new PomonaJsonSerializerFactory();
            var pomonaClient = Substitute.For<IPomonaClient>();
            this.deserializer =
                factory.GetDeserializer(new ClientSerializationContextProvider(this.clientTypeMapper, pomonaClient, pomonaClient));
        }

        #endregion

        [Test]
        public void UnknownPropertyIsIgnoredByDeserializer()
        {
            this.deserializer.DeserializeString<IOrderItem>("{name:\"blah\",ignored:\"optional\"}");
        }


        public class TestClass : IClientResource
        {
            public string FooBar { get; set; }
        }
    }
}