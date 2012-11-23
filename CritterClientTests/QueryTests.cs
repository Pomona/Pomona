#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Linq.Expressions;

using Critters.Client;

using NUnit.Framework;

using Pomona.Client;
using Pomona.Client.Proxies;
using Pomona.Example.Models;

namespace CritterClientTests
{
    [TestFixture]
    public class QueryTests : ClientTestsBase
    {
        [Test]
        public void QueryAgainstRepositoryOnEntity_ReturnsResultsRestrictedToEntity()
        {
            var farms = this.client.Farms.Query(x => true).ToList();
            Assert.That(farms.Count, Is.GreaterThanOrEqualTo(2));
            var firstFarm = farms[0];
            var secondFarm = farms[1];

            var someCritters = firstFarm.Critters.Query(x => x.Farm.Id == firstFarm.Id).ToList();
            Assert.That(someCritters, Has.Count.GreaterThanOrEqualTo(1));
            var noCritters = firstFarm.Critters.Query(x => x.Farm.Id == secondFarm.Id);
            Assert.That(noCritters, Has.Count.EqualTo(0));
        }


        [Test]
        public void QueryClientSideInheritedResource_ReturnsCorrectResults()
        {
            DataSource.Post(
                new DictionaryContainer() { Map = new Dictionary<string, string>() { { "Lulu", "booja" } } });
            DataSource.Post(
                new DictionaryContainer() { Map = new Dictionary<string, string>() { { "WrappedAttribute", "booja" } } });
            DataSource.Post(
                new DictionaryContainer() { Map = new Dictionary<string, string>() { { "WrappedAttribute", "hooha" } } });
            DataSource.Post(
                new DictionaryContainer()
                { Map = new Dictionary<string, string>() { { "WrappedAttribute", "halala" } } });

            var critters = this.client.Query<IHasCustomAttributes>(
                this.client.BaseUri + "dictionary-containers", x => x.WrappedAttribute.StartsWith("h"));

            Assert.That(critters.Any(x => x.WrappedAttribute == "hooha"), Is.True);
            Assert.That(critters.Any(x => x.WrappedAttribute == "booja"), Is.False);
            Assert.That(critters.Count, Is.EqualTo(2));
        }


        [Test]
        public void QueryCritter_CastToMusicalCritterWithEqualsOperator_ReturnsCorrectMusicalCritter()
        {
            var firstMusicalCritter =
                CritterEntities.OfType<MusicalCritter>().First();
            var bandName = firstMusicalCritter.BandName;

            TestQuery<ICritter, Critter>(
                x => x is IMusicalCritter && ((IMusicalCritter)x).BandName == bandName,
                x => x is MusicalCritter && ((MusicalCritter)x).BandName == bandName);
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
            this.client.Critters.Post(x => { x.Name = name; });
            var results = TestQuery<ICritter, Critter>(
                x => x.Name == name, x => x.Name == name);
            Assert.That(results, Has.Count.EqualTo(1));
        }


        [Test]
        public void QueryCritter_NameEqualsStringWithNonAsciiCharacter_ReturnsCorrectCritters()
        {
            var name = "MøllÆÅØΔδ" + Guid.NewGuid();
            this.client.Critters.Post(x => { x.Name = name; });
            var results = TestQuery<ICritter, Critter>(x => x.Name == name, x => x.Name == name);
            Assert.That(results, Has.Count.EqualTo(1));
        }


        [Test]
        public void QueryCritter_ReturnsExpandedProperties()
        {
            var critter = this.client.Query<ICritter>(x => true, expand : "hat,weapons").First();
            // Check that we're not dealing with a lazy proxy
            Assert.That(critter.Hat, Is.TypeOf<HatResource>());
            Assert.That(critter.Weapons, Is.Not.TypeOf<LazyListProxy<IWeapon>>());
            Assert.That(critter.Subscriptions, Is.TypeOf<LazyListProxy<ISubscription>>());
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
            var firstCritter = this.critterHost.DataSource.List<Critter>().First();
            var createdOn = firstCritter.CreatedOn;
            var fetchedCritter = this.client.Query<ICritter>(x => x.CreatedOn == createdOn);

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

            var critters = this.client.Query<ICritter>(x => x.Id >= minId && x.Id <= maxId);

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
                this.critterHost.DataSource.List<Critter>().Skip(1).First().Name;

            var critters =
                this.client.Query<ICritter>(x => x.Name == nameOfFirstCritter || x.Name == nameOfSecondCritter);

            Assert.That(critters.Any(x => x.Name == nameOfFirstCritter));
            Assert.That(critters.Any(x => x.Name == nameOfSecondCritter));
        }


