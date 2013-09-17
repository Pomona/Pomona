#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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
        public void QueryCritter_CastToMusicalCritterWithEqualsOperator_ReturnsCorrectMusicalCritter()
        {
            var firstMusicalCritter =
                CritterEntities.OfType<MusicalCritter>().First();
            var bandName = firstMusicalCritter.BandName;

            TestQuery<ICritter, Critter>(
                x => x is IMusicalCritter && ((IMusicalCritter) x).BandName == bandName,
                x => x is MusicalCritter && ((MusicalCritter) x).BandName == bandName);
        }

        [Test]
        public void QueryCritter_IdInArray_ReturnsCorrectCritters()
        {
            this.Repository.CreateRandomData(critterCount: 10);
            var ids = CritterEntities.Skip(4).Select(x => x.Id).Take(5).ToArray();
            TestQuery<ICritter, Critter>(x => ids.Contains(x.Id), x => ids.Contains(x.Id), expectedResultCount: 5);
        }

        [Test]
        public void QueryCritter_IdInList_ReturnsCorrectCritters()
        {
            this.Repository.CreateRandomData(critterCount: 10);
            var ids = CritterEntities.Skip(4).Select(x => x.Id).Take(5).ToList();
            TestQuery<ICritter, Critter>(x => ids.Contains(x.Id), x => ids.Contains(x.Id), expectedResultCount: 5);
        }


        [Test]
        public void QueryCritter_IsOfMusicalCritter_ReturnsAllMusicalCritters()
        {
            TestQuery<ICritter, Critter>(x => x is IMusicalCritter, x => x is MusicalCritter);
        }


        [Test]
        public void QueryCritter_NameContainsString_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Name.Contains("Bear"), x => x.Name.Contains("Bear"));
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
            client.Critters.Post(x => { x.Name = name; });
            var results = TestQuery<ICritter, Critter>(
                x => x.Name == name, x => x.Name == name);
            Assert.That(results, Has.Count.EqualTo(1));
        }


        [Test]
        public void QueryCritter_NameEqualsStringWithNonAsciiCharacter_ReturnsCorrectCritters()
        {
            var name = "MøllÆÅØΔδ" + Guid.NewGuid();
            client.Critters.Post(x => { x.Name = name; });
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
        public void QueryCritter_ReturnsExpandedProperties()
        {
            var critter = client.Query<ICritter>().Expand(x => x.Hat).Expand(x => x.Weapons).First();
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
            TestQuery<ICritter, Critter>(x => x.CreatedOn.Day%3 == 0, x => x.CreatedOn.Day%3 == 0);
        }


        [Test]
        public void QueryCritter_WithDateBetween_ReturnsCorrectResult()
        {
            var fromTime = DateTime.UtcNow.AddDays(-5);
            var toTime = DateTime.UtcNow.AddDays(-2);
            TestQuery<ICritter, Critter>(
                x => x.CreatedOn > fromTime && x.CreatedOn <= toTime,
                x => x.CreatedOn > fromTime && x.CreatedOn <= toTime);
        }


        [Test]
        public void QueryCritter_WithDateEquals_ReturnsCorrectResult()
        {
            var firstCritter = this.Repository.List<Critter>().First();
            var createdOn = firstCritter.CreatedOn;
            var fetchedCritter = client.Query<ICritter>(x => x.CreatedOn == createdOn).ToList();

            Assert.That(fetchedCritter, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(fetchedCritter.First().Id, Is.EqualTo(firstCritter.Id));
        }


        [Test]
        public void QueryCritter_WithIdBetween_ReturnsCorrectResult()
        {
            var orderedCritters = CritterEntities.OrderBy(x => x.Id).Skip(2).Take(5).
                                                  ToList();
            var maxId = orderedCritters.Max(x => x.Id);
            var minId = orderedCritters.Min(x => x.Id);

            var critters = client.Query<ICritter>(x => x.Id >= minId && x.Id <= maxId);

            Assert.That(
                critters.OrderBy(x => x.Id).Select(x => x.Id), Is.EquivalentTo(orderedCritters.Select(x => x.Id)));
        }


        [Test]
        public void QueryCritter_WithLengthEquals_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Name.Length == 11, x => x.Name.Length == 11);
        }


        [Test]
        public void QueryCritter_WithNameEqualsOrNameEqualsSomethingElse_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = CritterEntities.First().Name;
            var nameOfSecondCritter =
                this.Repository.List<Critter>().Skip(1).First().Name;

            var critters =
                client.Query<ICritter>(x => x.Name == nameOfFirstCritter || x.Name == nameOfSecondCritter);

            Assert.That(critters.Any(x => x.Name == nameOfFirstCritter));
            Assert.That(critters.Any(x => x.Name == nameOfSecondCritter));
        }


        [Test]
        public void QueryCritter_WithNameEquals_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = CritterEntities.First().Name;
            var fetchedCritters = client.Query<ICritter>(x => x.Name == nameOfFirstCritter);
            Assert.That(fetchedCritters.Any(x => x.Name == nameOfFirstCritter));
        }


        [Test]
        public void QueryCritter_WithNameStartsWithA_ReturnsCrittersWithNameStartingWithA()
        {
            TestQuery<ICritter, Critter>(x => x.Name.StartsWith("A"), x => x.Name.StartsWith("A"));
        }


        [Test]
        public void QueryCritter_WithOrderByIntDesc_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = client.Critters.Query().OrderByDescending(x => x.Id).Take(1000).ToList();
            AssertIsOrderedBy(fetchedCritters, x => x.Id, SortOrder.Descending);
        }


        [Test]
        public void QueryCritter_WithOrderByInt_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = client.Critters.Query().OrderBy(x => x.Id).Take(1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Id, SortOrder.Ascending);
        }


        [Test]
        public void QueryCritter_WithOrderByStringDesc_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = client.Critters.Query().OrderByDescending(x => x.Name).Take(1000).ToList();
            AssertIsOrderedBy(fetchedCritters, x => x.Name, SortOrder.Descending);
        }


        [Test]
        public void QueryCritter_WithOrderByString_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = client.Critters.Query().OrderBy(x => x.Name).Take(1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Name, SortOrder.Ascending);
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