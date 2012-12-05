using System;
using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common.Linq;

namespace CritterClientTests.Linq
{
    [TestFixture]
    public class LinqQueryTests : ClientTestsBase
    {
        [Test]
        public void QueryCritter_AnyWithExistingName_ReturnsTrue()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(6).Take(1).First();
            var hasCritterWithGuid =
                this.client.Critters.Query().Any(x => x.Name == critter.Name);
            Assert.That(hasCritterWithGuid, Is.True);
        }


        [Test]
        public void QueryCritter_AnyWithNameEqualToRandomGuid_ReturnsFalse()
        {
            var hasCritterWithGuid =
                this.client.Query<ICritter>().Any(x => x.Name == Guid.NewGuid().ToString());
            Assert.That(hasCritterWithGuid, Is.False);
        }


        [Test]
        public void QueryCritter_WhereFirst_ReturnsCorrectCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                this.client.Query<ICritter>().First(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WhereThenSelectSingleProperty_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var critterNames =
                this.client.Query<ICritter>().Where(x => x.Id % 2 == 0).Select(x => x.Name).ToList();
            Assert.Fail("Assert not written for test");
        }
    }
}