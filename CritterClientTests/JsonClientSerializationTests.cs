using System;
using System.IO;
using Critters.Client;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.TestHelpers;

namespace CritterClientTests
{
    [TestFixture]
    public class JsonClientSerializationTests
    {
        [SetUp]
        public void SetUp()
        {
            typeMapper = new ClientTypeMapper(Client.ResourceTypes);
        }

        private ClientTypeMapper typeMapper;


        private JObject SerializeAndGetJsonObject<T>(T value)
        {
            var serializerFactory = new PomonaJsonSerializerFactory(new IMappedType[] {});
            var serializer = serializerFactory.GetSerialier();
            var stringWriter = new StringWriter();
            var jsonWriter = serializer.CreateWriter(stringWriter);
            serializer.SerializeNode(
                new ItemValueSerializerNode(value, GetClassMapping(value.GetType()), "", GetFetchContext()), jsonWriter);

            Console.WriteLine("Serialized object to json:");
            var jsonString = stringWriter.ToString();
            Console.WriteLine(jsonString);

            return (JObject) JToken.Parse(jsonString);
        }


        private ISerializationContext GetFetchContext()
        {
            return new ClientSerializationContext(typeMapper);
        }


        private IMappedType GetClassMapping(Type type)
        {
            return typeMapper.GetClassMapping(type);
        }

        [Test]
        public void SerializeCritterForm_WithReferences()
        {
            var critterForm = new CritterForm()
                {
                    Name = "Sheep",
                    CrazyValue = new CrazyValueObjectForm() {Info = "blblbobobo", Sickness = "whawhahaha"}
                };

            critterForm.Weapons.Add(new GunForm());

            var jobject = SerializeAndGetJsonObject(critterForm);
            Assert.That(jobject.AssertHasPropertyWithString("name"), Is.EqualTo("Sheep"));
            jobject.AssertHasPropertyWithObject("crazyValue");
        }
    }
}