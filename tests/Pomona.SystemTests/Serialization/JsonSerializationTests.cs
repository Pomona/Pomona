#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

