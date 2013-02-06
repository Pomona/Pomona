using System;
using System.Linq;
using System.Reflection;
using Critters.Client;

using NUnit.Framework;

using Pomona.Common.Linq;
using Pomona.Internals;

namespace Pomona.SystemTests.Linq
{
    [TestFixture]
    public class LinqQueryTests : ClientTestsBase
    {
        public interface ICustomCritter : IDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }


        [Test]
        public void QueryCritter_AnyWithExistingName_ReturnsTrue()
        {
            // Just take some random critter
            var critter = this.CritterEntities.Skip(6).Take(1).First();
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
        public void QueryCritter_GroupByThenSelectAnonymousClassThenOrderBy_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var expected =
                this.CritterEntities
                    .Where(x => x.Id % 2 == 0)
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
                this.client.Query<ICritter>()
                    .Where(x => x.Id % 2 == 0)
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
                this.CritterEntities
                    .Where(x => x.Id % 2 == 0)
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
                this.client.Query<ICritter>()
                    .Where(x => x.Id % 2 == 0)
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
            var critter = this.CritterEntities.Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                this.client.Query<ICritter>().First(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WhereThenSelectAnonymousClass_ReturnsCorrectValues()
        {
            var expected = this.CritterEntities
                .Where(x => x.Id % 2 == 0)
                .Select(x => new { x.Name, Crazy = x.CrazyValue.Sickness })
                .OrderBy(x => x.Name)
                .Take(10)
                .ToList();
            var actual =
                this.client.Query<ICritter>()
                    .Where(x => x.Id % 2 == 0)
                    .Select(x => new { x.Name, Crazy = x.CrazyValue.Sickness })
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
            var expected = this.CritterEntities.OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList();
            var actual =
                this.client.Query<ICritter>().OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList().ToList();
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_WithAttributeEquals_ReturnsCorrectCritter()
        {
            var critter =
                this.client.Critters.Query().Where(
                    x => x.SimpleAttributes.Any(y => y.Key == "AttrKey" && y.Value == "dde")).ToList();
        }


        [Test]
        public void QueryCritter_WithExpandedPropertyOfAnonymousClass_HasPropertyExpanded()
        {
            var result =
                this.client.Critters.Query().Select(x => new { TheHat = x.Hat, x.Name }).Expand(x => x.TheHat).Take(1).
                     First();
            Assert.That(result.TheHat, Is.TypeOf<HatResource>());
        }


        [Test]
        public void QueryCritter_WithExpandedProperty_HasPropertyExpanded()
        {
            var result = this.client.Critters.Query().Expand(x => x.Hat).Take(1).First();
            Assert.That(result.Hat, Is.TypeOf<HatResource>());
        }


        [Category("TODO")]
        [Test(Description = "Fails because it's not implemented yet.")]
        public void QueryUserCustomizedCritters_ReturnsCustomizedCritters()
        {
            var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomCritter), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));
            var cust2 = client.Query<ICustomCritter>()
                              .Where(x => x.CustomString == "Lalalala")
                              .Where(x => x.OtherCustom == "Blob rob")
                              .Select(x => x.OtherCustom);

            var cust2mod = visitor.Visit(cust2.Expression);
            var cust2modString = cust2mod.ToString();


            IQueryable<ICustomCritter> customCrittersQuery =
                this.client.Query<ICustomCritter>()
                .Where(x => x.CustomString == "Lalala");
            var modifiedQUery = visitor.Visit(customCrittersQuery.Expression);
            var customCritter = customCrittersQuery.ToList();
            Assert.Fail("Test not written yet.");
        }
    }
}