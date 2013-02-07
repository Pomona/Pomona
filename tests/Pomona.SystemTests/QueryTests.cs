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
using Pomona.Example.Models;

using CustomEnum = Pomona.Example.Models.CustomEnum;

namespace Pomona.SystemTests
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
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "booja" } } });
            DataSource.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "WrappedAttribute", "booja" } } });
            DataSource.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "WrappedAttribute", "hooha" } } });
            DataSource.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "WrappedAttribute", "halala" } } });

            var critters = this.client.Query<IHasCustomAttributes>(
                this.client.BaseUri + "dictionary-containers", x => x.WrappedAttribute.StartsWith("h"));

            Assert.That(critters.Any(x => x.WrappedAttribute == "hooha"), Is.True);
            Assert.That(critters.Any(x => x.WrappedAttribute == "booja"), Is.False);
            Assert.That(critters.Count, Is.EqualTo(2));
        }


        [Test]
        public void QueryDictionaryContainer_WhereAttributeContainsValueAndKey_ReturnsCorrectResults()
        {
            var includedFirst = (DictionaryContainer)DataSource.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "booFirst" } } });
            DataSource.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "naaja" } } });
            var includedSecond = (DictionaryContainer)DataSource.Post(
                new DictionaryContainer { Map = new Dictionary<string, string> { { "Lulu", "booAgain" } } });
            DataSource.Post(
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
            var matching = (DictionaryContainer)DataSource.Post(
                new DictionaryContainer
                {
                    Map = new Dictionary<string, string> { { "fubu", "bar" } }
                });
            var notMatching = (DictionaryContainer)DataSource.Post(
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
            DataSource.Post(new HasCustomEnum { TheEnumValue = CustomEnum.Tack });
            DataSource.Post(new HasCustomEnum { TheEnumValue = CustomEnum.Tick });
            TestQuery<IHasCustomEnum, HasCustomEnum>(
                x => x.TheEnumValue == Critters.Client.CustomEnum.Tack, x => x.TheEnumValue == CustomEnum.Tack);
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