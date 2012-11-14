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
using System.Reflection;
using System.Text;

using CritterClient;

using NUnit.Framework;

using Pomona;
using Pomona.Client;
using Pomona.Example;
using Pomona.Example.Models;

namespace CritterClientTests
{
    public class CritterModuleInternal : CritterModule
    {
        public CritterModuleInternal(CritterDataSource dataSource, TypeMapper typeMapper) : base(dataSource, typeMapper)
        {
        }
    }

    /// <summary>
    /// Tests for generated assembly
    /// </summary>
    [TestFixture]
    public class CritterTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.critterHost.DataSource.ResetTestData();
        }

        #endregion

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var rng = new Random();
            this.baseUri = "http://localhost:" + rng.Next(10000, 23000) + "/";
            Console.WriteLine("Starting CritterHost on " + this.baseUri);
            this.critterHost = new CritterHost(new Uri(this.baseUri));
            this.critterHost.Start();
            this.client = new Client(this.baseUri);
        }


        [TestFixtureTearDown()]
        public void FixtureTearDown()
        {
            this.critterHost.Stop();
        }


        private CritterHost critterHost;
        private string baseUri;
        private Client client;


        private IHat PostAHat(string hatType)
        {
            var hat = this.client.Post<IHat>(
                x => { x.HatType = hatType; });
            return (IHat)hat;
        }


        private IEnumerable<Type> FlattenGenericTypeHierarchy(Type t)
        {
            if (t.IsGenericType)
            {
                yield return t.GetGenericTypeDefinition();
                foreach (var genarg in t.GetGenericArguments())
                {
                    foreach (var gent in FlattenGenericTypeHierarchy(genarg))
                        yield return gent;
                }
            }
            else
                yield return t;
        }


        private bool IsAllowedType(Type t)
        {
            return FlattenGenericTypeHierarchy(t).All(x => IsAllowedClientReferencedAssembly(x.Assembly));
        }


        private bool IsAllowedClientReferencedAssembly(Assembly assembly)
        {
            return assembly == typeof(object).Assembly ||
                   assembly == typeof(ICritter).Assembly ||
                   assembly == typeof(ClientBase).Assembly ||
                   assembly == typeof(Uri).Assembly;
        }


        public void TestQuery<TResource, TEntity>(
            Expression<Func<TResource, bool>> resourcePredicate, Func<TEntity, bool> entityPredicate)
            where TResource : IEntityBase
            where TEntity : EntityBase
        {
            var entities =
                this.critterHost.DataSource.List<TEntity>().Where(entityPredicate).OrderBy(x => x.Id).ToList();
            var fetchedResources = this.client.Query(resourcePredicate, top : 10000);
            Assert.That(fetchedResources.Select(x => x.Id), Is.EquivalentTo(entities.Select(x => x.Id)));
        }


        private ICollection<Critter> CritterEntities
        {
            get { return this.critterHost.DataSource.List<Critter>(); }
        }


        [Test]
        public void AllPropertyTypesOfClientTypesAreAllowed()
        {
            var clientAssembly = typeof(ICritter).Assembly;
            var allPropTypes =
                clientAssembly.GetExportedTypes().SelectMany(
                    x => x.GetProperties().Select(y => y.PropertyType)).Distinct();

            var allTypesOk = true;
            foreach (var type in allPropTypes)
            {
                if (!IsAllowedType(type))
                {
                    allTypesOk = false;
                    var typeLocal = type;
                    var propsWithType = clientAssembly
                        .GetExportedTypes()
                        .SelectMany(x => x.GetProperties())
                        .Where(x => x.PropertyType == typeLocal).ToList();
                    foreach (var propertyInfo in propsWithType)
                    {
                        Console.WriteLine(
                            "Property {0} of {1} has type {2} of assembly {3}, which should not be referenced by client!",
                            propertyInfo.Name,
                            propertyInfo.DeclaringType.FullName,
                            propertyInfo.PropertyType.FullName,
                            propertyInfo.PropertyType.Assembly.FullName);
                    }
                }
            }

            Assert.IsTrue(allTypesOk, "There was properties in CritterClient with references to disallowed assemblies.");
        }


        [Test]
        public void ClientLibraryIsCorrectlyGenerated()
        {
            var foundError = false;
            var errors = new StringBuilder();
            foreach (
                var prop in
                    this.client.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                        x =>
                        x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof(ClientRepository<,>)))
            {
                var value = prop.GetValue(this.client, null);
                if (value == null)
                {
                    foundError = true;
                    errors.AppendFormat("Property {0} of generated client lib is null\r\n", prop.Name);
                }
                if (prop.GetSetMethod(true).IsPublic)
                {
                    foundError = true;
                    errors.AppendFormat("Property {0} of generated client lib has a public setter.\r\n", prop.Name);
                }
            }

            if (foundError)
                Assert.Fail("Found the following errors on generated client lib: {0}\r\n", errors);
        }


        [Test]
        public void DeserializeCritters()
        {
            for (var i = 0; i < 1; i++)
                this.client.Query<ICritter>(x => true, "weapons.model", top : 100);

            //var allSubscriptions = critters.SelectMany(x => x.Subscriptions).ToList();
        }


        [Test]
        public void GetMusicalCritter()
        {
            var musicalCritterId = CritterEntities.OfType<MusicalCritter>().First().Id;

            var musicalCritter = this.client.GetUri<ICritter>(this.critterHost.BaseUri + "critters/" + musicalCritterId);

            Assert.That(musicalCritter, Is.AssignableTo<IMusicalCritter>());
        }


        [Test]
        public void GetWeaponsLazy_FromCritter()
        {
            var critter = this.client.List<ICritter>().First();
            var weapons = critter.Weapons.ToList();
        }


        [Test]
        public void PostCritterWithExistingHat()
        {
            const string hatType = "Old";

            var hat = PostAHat(hatType);

            const string critterName = "Super critter";

            var critter = (ICritter)this.client.Post<ICritter>(
                x =>
                {
                    x.Hat = hat;
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostCritterWithHatForm()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = (ICritter)this.client.Post<ICritter>(
                x =>
                {
                    x.Hat = new HatForm() { HatType = hatType };
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostDictionaryContainer_WithItemSetInDictionary()
        {
            var response = (IDictionaryContainer)this.client.Post<IDictionaryContainer>(x => { x.Map["cow"] = "moo"; });
            Assert.That(response.Map.ContainsKey("cow"));
            Assert.That(response.Map["cow"] == "moo");
        }


        [Test]
        public void PostJunkWithRenamedProperty()
        {
            var propval = "Jalla jalla";
            var junk =
                (IJunkWithRenamedProperty)
                this.client.Post<IJunkWithRenamedProperty>(x => { x.BeautifulAndExposed = propval; });

            Assert.That(junk.BeautifulAndExposed, Is.EqualTo(propval));
        }


        [Test]
        public void PostMusicalCritter()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = (IMusicalCritter)this.client.Post<IMusicalCritter>(
                x =>
                {
                    x.Hat = new HatForm() { HatType = hatType };
                    x.Name = critterName;
                    x.BandName = "banana";
                    x.Instrument = new InstrumentForm() { Type = "helo" };
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
            Assert.That(critter.BandName, Is.EqualTo("banana"));
        }


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
        public void QueryCritter_ReturnsExpandedProperties()
        {
            var critter = this.client.Query<ICritter>(x => true, expand : "hat,weapons").First();
            // Check that we're not dealing with a lazy proxy
            Assert.That(critter.Hat, Is.TypeOf<HatResource>());
            Assert.That(critter.Weapons, Is.Not.TypeOf<LazyListProxy<IWeapon>>());
            Assert.That(critter.Subscriptions, Is.TypeOf<LazyListProxy<ISubscription>>());
        }


        [Test]
        public void QueryCritter_RoundDecimal_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => decimal.Round(3.33m) == 3m, x => decimal.Round(3.33m) == 3m);
        }


        [Test]
        public void QueryCritter_RoundDouble_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => Math.Round(3.33) == 3.0, x => Math.Round(3.33) == 3.0);
        }


        [Test]
        public void QueryCritter_SearchByAttribute()
        {
            TestQuery<ICritter, Critter>(
                x => x.SimpleAttributes.Any(y => y.Key == "Moo" && y.Value == "Boo"),
                x => x.SimpleAttributes.Any(y => y.Key == "Moo" && y.Value == "Boo"));
            Assert.Fail("Test is stupid");
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
            var fetchedCritters = this.client.Query<ICritter>(x => x.CreatedOn > fromTime && x.CreatedOn <= toTime);

            Assert.Fail("Remove this test..");
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
        public void QueryCritter_WithWeaponsCountIsGreaterThan7_ReturnsCorrectCritters()
        {
            TestQuery<ICritter, Critter>(x => x.Weapons.Count > 7, x => x.Weapons.Count > 7);
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
        public void QueryWeapons_WithFilterOnDecimal_ReturnsCorrectCritters()
        {
            TestQuery<IWeapon, Weapon>(x => x.Price < 500m, x => x.Price < 500m);
        }


        [Test]
        public void QueryWeapons_WithFilterOnDouble_ReturnsCorrectCritters()
        {
            TestQuery<IWeapon, Weapon>(x => x.Dependability > 0.8, x => x.Dependability > 0.8);
        }
    }
}