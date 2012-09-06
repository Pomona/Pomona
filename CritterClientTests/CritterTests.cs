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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CritterClient;
using NUnit.Framework;
using Pomona.Client;
using Pomona.Example;

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
            baseUri = "http://localhost:4186/";
            critterHost = new CritterHost(new Uri(baseUri));
            critterHost.Start();
            client = new ClientHelper();
            client.BaseUri = baseUri;
        }


        [TearDown]
        public void TearDown()
        {
            critterHost.Stop();
        }

        #endregion

        private CritterHost critterHost;
        private string baseUri;
        private ClientHelper client;


        private IHat PostAHat(string hatType)
        {
            var hat = client.Post<IHat>(
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
            return assembly == typeof (object).Assembly ||
                   assembly == typeof (Critter).Assembly ||
                   assembly == typeof (ClientHelper).Assembly ||
                   assembly == typeof (Uri).Assembly;
        }


        [Test]
        public void AllPropertyTypesOfClientTypesAreAllowed()
        {
            var clientAssembly = typeof (ICritter).Assembly;
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
            var critters = client.List<ICritter>("weapons.model");
            var allSubscriptions = critters.SelectMany(x => x.Subscriptions).ToList();
        }


        [Test]
        public void PostCritterWithExistingHat()
        {
            const string hatType = "Old";

            var hat = PostAHat(hatType);

            const string critterName = "Super critter";

            var critter = client.Post<ICritter>(
                x =>
                    {
                        x.Hat = hat;
                        x.Name = critterName;
                    });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostCritterWithNewHat()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = client.Post<ICritter>(
                x =>
                    {
                        x.Hat = new NewHat() {HatType = hatType};
                        x.Name = critterName;
                    });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostJunkWithRenamedProperty()
        {
            var propval = "Jalla jalla";
            var junk = client.Post<IJunkWithRenamedProperty>(x => { x.BeautifulAndExposed = propval; });

            Assert.That(junk.BeautifulAndExposed, Is.EqualTo(propval));
        }
    }
}