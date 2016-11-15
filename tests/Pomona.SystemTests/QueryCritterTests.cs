#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Common.Proxies;
using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class QueryCritterTests : ClientTestsBase
    {
        [Test]
        public void GetCritter_HavingEnemies_ReturnsList_WithItemsSerializedAsReferences()
        {
            Repository.CreateRandomData(critterCount : 4, alwaysHasEnemies : true);
            var critterId = CritterEntities.OrderByDescending(x => x.Enemies.Count).Select(x => x.Id).First();
            var critterResource = Client.Critters.Get(critterId);

            Assert.That(critterResource.Enemies.IsLoaded(), Is.True);
            foreach (var enemy in critterResource.Enemies)
                Assert.That(enemy.IsLoaded(), Is.False);
        }


        [Test]
        public void QueryCritter_CastToMusicalCritterWithEqualsOperator_ReturnsCorrectMusicalCritter()
        {
            var firstMusicalCritter =
                CritterEntities.OfType<MusicalCritter>().First();
            var bandName = firstMusicalCritter.BandName;

            TestQuery<ICritter, Critter>(x => x is IMusicalCritter && ((IMusicalCritter)x).BandName == bandName,
                                         x => x is MusicalCritter && ((MusicalCritter)x).BandName == bandName);
        }


        [Test]
        public void QueryCritter_IdInArray_ReturnsCorrectCritters()
        {
            Repository.CreateRandomData(critterCount : 10);
            var ids = CritterEntities.Skip(4).Select(x => x.Id).Take(5).ToArray();
            TestQuery<ICritter, Critter>(x => ids.Contains(x.Id), x => ids.Contains(x.Id), expectedResultCount : 5);
        }


        [Test]
        public void QueryCritter_IdInList_ReturnsCorrectCritters()
        {
            Repository.CreateRandomData(critterCount : 10);
            var ids = CritterEntities.Skip(4).Select(x => x.Id).Take(5).ToList();
            TestQuery<ICritter, Critter>(x => ids.Contains(x.Id), x => ids.Contains(x.Id), expectedResultCount : 5);
        }


        [Test]
        public void QueryCritter_IsOfMusicalCritter_ReturnsAllMusicalCritters()
        {
            TestQuery<ICritter, Critter>(x => x is IMusicalCritter, x => x is MusicalCritter);
        }


        [Test]
        public void QueryCritter_NameContainsString_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Name.Contains("Canary"), x => x.Name.Contains("Canary"));
        }


        [Test]
        public void QueryCritter_NameEndsWith_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Name.EndsWith("e"), x => x.Name.EndsWith("e"));
        }


        [Test]
        public void QueryCritter_NameEqualsStringWithEncodedSingleQuote_ReturnsCorrectCritters()
        {
            var name = "bah'bah''" + Guid.NewGuid();
            Client.Critters.Post(x =>
            {
                x.Name = name;
            });
            var results = TestQuery<ICritter, Critter>(x => x.Name == name,
                                                       x => x.Name == name);
            Assert.That(results, Has.Count.EqualTo(1));
        }


        [Test]
        public void QueryCritter_NameEqualsStringWithNonAsciiCharacter_ReturnsCorrectCritters()
        {
            var name = "MøllÆÅØΔδ" + Guid.NewGuid();
            Client.Critters.Post(x =>
            {
                x.Name = name;
            });
            var results = TestQuery<ICritter, Critter>(x => x.Name == name, x => x.Name == name);
            Assert.That(results, Has.Count.EqualTo(1));
        }


        [Test]
        public void QueryCritter_NameInList_ReturnsCorrectCritters()
        {
            var names = CritterEntities.Skip(4).Select(x => x.Name).Take(5).ToArray();
            TestQuery<ICritter, Critter>(x => names.Contains(x.Name), x => names.Contains(x.Name));
        }


        [Test]
        public void QueryCritter_OrderByThenBy_ReturnsCrittersInCorrectOrder()
        {
            Repository.CreateRandomData(40);
            var fetchedCritters = Client.Critters
                                        .Query()
                                        .Expand(x => x.Weapons)
                                        .OrderByDescending(x => x.Weapons.Count)
                                        .ThenBy(x => x.Id)
                                        .Take(1000)
                                        .ToList();

            var expected = fetchedCritters.OrderByDescending(x => x.Weapons.Count).ThenBy(x => x.Id).ToList();
            Assert.That(fetchedCritters.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_OrderByThenByDescending_ReturnsCrittersInCorrectOrder()
        {
            Repository.CreateRandomData(40);
            var fetchedCritters = Client.Critters
                                        .Query()
                                        .Expand(x => x.Weapons)
                                        .OrderBy(x => x.Weapons.Count)
                                        .ThenByDescending(x => x.Id)
                                        .Take(1000)
                                        .ToList();

            var expected = fetchedCritters.OrderBy(x => x.Weapons.Count).ThenByDescending(x => x.Id).ToList();
            Assert.That(fetchedCritters.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_ReturnsExpandedProperties()
        {
            var critter = Client.Query<ICritter>().Expand(x => x.Hat).Expand(x => x.Weapons).First();
            // Check that we're not dealing with a lazy proxy
            Assert.That(critter.Hat, Is.TypeOf<HatResource>());
            Assert.That(critter.Weapons, Is.Not.TypeOf<LazyListProxy<IWeapon>>());

            // Subscriptions is configured to always be expanded
            Assert.That(critter.Subscriptions, Is.TypeOf<List<ISubscription>>());
            Assert.That(critter.Subscriptions.Count, Is.GreaterThan(0));
            Assert.That(critter.Subscriptions.All(x => x is SubscriptionResource));
        }


        [Test]
        public void QueryCritter_SearchByAttribute()
        {
            TestQuery<ICritter, Critter>(
                x => x.SimpleAttributes.Any(y => y.Key == "Moo" && y.Value == "Boo"),
                x => x.SimpleAttributes.Any(y => y.Key == "Moo" && y.Value == "Boo"));
        }


        [Test]
        public void QueryCritter_Sqrt_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => Math.Sqrt(9.0) == 3.0, x => Math.Sqrt(9.0) == 3.0);
        }


        [Test]
        public void QueryCritter_TolowerNameContainsString_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Name.ToLower().Contains("bear"), x => x.Name.ToLower().Contains("bear"));
        }


        [Test]
        public void QueryCritter_WithCreatedDayMod3Equals0_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.CreatedOn.Day % 3 == 0, x => x.CreatedOn.Day % 3 == 0);
        }


        [Test]
        public void QueryCritter_WithDateTimeBetween_ReturnsCorrectResult()
        {
            var from = DateTime.UtcNow.AddDays(-5);
            var to = DateTime.UtcNow.AddDays(-2);
            TestQuery<ICritter, Critter>(x => x.CreatedOn > from && x.CreatedOn <= to,
                                         x => x.CreatedOn > from && x.CreatedOn <= to);
        }


        [Test]
        public void QueryCritter_WithDateTimeEquals_ReturnsCorrectResult()
        {
            var firstCritter = Repository.List<Critter>().First();
            var createdOn = firstCritter.CreatedOn;
            TestQuery<ICritter, Critter>(x => x.CreatedOn == createdOn,
                                         x => x.CreatedOn == createdOn);
        }


        [Test]
        public void QueryCritter_WithDateTimeOffsetBetween_ReturnsCorrectResult()
        {
            var from = DateTimeOffset.Now.AddDays(-5);
            var to = DateTimeOffset.Now.AddDays(-2);
            TestQuery<ICritter, Critter>(x => x.CreatedOnOffset > from && x.CreatedOnOffset <= to,
                                         x => x.CreatedOnOffset > from && x.CreatedOnOffset <= to);
        }


        [Test]
        public void QueryCritter_WithDateTimeOffsetEquals_ReturnsCorrectResult()
        {
            var firstCritter = Repository.List<Critter>().First();
            var createdOnOffset = firstCritter.CreatedOnOffset;
            TestQuery<ICritter, Critter>(x => x.CreatedOnOffset == createdOnOffset,
                                         x => x.CreatedOnOffset == createdOnOffset);
        }


        [Test]
        public void QueryCritter_WithIdBetween_ReturnsCorrectResult()
        {
            var orderedCritters = CritterEntities.OrderBy(x => x.Id).Skip(2).Take(5).
                                                  ToList();
            var maxId = orderedCritters.Max(x => x.Id);
            var minId = orderedCritters.Min(x => x.Id);

            var critters = Client.Query<ICritter>(x => x.Id >= minId && x.Id <= maxId);

            Assert.That(critters.OrderBy(x => x.Id).Select(x => x.Id),
                        Is.EquivalentTo(orderedCritters.Select(x => x.Id)));
        }


        [Test]
        public void QueryCritter_WithLengthEquals_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Name.Length == 11, x => x.Name.Length == 11);
        }


        [Test]
        public void QueryCritter_WithNameEquals_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = CritterEntities.First().Name;
            var fetchedCritters = Client.Query<ICritter>(x => x.Name == nameOfFirstCritter);
            Assert.That(fetchedCritters.Any(x => x.Name == nameOfFirstCritter));
        }


        [Test]
        public void QueryCritter_WithNameEqualsOrNameEqualsSomethingElse_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = CritterEntities.First().Name;
            var nameOfSecondCritter =
                Repository.List<Critter>().Skip(1).First().Name;

            var critters =
                Client.Query<ICritter>(x => x.Name == nameOfFirstCritter || x.Name == nameOfSecondCritter);

            Assert.That(critters.Any(x => x.Name == nameOfFirstCritter));
            Assert.That(critters.Any(x => x.Name == nameOfSecondCritter));
        }


        [Test]
        public void QueryCritter_WithNameStartsWithA_ReturnsCrittersWithNameStartingWithA()
        {
            TestQuery<ICritter, Critter>(x => x.Name.StartsWith("A"), x => x.Name.StartsWith("A"));
        }


        [Test]
        public void QueryCritter_WithOrderByInt_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = Client.Critters.Query().OrderBy(x => x.Id).Take(1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Id, SortOrder.Ascending);
        }


        [Test]
        public void QueryCritter_WithOrderByIntDesc_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = Client.Critters.Query().OrderByDescending(x => x.Id).Take(1000).ToList();
            AssertIsOrderedBy(fetchedCritters, x => x.Id, SortOrder.Descending);
        }


        [Test]
        public void QueryCritter_WithOrderByString_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = Client.Critters.Query().OrderBy(x => x.Name).Take(1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Name, SortOrder.Ascending);
        }


        [Test]
        public void QueryCritter_WithOrderByStringDesc_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = Client.Critters.Query().OrderByDescending(x => x.Name).Take(1000).ToList();
            AssertIsOrderedBy(fetchedCritters, x => x.Name, SortOrder.Descending);
        }


        [Test]
        public void QueryCritter_WithReferenceToAnotherCritterEqualToOtherCritter_ReturnsCorrectResult()
        {
            var anotherSavedCritter = Repository.Save(new Critter());
            Repository.Save(new Critter { ReferenceToAnotherCritter = anotherSavedCritter });
            var anotherSavedCritterId = anotherSavedCritter.Id;
            var anotherCritter = Client.Query<ICritter>(x => x.Id == anotherSavedCritterId).First();
            var critter = Client.Query<ICritter>(x => // TODO: Figure out a way to avoid this null check. @asbjornu
                                                     x.ReferenceToAnotherCritter != null
                                                     && x.ReferenceToAnotherCritter == anotherCritter).First();

            Assert.That(critter, Is.Not.Null);
            Assert.That(critter.ReferenceToAnotherCritter, Is.Not.Null);
            Assert.That(critter.ReferenceToAnotherCritter.Id, Is.EqualTo(anotherSavedCritterId));
        }


        [Test]
        public void QueryCritter_WithSelectLambda_ReturnsCorrectResults()
        {
            TestQuery<ICritter, Critter>(
                x => x.Weapons.Select(y => y.Strength).Sum() > 5.0,
                x => x.Weapons.Select(y => y.Strength).Sum() > 5.0);
        }


        [Test]
        public void QueryCritter_WithSelectWhereLambda_ReturnsCorrectResults()
        {
            TestQuery<ICritter, Critter>(
                x => x.Weapons.Select(y => y.Strength).Where(z => z > 0.6).Sum() > 5.0,
                x => x.Weapons.Select(y => y.Strength).Where(z => z > 0.6).Sum() > 5.0);
        }


        [Test]
        public void QueryCritter_WithWeaponsCountIsGreaterThan7_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Weapons.Count > 7, x => x.Weapons.Count > 7);
        }
    }
}