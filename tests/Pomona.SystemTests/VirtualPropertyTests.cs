#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class VirtualPropertyTests : ClientTestsBase
    {
        [Test]
        public void Get_type_having_virtual_properties_is_successful()
        {
            var entity = Save(new VirtualPropertyThing() { Number = 25.0 });
            var resource = Client.VirtualPropertyThings.Get(entity.Id);

            Assert.That(resource, Is.Not.Null);
            Assert.That(resource.Rocky, Is.EqualTo("BALOBA"));
            Assert.That(resource.NumberSquareRoot, Is.EqualTo(5.0));
        }


        [Test]
        public void Post_of_type_having_virtual_properties_is_successful()
        {
            var resource = Client.VirtualPropertyThings.Post(new VirtualPropertyThingForm()
            {
                NumberSquareRoot = 13,
                Rocky = "IS DA MAN"
            });

            Assert.That(resource, Is.Not.Null);
            Assert.That(resource.Number, Is.EqualTo(13.0 * 13.0));
            Assert.That(resource.NumberSquareRoot, Is.EqualTo(13.0));
            Assert.That(resource.Rocky, Is.EqualTo("IS DA MAN"));
        }


        [Test]
        public void Query_using_virtual_property_in_predicate_is_successful()
        {
            Save(new VirtualPropertyThing()); // Just another thing that will not match predicate.
            var virtualPropThing = Save(new VirtualPropertyThing() { Number = 9.0 });
            var id =
                Client.VirtualPropertyThings.Query().Where(x => x.NumberSquareRoot == 3.0).Select(x => x.Id).Single();
            Assert.That(id, Is.EqualTo(virtualPropThing.Id));
        }
    }
}