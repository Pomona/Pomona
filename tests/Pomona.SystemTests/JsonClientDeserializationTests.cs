#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Critters.Client;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class JsonClientDeserializationTests
    {
        private ClientTypeMapper typeMapper;


        [Test]
        public void DeserializeBoolToObject_ReturnsDeserializedValue()
        {
            var jsonString = @"true";
            var deserialized = Deserialize<object>(jsonString);
            Assert.That(deserialized, Is.True);
        }


        [Test]
        public void DeserializeBoxedBoolToObject_ReturnsDeserializedValue()
        {
            var jsonString = @"{
      ""_type"": ""Boolean"",
      ""value"": true
    }";
            var deserialized = Deserialize<object>(jsonString);
            Assert.That(deserialized, Is.True);
        }


        [Test]
        public void DeserializeClassWithObjectProperty_PropertyGotBoxedIntValue_ReturnsDeserializedObject()
        {
            var jsonString = @"{
  ""fooBar"": {
    ""_type"": ""Int32"",
    ""value"": 1337
  }
}
";

            var deserialized = Deserialize<IHasObjectProperty>(jsonString);
            Assert.That(deserialized.FooBar, Is.EqualTo(1337));
        }


        [Test]
        public void DeserializeClassWithObjectProperty_PropertyGotBoxedStringValue_ReturnsDeserializedObject()
        {
            var jsonString = @"{
  ""fooBar"": {
    ""_type"": ""String"",
    ""value"": ""blabla""
  }
}
";

            var deserialized = Deserialize<IHasObjectProperty>(jsonString);
            Assert.That(deserialized.FooBar, Is.EqualTo("blabla"));
        }


        [Test]
        public void DeserializeClassWithObjectProperty_PropertyGotStringValue_ContainingDate_IsStillParsedAsString()
        {
            var jsonString = @"{ ""fooBar"": ""2015-01-02T12:08:33"" }";

            var deserialized = Deserialize<IHasObjectProperty>(jsonString);
            Assert.That(deserialized.FooBar, Is.EqualTo("2015-01-02T12:08:33"));
        }


        [Test]
        public void
            DeserializeClassWithObjectProperty_PropertyGotStringValue_ReturnsDeserializedObject()
        {
            var jsonString = @"{ ""fooBar"": ""blabla"" }";

            var deserialized = Deserialize<IHasObjectProperty>(jsonString);
            Assert.That(deserialized.FooBar, Is.EqualTo("blabla"));
        }


        [Test]
        public void DeserializeCritter_AndCheckSomeProperties()
        {
            var jsonString =
                @"{
      '_uri': 'http://localhost:17717/critters/658',
      'createdOn': '2012-10-31T22:16:28.935146Z',
      'guid': '16f3e37f-64c4-4142-9685-e1026d1b1756',
      'id': 658,
      'name': 'Excellent Bear',
      'crazyValue': {
        'info': 'Yup, this is a value object. Look.. no _ref URI.',
        'sickness': 'Excellent Bear has bronchitis'
      },
      'enemies': {
        '_ref': 'http://localhost:17717/critters/658/enemies'
      },
      'farm': {
        '_ref': 'http://localhost:17717/farms/71'
      },
      'hat': {
        '_ref': 'http://localhost:17717/hats/657'
      },
      'simpleAttributes': {
        '_ref': 'http://localhost:17717/critters/658/simpleattributes'
      },
      'subscriptions': {
        '_ref': 'http://localhost:17717/critters/658/subscriptions'
      },
      'weapons': {
        '_ref': 'http://localhost:17717/critters/658/weapons'
      }
    }";

            var critter = Deserialize<ICritter>(jsonString);
            Assert.That(critter.Name, Is.EqualTo("Excellent Bear"));
        }


        [Test]
        public void DeserializeNullToObject_ReturnsDeserializedValue()
        {
            var jsonString = @"null";
            var deserialized = Deserialize<object>(jsonString);
            Assert.That(deserialized, Is.Null);
        }


        [Test]
        public void DeserializeStringToObjectDictionary_ReturnsDeserializedObject()
        {
            var jsonString = @"{
  ""map"": {
    ""foo"": {
      ""_type"": ""Int32"",
      ""value"": 1234
    },
    ""bar"": {
      ""_type"": ""String"",
      ""value"": ""hoho""
    }
  }
}";
            var deserialized = Deserialize<IStringToObjectDictionaryContainer>(jsonString);

            Assert.That(deserialized.Map, Is.Not.Null);
            Assert.That(deserialized.Map.Count, Is.EqualTo(2));
            Assert.IsTrue(deserialized.Map.ContainsKey("foo"));
            Assert.IsTrue(deserialized.Map.ContainsKey("bar"));
            Assert.That(deserialized.Map["foo"], Is.EqualTo(1234));
            Assert.That(deserialized.Map["bar"], Is.EqualTo("hoho"));
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new ClientTypeMapper(typeof(CritterClient).Assembly);
        }

        #endregion

        private T Deserialize<T>(string jsonString)
        {
            var pomonaClient = Substitute.For<IPomonaClient>();
            var jsonDeserializer = new PomonaJsonDeserializer(
                new ClientSerializationContextProvider(this.typeMapper,
                                                       pomonaClient,
                                                       pomonaClient));
            return jsonDeserializer.DeserializeString<T>(jsonString);
        }
    }
}
