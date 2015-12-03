#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using Pomona.Common.Internals;
using Pomona.Common.Linq;
using Pomona.Common.Proxies;
using Pomona.Example;
using Pomona.Example.Models;

using CustomEnum = Pomona.Example.Models.CustomEnum;
using CustomStringEnum = Pomona.Example.Models.CustomStringEnum;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class QueryTests : ClientTestsBase
    {
        [Test]
        public void GetLazyById_ReturnsLazyProxy()
        {
            var critterEntity = CritterEntities.Last(x => !(x is MusicalCritter));
            var critter = Client.Critters.GetLazy(critterEntity.Id);
            Assert.That(critter, Is.TypeOf<CritterLazyProxy>());
            var proxyBase = (LazyProxyBase)critter;
            Assert.That(proxyBase.ProxyTarget, Is.EqualTo(null));
            Assert.That(critter.Name, Is.EqualTo(critterEntity.Name));
            Assert.That(proxyBase.ProxyTarget, Is.TypeOf<CritterResource>());
        }


        [Test]
        public void GetResource_WithNonRepoCollectionProperty_ReturnsAllChildResources()
        {
            var thingEntity = new ThingWithCollectionNotExposedAsRepository();
            Enumerable.Range(0, 500).Select(x => new Hat() { HatType = x.ToString(), Style = "blah" }).AddTo(thingEntity.Hats);
            thingEntity.Hats.ForEach(x => Save(x));
            Save(thingEntity);
            var thingResource = Client.ThingWithCollectionNotExposedAsRepositories.Get(thingEntity.Id);
            Assert.That(thingResource.Hats.IsLoaded(), Is.False);
            Assert.That(thingResource.Hats.Count, Is.EqualTo(500));
        }


        [Test]
        public void GetResourceById_UsingClientRepository_ReturnsResource()
        {
            var critterEntity = CritterEntities.First();
            var critterResource = Client.Critters.Get(critterEntity.Id);
            Assert.That(critterResource, Is.Not.Null);
        }


        [Test]
        public void GetResourceById_WithExpandedProperty_ReturnedResourceHasExpandedProperty()
        {
            // TOOD: Get single item with expand is not yet implemented in client
            var critter = Client.Get<ICritter>(((IHasResourceUri)Client.Critters.GetLazy(CritterEntities.First().Id)).Uri,
                                               RequestOptions.Create<ICritter>(x => x.Expand(y => y.Weapons)));
            Assert.That(critter.Weapons.IsLoaded());
        }


        [Test]
        public void GetResourceWithReferenceToSubclassedResource_CreatesSubclassedLazyProxy()
        {
            var critterEntity = Repository.CreateRandomCritter(rngSeed : 65236);
            var referencedMusicalCritterEntity = Repository.CreateRandomCritter(rngSeed : 57823,
                                                                                forceMusicalCritter : true);
            critterEntity.ReferenceToAnotherCritter = referencedMusicalCritterEntity;

            var critterResource = Client.Critters.Get(critterEntity.Id);
            Assert.That(critterResource.ReferenceToAnotherCritter, Is.TypeOf<MusicalCritterLazyProxy>());
        }


        [Test]
        public void Query_SelectEnumInAnonymousType_IsSuccessful()
        {
            Save(new HasCustomEnum() { TheEnumValue = CustomEnum.Tock });
            var result = Client.HasCustomEnums.Select(x => new { val = x.TheEnumValue }).First();
            Assert.That(result.val, Is.EqualTo(Critters.Client.CustomEnum.Tock));
        }


        [Test]
        public void Query_SelectEnumInObjectArray_IsSuccessful()
        {
            Save(new HasCustomEnum() { TheEnumValue = CustomEnum.Tock });
            var result = Client.HasCustomEnums.Select(x => new object[] { x.TheEnumValue }).First();
            Assert.That(result[0], Is.EqualTo(Critters.Client.CustomEnum.Tock));
        }


        [Test]
        public void Query_SelectNullableIntegerInAnonymousType_IsSuccessful()
        {
            var results = Client.Critters.Query().Select(x => new { theNull = (int?)null }).Take(1).ToList();
            Assert.That(results.Select(x => x.theNull), Is.EquivalentTo(new[] { (int?)null }));
        }


        [Test]
        public void Query_SelectObjectArray_IsSuccessful()
        {
            var results = Client.Critters.Query().Select(x => new object[] { x.Id, x.Guid, x.Hat }).ToList();
            var comparableResults = results.Select(x => new { Id = (int)x[0], Guid = (Guid)x[1], HatId = ((IHat)x[2]).Id }).ToList();
            var expected = CritterEntities.Select(x => new { x.Id, x.Guid, HatId = x.Hat.Id });
            CollectionAssert.AreEqual(expected, comparableResults);
        }


        [Test]
        public void Query_Using_Last_Projection_SequenceHasElements_ReturnsLastElement()
        {
            var critterResource = Client.Critters.OrderBy(x => x.Id).Last();
            Assert.That(critterResource.Id, Is.EqualTo(CritterEntities.OrderBy(x => x.Id).Last().Id));
        }


        [Test]
        public void Query_Using_Last_Projection_SequenceIsEmpty_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Client.Critters.Last(x => x.Id == -1111111));
        }


        [Test]
        public void Query_Using_LastOrDefault_Projection_SequenceHasElements_ReturnsNull()
        {
            Assert.That(Client.Critters.LastOrDefault(x => x.Id == -1111111), Is.Null);
        }


        [Test]
        public void Query_Where_StringEnum_Equals_Constant_Is_Successful()
        {
            Save(new HasCustomStringEnum() { Value = CustomStringEnum.Cat });
            var result = Client.HasCustomStringEnums.First(x => x.Value == Critters.Client.CustomStringEnum.Cat);
            Assert.That(result.Value, Is.EqualTo(Critters.Client.CustomStringEnum.Cat));
        }


        [Test]
        public void QueryAgainstEntityWithRepositoryProperty_WithPredicateOnRepositoryProperty()
        {
            var firstCritterName = CritterEntities.First().Name;
            var farm = Client.Farms.Where(x => x.Critters.Any(y => y.Name == firstCritterName)).ToList();
        }


        [Test]
        public void QueryAgainstRepositoryOnEntity_ReturnsResultsRestrictedToEntity()
        {
            var farms = Client.Farms.Query().ToList();
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
                Client.Query<IHasCustomAttributes>(x => x.WrappedAttribute != null && x.WrappedAttribute.StartsWith("h"))
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
                x => x.Map["fubu"] == "bar",
                x => x.Map["fubu"] == "bar").Select(x => x.Id);

            Assert.That(resultIds, Has.Member(matching.Id));
            Assert.That(resultIds, Has.No.Member(notMatching.Id));
        }


        [Test]
        public void QueryExposedInterface_ReturnsSingleExposedInterfaceResource()
        {
            var entity =
                Save(new ExposedInterfaceInternalImplementation()
                {
                    FooBar = "lalala",
                    PropertyFromInheritedInterface = 123
                });
            var resultResource = Client.ExposedInterfaces.Query(x => x.FooBar == "lalala").First();
            Assert.That(entity.Id, Is.EqualTo(resultResource.Id));
        }


        [Test]
        public void QueryGalaxyHavingQuestionMarkInName_ReturnsCorrectResource()
        {
            // NOTE: This only works through NancyTestingWebClient. It will fail through self host or ASP.NET host.
            var galaxy = Client.Galaxies.Post(new GalaxyForm() { Name = "The Joker?" });
            Assert.That(galaxy.Name, Is.EqualTo("The Joker?"));
            galaxy = Client.Reload(galaxy);
            Assert.That(galaxy.Name, Is.EqualTo("The Joker?"));
        }


        [Test]
        public void QueryHasCustomEnum_ReturnsCorrectItems()
        {
            Repository.Post(new HasCustomEnum { TheEnumValue = CustomEnum.Tack });
            Repository.Post(new HasCustomEnum { TheEnumValue = CustomEnum.Tick });
            TestQuery<IHasCustomEnum, HasCustomEnum>(
                x => x.TheEnumValue == Critters.Client.CustomEnum.Tack,
                x => x.TheEnumValue == CustomEnum.Tack);
        }


        [Test]
        public void QueryHashSetContainer_WithSetExpanded_ReturnsCorrectHashSet()
        {
            var hashSetEntity =
                (SetCollectionContainer)Repository.Post(new SetCollectionContainer()
                {
                    TheSet = { (Critter)Repository.Post(Repository.CreateRandomCritter(rngSeed : 1235534)) }
                });
            var theSet =
                Client.SetCollectionContainers.Expand(x => x.TheSet).First(x => x.Id == hashSetEntity.Id).TheSet;
            Assert.That(theSet, Is.TypeOf<HashSet<ICritter>>());
            Assert.That(theSet.Count, Is.EqualTo(1));
        }


        [Test]
        public void QueryHashSetContainer_WithSetLazy_ReturnsCorrectHashSet()
        {
            var hashSetEntity =
                (SetCollectionContainer)Repository.Post(new SetCollectionContainer()
                {
                    TheSet = { (Critter)Repository.Post(Repository.CreateRandomCritter(rngSeed : 1235534)) }
                });
            var theSet = Client.SetCollectionContainers.First(x => x.Id == hashSetEntity.Id).TheSet;
            Assert.That(theSet, Is.TypeOf<LazySetProxy<ICritter>>());
            Assert.That(theSet.Count, Is.EqualTo(1));
        }


        [Test]
        public void QueryMusicalCritter_WithBandNameEquals_ReturnsCorrectResult()
        {
            var musicalCritter =
                (MusicalCritter)Repository.CreateRandomCritter(rngSeed : 34242552, forceMusicalCritter : true);
            var bandName = musicalCritter.BandName;
            var critters =
                Client.Query<IMusicalCritter>(x => x.BandName == bandName && x.Name == musicalCritter.Name);
            Assert.That(critters.Any(x => x.Id == musicalCritter.Id));
        }


        [Test]
        public void QueryMusicalCritter_WithPropertyOnlyOnMusicalCritterExpanded_ReturnsExpandedProperty()
        {
            var musicalCritter = Client.Query<IMusicalCritter>().Expand(x => x.Instrument).First();
            // Check that we're not dealing with a lazy proxy
            Assert.That(musicalCritter.Instrument, Is.TypeOf<InstrumentResource>());
        }


        [Test]
        public void QueryNonExistingUrl_ThrowsResourceNotFoundException()
        {
            Assert.That(() => Client.Get<Critter>(BaseUri + "critters/9999999"),
                        Throws.TypeOf<Common.Web.ResourceNotFoundException>());
        }


        [Test]
        public void QueryResourceWithEnumerable_PredicateOnEmumerable_ReturnsCorrectResults()
        {
            var musicalCritter = (MusicalCritter)Repository.CreateRandomCritter(forceMusicalCritter : true);
            var farms =
                Client.Farms.Where(x => x.MusicalCritters.Any(y => y.BandName == musicalCritter.BandName)).ToList();
            Assert.That(farms.Any(x => x.MusicalCritters.Select(y => y.Id).Contains(musicalCritter.Id)));
        }


        [Test]
        public void QueryResourceWithExpandedEnumerable_ReturnsExpandedItems()
        {
            Repository.CreateRandomData(critterCount : 20);
            var farms = Client.Farms.Query().Expand(x => x.MusicalCritters).ToList();
            var musicalCritters = farms.SelectMany(x => x.MusicalCritters).ToList();
            Assert.That(farms.All(x => x.MusicalCritters.IsLoaded()), Is.True);
            Assert.That(farms.SelectMany(x => x.MusicalCritters).All(x => x.IsLoaded()), Is.True);
            Assert.That(musicalCritters.Select(x => x.Id).OrderBy(x => x),
                        Is.EquivalentTo(CritterEntities.OfType<MusicalCritter>().Select(x => x.Id)));
        }


        [Test]
        public void QueryResourceWithNonExpandedEnumerable_ReturnsLazyItems()
        {
            Repository.CreateRandomData(critterCount : 20);
            var farms = Client.Farms.Query().ToList();
            Assert.That(farms.All(x => x.MusicalCritters is LazyListProxy<IMusicalCritter>));
            var musicalCritters = farms.SelectMany(x => x.MusicalCritters).ToList();
            Assert.That(musicalCritters.Select(x => x.Id).OrderBy(x => x),
                        Is.EquivalentTo(CritterEntities.OfType<MusicalCritter>().Select(x => x.Id)));
        }


        [Test]
        public void QueryResourceWithShallowExpandedEnumerable_ReturnsExpandedListOfReferences()
        {
            Repository.CreateRandomData(critterCount : 20);
            var farms = Client.Farms.Query().ExpandShallow(x => x.MusicalCritters).ToList();
            Assert.That(farms.All(x => x.MusicalCritters.IsLoaded()), Is.True);
            var musicalCritters = farms.SelectMany(x => x.MusicalCritters).ToList();
            Assert.That(farms.SelectMany(x => x.MusicalCritters).All(x => x.IsLoaded()), Is.False);
            Assert.That(musicalCritters.Select(x => x.Id).OrderBy(x => x),
                        Is.EquivalentTo(CritterEntities.OfType<MusicalCritter>().Select(x => x.Id)));
        }


        [Test]
        public void QueryStringToObjectDictionaryContainer_ReturnsCorrectObject()
        {
            var entity =
                Repository.Save(new StringToObjectDictionaryContainer { Map = { { "foo", 1234 }, { "bar", "hoho" } } });

            var resource = Client.Query<IStringToObjectDictionaryContainer>(x => x.Id == entity.Id).FirstOrDefault();

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
        public void Select_WithClientServerSplit_CallingMethodOnlyAvailableToClient_IsSuccessful()
        {
            var expected =
                CritterEntities.Select(x => new ClientSideClass(x.Id, x.Name + "|" + SomeCrazyMethod(x.Name))).ToList();
            var actual =
                Client.Critters.Select(x => new ClientSideClass(x.Id, x.Name + "|" + SomeCrazyMethod(x.Name))).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }


        [Test]
        public void Select_WithClientServerSplit_CallingMethodOnlyAvailableToClient_IsSuccessful_UsingFirstProjection()
        {
            var expected =
                CritterEntities.Select(x => new ClientSideClass(x.Id, x.Name + "|" + SomeCrazyMethod(x.Name))).First();
            var actual =
                Client.Critters.Select(x => new ClientSideClass(x.Id, x.Name + "|" + SomeCrazyMethod(x.Name))).First();

            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void Select_WithClientServerSplit_CallingNewOfTypeOnlyAvailableOnClient_IsSuccessful()
        {
            var expected =
                CritterEntities.Select(x => new ClientSideClass(x.Id, x.Name)).ToList();
            var actual =
                Client.Critters.Select(x => new ClientSideClass(x.Id, x.Name)).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }


        [Test]
        public void Select_WithClientServerSplit_CallingNewOfTypeOnlyAvailableOnClientWithMemberInit_IsSuccessful()
        {
            var expected =
                CritterEntities.Select(x => new ClientSideClass { AdditionalMember = x.Name }).ToList();
            var actual =
                Client.Critters.Select(x => new ClientSideClass { AdditionalMember = x.Name }).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }


        [Test]
        public void Select_WithClientServerSplit_UsingLocallyExecutedConcatMethod_IsSuccessful()
        {
            var expected =
                CritterEntities.Select(
                    x => new ClientSideClass(x.Id, x.Name.ToUpper() + "|" + SomeCrazyMethod(x.Name.ToLower()))).ToList();
            var actual =
                Client.Critters.Select(
                    x => new ClientSideClass(x.Id, x.Name.ToUpper() + "|" + SomeCrazyMethod(x.Name.ToLower()))).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }


        [Test]
        public void Select_WithClientServerSplit_UsingServerExecutedCountMethod_IsSuccessful()
        {
            var expected =
                CritterEntities.Select(
                    x =>
                        new ClientSideClass(x.Id + x.Subscriptions.Count,
                                            x.Name.ToUpper() + "|" + SomeCrazyMethod(x.Name.ToLower()))).ToList();
            var actual =
                Client.Critters.Select(
                    x =>
                        new ClientSideClass(x.Id + x.Subscriptions.Count,
                                            x.Name.ToUpper() + "|" + SomeCrazyMethod(x.Name.ToLower()))).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }


        [Test]
        public void Select_WithClientServerSplit_UsingServerExecutedLambdaPart_IsSuccessful()
        {
            var expected =
                CritterEntities.Select(
                    x =>
                        new ClientSideClass(x.Id + x.Subscriptions.Count(y => y.Sku != "xfiles"),
                                            x.Name.ToUpper() + "|" + SomeCrazyMethod(x.Name.ToLower()))).ToList();
            var actual =
                Client.Critters.Select(
                    x =>
                        new ClientSideClass(x.Id + x.Subscriptions.Count(y => y.Sku != "xfiles"),
                                            x.Name.ToUpper() + "|" + SomeCrazyMethod(x.Name.ToLower()))).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }


        private string SomeCrazyMethod(string s)
        {
            return new string(s.ToCharArray().Reverse().ToArray());
        }


        public class ClientSideClass : IEquatable<ClientSideClass>
        {
            public ClientSideClass()
            {
            }


            public ClientSideClass(int foo, string bar)
            {
                this.Foo = foo;
                this.Bar = bar;
            }


            public string AdditionalMember { get; set; }

            public string Bar { get; }

            public int Foo { get; }


            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((ClientSideClass)obj);
            }


            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Bar != null ? Bar.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Foo;
                    hashCode = (hashCode * 397) ^ (AdditionalMember != null ? AdditionalMember.GetHashCode() : 0);
                    return hashCode;
                }
            }


            public static bool operator ==(ClientSideClass left, ClientSideClass right)
            {
                return Equals(left, right);
            }


            public static bool operator !=(ClientSideClass left, ClientSideClass right)
            {
                return !Equals(left, right);
            }


            public bool Equals(ClientSideClass other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return string.Equals(Bar, other.Bar) && Foo == other.Foo
                       && string.Equals(AdditionalMember, other.AdditionalMember);
            }
        }
    }
}