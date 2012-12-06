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
                client.Critters.Query().Any(x => x.Name == critter.Name);
            Assert.That(hasCritterWithGuid, Is.True);
        }


        [Test]
        public void QueryCritter_AnyWithNameEqualToRandomGuid_ReturnsFalse()
        {
            var hasCritterWithGuid =
                client.Query<ICritter>().Any(x => x.Name == Guid.NewGuid().ToString());
            Assert.That(hasCritterWithGuid, Is.False);
        }


        [Test]
        public void QueryCritter_GroupByThenSelectAnonymousClassThenOrderBy_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var expected =
                CritterEntities
                    .Where(x => x.Id%2 == 0)
                    .GroupBy(x => x.Name.Substring(0, 1))
                    .Select(
                        x => new
                            {
                                x.Key,
                                Count = x.Count(),
                                WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                            })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

            var actual =
                client.Query<ICritter>()
                      .Where(x => x.Id%2 == 0)
                      .GroupBy(x => x.Name.Substring(0, 1))
                      .Select(
                          x => new
                              {
                                  x.Key,
                                  Count = x.Count(),
                                  WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                              })
                      .OrderByDescending(x => x.Count)
                      .Take(10)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }

        [Test]
        public void QueryCritter_GroupByThenSelectAnonymousClass_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var expected =
                CritterEntities
                    .Where(x => x.Id%2 == 0)
                    .GroupBy(x => x.Farm.Id)
                    .Select(
                        x => new
                            {
                                Count = x.Count(),
                                WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                            })
                    .Take(1)
                    .ToList();

            var actual =
                client.Query<ICritter>()
                      .Where(x => x.Id%2 == 0)
                      .GroupBy(x => x.Farm.Id)
                      .Select(
                          x => new
                              {
                                  Count = x.Count(),
                                  WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                              })
                      .Take(1)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_WhereFirst_ReturnsCorrectCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                client.Query<ICritter>().First(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WhereThenSelectAnonymousClass_ReturnsCorrectValues()
        {
            var expected = CritterEntities
                .Where(x => x.Id%2 == 0)
                .Select(x => new {x.Name, Crazy = x.CrazyValue.Sickness})
                .OrderBy(x => x.Name)
                .Take(10)
                .ToList();
            var actual =
                client.Query<ICritter>()
                      .Where(x => x.Id%2 == 0)
                      .Select(x => new {x.Name, Crazy = x.CrazyValue.Sickness})
                      .OrderBy(x => x.Name)
                      .Take(10)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_WhereThenSelectSingleProperty_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var expected = CritterEntities.OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList();
            var actual =
                client.Query<ICritter>().OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList().ToList();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_WithExpandedPropertyOfAnonymousClass_HasPropertyExpanded()
        {
            var result =
                client.Critters.Query().Select(x => new {TheHat = x.Hat, x.Name}).Expand(x => x.TheHat).Take(1).First();
            Assert.That(result.TheHat, Is.TypeOf<HatResource>());
        }

        [Test]
        public void QueryCritter_WithExpandedProperty_HasPropertyExpanded()
        {
            var result = client.Critters.Query().Expand(x => x.Hat).Take(1).First();
            Assert.That(result.Hat, Is.TypeOf<HatResource>());
        }
    }
}