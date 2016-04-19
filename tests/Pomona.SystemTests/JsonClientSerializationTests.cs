#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;

using Critters.Client;

using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization.Json;
using Pomona.TestHelpers;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class JsonClientSerializationTests
    {
        private ClientTypeMapper typeMapper;


        [Test]
        public void SerializeClassWithObjectProperty_PropertyGotBoolValue_ReturnsCorrectJson()
        {
            var obj = new HasObjectPropertyForm();
            obj.FooBar = true;
            var jobject = SerializeAndGetJsonObject(obj);

            Assert.That(jobject.AssertHasPropertyWithBool("fooBar"), Is.True);
        }


        [Test]
        public void SerializeClassWithObjectProperty_PropertyGotIntValue_ReturnsCorrectJson()
        {
            var obj = new HasObjectPropertyForm();
            obj.FooBar = 1337;
            var jobject = SerializeAndGetJsonObject(obj);

            var fooBarBox = jobject.AssertHasPropertyWithObject("fooBar");
            Assert.That(fooBarBox.AssertHasPropertyWithString("_type"), Is.EqualTo("Int32"));
            Assert.That(fooBarBox.AssertHasPropertyWithInteger("value"), Is.EqualTo(1337));
        }


        [Test]
        public void SerializeClassWithObjectProperty_PropertyGotNull_ReturnsCorrectJson()
        {
            var obj = new HasObjectPropertyForm();
            obj.FooBar = null;
            var jobject = SerializeAndGetJsonObject(obj);

            jobject.AssertHasPropertyWithNull("fooBar");
        }


        [Test]
        public void SerializeCritterForm_WithReferences()
        {
            var critterForm = new CritterForm
            {
                Name = "Sheep",
                CrazyValue = new CrazyValueObjectForm { Info = "blblbobobo", Sickness = "whawhahaha" }
            };

            critterForm.Weapons.Add(new GunForm());

            var jobject = SerializeAndGetJsonObject(critterForm);
            Assert.That(jobject.AssertHasPropertyWithString("name"), Is.EqualTo("Sheep"));
            jobject.AssertHasPropertyWithObject("crazyValue");
        }


        [Test]
        public void SerializeStringToObjectDictionary_ReturnsCorrectJson()
        {
            var dictContainer = new StringToObjectDictionaryContainerForm();
            dictContainer.Map["foo"] = 1234;
            dictContainer.Map["bar"] = "hoho";

            var jobj = SerializeAndGetJsonObject(dictContainer);

            var mapJobj = jobj.AssertHasPropertyWithObject("map");
            var fooBox = mapJobj.AssertHasPropertyWithObject("foo");
            Assert.That(mapJobj.AssertHasPropertyWithString("bar"), Is.EqualTo("hoho"));

            fooBox.AssertHasPropertyWithValue("_type", "Int32");
            fooBox.AssertHasPropertyWithValue("value", 1234);
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new ClientTypeMapper(typeof(CritterClient).Assembly);
        }

        #endregion

        private JObject SerializeAndGetJsonObject<T>(T value)
        {
            var serializerFactory =
                new PomonaJsonSerializerFactory();
            var pomonaClient = Substitute.For<IPomonaClient>();
            var serializer = serializerFactory.GetSerializer(
                new ClientSerializationContextProvider(this.typeMapper, pomonaClient, pomonaClient));
            Console.WriteLine("Serialized object to json:");
            string jsonString;
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, value, null);
                jsonString = stringWriter.ToString();
            }
            Console.WriteLine(jsonString);

            return (JObject)JToken.Parse(jsonString);
        }
    }
}