        [Test]
        public void QueryCritter_WithNameEquals_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = CritterEntities.First().Name;
            var fetchedCritters = this.client.Query<ICritter>(x => x.Name == nameOfFirstCritter);
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
            var fetchedCritters = this.client.Critters.Query(
                x => true, orderBy : x => x.Id, sortOrder : SortOrder.Descending, top : 1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Id, SortOrder.Descending);
        }


        [Test]
        public void QueryCritter_WithOrderByInt_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = this.client.Critters.Query(x => true, orderBy : x => x.Id, top : 1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Id, SortOrder.Ascending);
        }


        [Test]
        public void QueryCritter_WithOrderByStringDesc_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = this.client.Critters.Query(
                x => true, orderBy : x => x.Name, sortOrder : SortOrder.Descending, top : 1000);
            AssertIsOrderedBy(fetchedCritters, x => x.Name, SortOrder.Descending);
        }


        [Test]
        public void QueryCritter_WithOrderByString_ReturnsCrittersInCorrectOrder()
        {
            var fetchedCritters = this.client.Critters.Query(x => true, orderBy : x => x.Name, top : 1000);
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


        [Test]
        public void QueryDictionaryContainer_WithDictonaryItemEquals_ReturnsCorrectStuff()
        {
            var matching = (DictionaryContainer)DataSource.Post(
                new DictionaryContainer()
                {
                    Map = new Dictionary<string, string>() { { "fubu", "bar" } }
                });
            var notMatching = (DictionaryContainer)DataSource.Post(
                new DictionaryContainer()
                {
                    Map = new Dictionary<string, string>() { { "fubu", "nope" } }
                });

            var resultIds = TestQuery<IDictionaryContainer, DictionaryContainer>(
                x => x.Map["fubu"] == "bar", x => x.Map["fubu"] == "bar").Select(x => x.Id);

            Assert.That(resultIds, Has.Member(matching.Id));
            Assert.That(resultIds, Has.No.Member(notMatching.Id));
        }


        [Test]
        public void QueryMusicalCritter_WithBandNameEquals_ReturnsCorrectResult()
        {
            var musicalCritter = CritterEntities.OfType<MusicalCritter>().Skip(1).First();
            var bandName = musicalCritter.BandName;
            var critters =
                this.client.Query<IMusicalCritter>(x => x.BandName == bandName && x.Name == musicalCritter.Name);
            Assert.That(critters.Any(x => x.Id == musicalCritter.Id));
        }


        [Test]
        public void QueryMusicalCritter_WithPropertyOnlyOnMusicalCritterExpanded_ReturnsExpandedProperty()
        {
            var musicalCritter = this.client.Query<IMusicalCritter>(x => true, expand : "instrument").First();
            // Check that we're not dealing with a lazy proxy
            Assert.That(musicalCritter.Instrument, Is.TypeOf<InstrumentResource>());
        }


        [Test]
        public void QueryWeapon_RoundDecimal_ReturnsCorrectCritters()
        {
            TestQuery<IWeapon, Weapon>(x => decimal.Round(x.Price) == 5m, x => decimal.Round(x.Price) == 5m);
        }


        [Test]
        public void QueryWeapon_RoundDouble_ReturnsCorrectCritters()
        {
            TestQuery<IWeapon, Weapon>(x => Math.Round(x.Strength) == 1.0, x => Math.Round(x.Strength) == 1.0);
        }


        [Test]
        public void QueryWeapons_WithFilterOnDecimal_ReturnsCorrectCritters()
        {
            TestQuery<IWeapon, Weapon>(x => x.Price < 500m, x => x.Price < 500m);
        }


        [Test]
        public void QueryWeapons_WithFilterOnDouble_ReturnsCorrectCritters()
        {
            TestQuery<IWeapon, Weapon>(x => x.Strength > 0.8, x => x.Strength > 0.8);
        }
    }
}