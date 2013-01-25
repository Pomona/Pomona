using System.IO;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class JsonClientDeserializationTests
    {
        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new ClientTypeMapper(Client.ResourceTypes);
        }

        private ClientTypeMapper typeMapper;

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

            var jsonDeserializer = new PomonaJsonDeserializer();
            var reader = jsonDeserializer.CreateReader(new StringReader(jsonString));
            var context = new ClientDeserializationContext(this.typeMapper);
            var node = new ItemValueDeserializerNode(this.typeMapper.GetClassMapping(typeof (ICritter)), context);
            jsonDeserializer.DeserializeNode(node, reader);
            var critter = (ICritter) node.Value;
            Assert.That(critter.Name, Is.EqualTo("Excellent Bear"));
        }
    }
}