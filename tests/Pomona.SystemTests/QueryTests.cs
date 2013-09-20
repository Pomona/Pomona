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
using CustomEnum = Pomona.Example.Models.CustomEnum;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class QueryTests : ClientTestsBase
    {
        [Test]
        public void GetLazyById_ReturnsLazyProxy()
        {
            var critterEntity = CritterEntities.Last(x => !(x is MusicalCritter));
            var critter = client.Critters.GetLazy(critterEntity.Id);
            Assert.That(critter, Is.TypeOf<CritterLazyProxy>());
            var proxyBase = (LazyProxyBase)critter;
            Assert.That(proxyBase.ProxyTarget, Is.EqualTo(null));
            Assert.That(critter.Name, Is.EqualTo(critterEntity.Name));
            Assert.That(proxyBase.ProxyTarget, Is.TypeOf<CritterResource>());
        }

        [Test]
        public void GetResourceById_UsingClientRepository_ReturnsResource()
        {
            var critterEntity = CritterEntities.First();
            var critterResource = client.Critters.Get(critterEntity.Id);
            Assert.That(critterResource, Is.Not.Null);
        }


        [Test]
        public void QueryAgainstEntityWithRepositoryProperty_WithPredicateOnRepositoryProperty()
        {
            var firstCritterName = CritterEntities.First().Name;
            var farm = client.Farms.Where(x => x.Critters.Any(y => y.Name == firstCritterName)).ToList();
        }

        [Test]
        public void QueryAgainstRepositoryOnEntity_ReturnsResultsRestrictedToEntity()
        {
            var farms = client.Farms.Query().ToList();
            Assert.That(farms.Count, Is.GreaterThanOrEqualTo(2));
            var firstFarm = farms[0];
            var secondFarm = farms[1];

            var someCritters = firstFarm.Critters.Query(x => x.Farm.Id == firstFarm.Id).ToList();
            Assert.That(someCritters, Has.Count.GreaterThanOrEqualTo(1));
            var noCritters = firstFarm.Critters.Query(x => x.Farm.Id == secondFarm.Id).ToList();
            Assert.That(noCritters, Has.Count.EqualTo(0));
        }


        [Test]
        public void QueryClientSideInheritedResource_ReturnsCorrectResults()
        {
            Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "booja" } } });
            Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "WrappedAttribute", "booja" } } });
            Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "WrappedAttribute", "hooha" } } });
            Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "WrappedAttribute", "halala" } } });

            var critters =
                client.Query<IHasCustomAttributes>(x => x.WrappedAttribute != null && x.WrappedAttribute.StartsWith("h"))
                      .ToList();

            Assert.That(critters.Any(x => x.WrappedAttribute == "hooha"), Is.True);
            Assert.That(critters.Any(x => x.WrappedAttribute == "booja"), Is.False);
            Assert.That(critters.Count, Is.EqualTo(2));
        }


        [Test]
        public void QueryDictionaryContainer_WhereAttributeContainsValueAndKey_ReturnsCorrectResults()
        {
            var includedFirst = (DictionaryContainer)Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "booFirst" } } });
            Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "naaja" } } });
            var includedSecond = (DictionaryContainer)Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "booAgain" } } });
            Repository.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Other", "booAgain" } } });

            var results = TestQuery<IDictionaryContainer, DictionaryContainer>(
                x => x.Map.Contains("Lulu", y => y.StartsWith("boo")),
                x => x.Map.Contains("Lulu", y => y.StartsWith("boo")));

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new[] { includedFirst.Id, includedSecond.Id }));
        }


        [Test]
        public void QueryDictionaryContainer_WithDictonaryItemEquals_ReturnsCorrectStuff()
        {
            var matching = (DictionaryContainer)Repository.Post(
                new DictionaryContainer
                    {
                        Map = new Dictionary<string, string> { { "fubu", "bar" } }
                    });
            var notMatching = (DictionaryContainer)Repository.Post(
                new DictionaryContainer
                    {
                        Map = new Dictionary<string, string> { { "fubu", "nope" } }
                    });

            var resultIds = TestQuery<IDictionaryContainer, DictionaryContainer>(
                x => x.Map["fubu"] == "bar", x => x.Map["fubu"] == "bar").Select(x => x.Id);

            Assert.That(resultIds, Has.Member(matching.Id));
            Assert.That(resultIds, Has.No.Member(notMatching.Id));
        }


        [Test]
        public void QueryHasCustomEnum_ReturnsCorrectItems()
        {
            Repository.Post(new HasCustomEnum { TheEnumValue = CustomEnum.Tack });
            Repository.Post(new HasCustomEnum { TheEnumValue = CustomEnum.Tick });
            TestQuery<IHasCustomEnum, HasCustomEnum>(
                x => x.TheEnumValue == Critters.Client.CustomEnum.Tack, x => x.TheEnumValue == CustomEnum.Tack);
        }


        [Test]
        public void QueryMusicalCritter_WithBandNameEquals_ReturnsCorrectResult()
        {
            var musicalCritter =
                (MusicalCritter)Repository.CreateRandomCritter(rngSeed: 34242552, forceMusicalCritter: true);
            var bandName = musicalCritter.BandName;
            var critters =
                client.Query<IMusicalCritter>(x => x.BandName == bandName && x.Name == musicalCritter.Name);
            Assert.That(critters.Any(x => x.Id == musicalCritter.Id));
        }

        [Test]
        public void QueryMusicalCritter_WithPropertyOnlyOnMusicalCritterExpanded_ReturnsExpandedProperty()
        {
            var musicalCritter = client.Query<IMusicalCritter>().Expand(x => x.Instrument).First();
            // Check that we're not dealing with a lazy proxy
            Assert.That(musicalCritter.Instrument, Is.TypeOf<InstrumentResource>());
        }

        [Test]
        public void QueryNonExistingUrl_ThrowsResourceNotFoundException()
        {
            Assert.That(() => client.Get<Critter>(BaseUri + "critters/9999999"),
                        Throws.TypeOf<Common.Web.ResourceNotFoundException>());
        }

        [Test]
        public void QueryResourceWithEnumerable_PredicateOnEmumerable_ReturnsCorrectResults()
        {
            var musicalCritter = (MusicalCritter)Repository.CreateRandomCritter(forceMusicalCritter: true);
            var farms =
                client.Farms.Where(x => x.MusicalCritters.Any(y => y.BandName == musicalCritter.BandName)).ToList();
            Assert.That(farms.Any(x => x.MusicalCritters.Select(y => y.Id).Contains(musicalCritter.Id)));
        }

        [Test]
        public void QueryResourceWithExpandedEnumerable_ReturnsExpandedItems()
        {
            Repository.CreateRandomData(critterCount: 20);
            var farms = client.Farms.Query().Expand(x => x.MusicalCritters).ToList();
            var musicalCritters = farms.SelectMany(x => x.MusicalCritters).ToList();
            Assert.That(farms.All(x => !(x.MusicalCritters is LazyListProxy<IMusicalCritter>)));
            Assert.That(musicalCritters.Select(x => x.Id).OrderBy(x => x),
                        Is.EquivalentTo(CritterEntities.OfType<MusicalCritter>().Select(x => x.Id)));
        }

        [Test]
        public void QueryResourceWithNonExpandedEnumerable_ReturnsLazyItems()
        {
            Repository.CreateRandomData(critterCount: 20);
            var farms = client.Farms.Query().ToList();
            Assert.That(farms.All(x => x.MusicalCritters is LazyListProxy<IMusicalCritter>));
            var musicalCritters = farms.SelectMany(x => x.MusicalCritters).ToList();
            Assert.That(musicalCritters.Select(x => x.Id).OrderBy(x => x),
                        Is.EquivalentTo(CritterEntities.OfType<MusicalCritter>().Select(x => x.Id)));
        }

        [Test]
        public void QueryStringToObjectDictionaryContainer_ReturnsCorrectObject()
        {
            var entity =
                Repository.Save(new StringToObjectDictionaryContainer { Map = { { "foo", 1234 }, { "bar", "hoho" } } });

            var resource = client.Query<IStringToObjectDictionaryContainer>(x => x.Id == entity.Id).FirstOrDefault();

            Assert.IsNotNull(resource);
            Assert.That(resource.Map, Has.Count.EqualTo(2));
            Assert.IsTrue(resource.Map.ContainsKey("foo"));
            Assert.IsTrue(resource.Map.ContainsKey("bar"));
            Assert.That(resource.Map["foo"], Is.EqualTo(1234));
            Assert.That(resource.Map["bar"], Is.EqualTo("hoho"));
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

        [Test]
        public void Query_SelectNullableIntegerInAnonymousType_IsSuccessful()
        {
            var results = client.Critters.Query().Select(x => new { theNull = (int?)null }).Take(1).ToList();
            Assert.That(results.Select(x => x.theNull), Is.EquivalentTo(new[] { (int?)null }));
        }
    }
}