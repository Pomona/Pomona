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
using System.Reflection;

using CritterClient;

using NUnit.Framework;

using Pomona.Client;
using Pomona.Example;

using MusicalCritter = Pomona.Example.Models.MusicalCritter;

namespace CritterClientTests
{
    public class CritterModuleInternal : CritterModule
    {
        public CritterModuleInternal(CritterDataSource dataSource) : base(dataSource)
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
            this.critterHost = new CritterHost(new Uri(this.baseUri));
            this.critterHost.Start();
            this.client = new Client { BaseUri = this.baseUri };
        }


        [TestFixtureTearDown()]
        public void FixtureTearDown()
        {
            this.critterHost.Stop();
        }


        private CritterHost critterHost;
        private string baseUri;
        private ClientBase client;


        private IHat PostAHat(string hatType)
        {
            var hat = this.client.Post<IHat>(
                x => { x.HatType = hatType; });
            return hat;
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
                   assembly == typeof(Critter).Assembly ||
                   assembly == typeof(ClientBase).Assembly ||
                   assembly == typeof(Uri).Assembly;
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
        public void DeserializeCritters()
        {
            var critters = this.client.List<ICritter>("weapons.model");
            var allSubscriptions = critters.SelectMany(x => x.Subscriptions).ToList();
        }


        [Test]
        public void GetMusicalCritter()
        {
            var musicalCritterId =
                this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().OfType<MusicalCritter>().First().Id;

            var musicalCritter = this.client.GetUri<ICritter>(this.critterHost.BaseUri + "critter/" + musicalCritterId);

            Assert.That(musicalCritter, Is.AssignableTo<IMusicalCritter>());
        }


        [Test]
        public void PostCritterWithExistingHat()
        {
            const string hatType = "Old";

            var hat = PostAHat(hatType);

            const string critterName = "Super critter";

            var critter = this.client.Post<ICritter>(
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

            var critter = this.client.Post<ICritter>(
                x =>
                {
                    x.Hat = new HatForm() { HatType = hatType };
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostJunkWithRenamedProperty()
        {
            var propval = "Jalla jalla";
            var junk = this.client.Post<IJunkWithRenamedProperty>(x => { x.BeautifulAndExposed = propval; });

            Assert.That(junk.BeautifulAndExposed, Is.EqualTo(propval));
        }


        [Test]
        public void PostMusicalCritter()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = this.client.Post<IMusicalCritter>(
                x =>
                {
                    x.Hat = new HatForm() { HatType = hatType };
                    x.Name = critterName;
                    x.Instrument = "banana";
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
            Assert.That(critter.Instrument, Is.EqualTo("banana"));
        }



        [Test]
        public void QueryCritter_WithIdBetween_ReturnsCorrectResult()
        {
            var orderedCritters =
                this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().OrderBy(x => x.Id).Skip(2).Take(5).
                    ToList();
            var maxId = orderedCritters.Max(x => x.Id);
            var minId = orderedCritters.Min(x => x.Id);

            var critters = this.client.Query<ICritter>(x => x.Id >= minId && x.Id <= maxId);

            Assert.That(
                critters.OrderBy(x => x.Id).Select(x => x.Id), Is.EquivalentTo(orderedCritters.Select(x => x.Id)));
        }



        [Test]
        public void QueryCritter_WithDateEquals_ReturnsCorrectResult()
        {
            var firstCritter = this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().First();
            var createdOn = firstCritter.CreatedOn;
            var fetchedCritter = client.Query<ICritter>(x => x.CreatedOn == createdOn);

            Assert.That(fetchedCritter, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(fetchedCritter.First().Id, Is.EqualTo(firstCritter.Id));
        }


        [Test]
        public void QueryCritter_WithNameEqualsOrNameEqualsSomethingElse_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().First().Name;
            var nameOfSecondCritter =
                this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().Skip(1).First().Name;

            var critters =
                this.client.Query<ICritter>(x => x.Name == nameOfFirstCritter || x.Name == nameOfSecondCritter);

            Assert.That(critters.Any(x => x.Name == nameOfFirstCritter));
            Assert.That(critters.Any(x => x.Name == nameOfSecondCritter));
        }


        [Test]
        public void QueryCritter_WithNameEquals_ReturnsCorrectResult()
        {
            var nameOfFirstCritter = this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().First().Name;
            var critters = this.client.Query<ICritter>(x => x.Name == nameOfFirstCritter);
            Assert.That(critters.Any(x => x.Name == nameOfFirstCritter));
        }




        [Test]
        public void QueryMusicalCritter_WithInstrumentEquals_ReturnsCorrectResult()
        {
            var firstMusicalCritter = this.critterHost.DataSource.List<Pomona.Example.Models.Critter>().OfType<Pomona.Example.Models.MusicalCritter>().First();
            var instrument = firstMusicalCritter.Instrument;
            var critters = this.client.Query<IMusicalCritter>(x => x.Instrument == instrument);
            Assert.That(critters.Any(x => x.Id == firstMusicalCritter.Id));
        }
    }
}