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
using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common.Linq;
using Pomona.Example.Models;

namespace Pomona.SystemTests.Linq
{
    [TestFixture]
    public class LinqQueryTests : ClientTestsBase
    {
        public interface ICustomTestEntity : IDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }

        public interface ICustomTestEntity2 : ISubtypedDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }


        [Test]
        public void QueryCritter_AnyWithExistingName_ReturnsTrue()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(6).Take(1).First();
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
                CritterEntities
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
                CritterEntities
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
            var critter = CritterEntities.Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                this.client.Query<ICritter>().First(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WhereThenSelectAnonymousClass_ReturnsCorrectValues()
        {
            var expected = CritterEntities
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
            var expected = CritterEntities.OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList();
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


        [Test]
        public void QueryCustomTestEntity2_WhereDictIsOnBaseInterface_ReturnsCustomTestEntity2()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));
            var subtypedDictionaryContainer = new SubtypedDictionaryContainer
            {
                Map = { { "CustomString", "Lalalala" }, { "OtherCustom", "Blob rob" } },
                SomethingExtra = "Hahahohohihi"
            };
            this.critterHost.DataSource.Save<DictionaryContainer>(
                subtypedDictionaryContainer);

            // Post does not yet work on subtypes
            //this.client.DictionaryContainers.Post<ISubtypedDictionaryContainer>(
            //    x =>
            //    {
            //        x.Map.Add("CustomString", "Lalalala");
            //        x.Map.Add("OtherCustom", "Blob rob");
            //        x.SomethingExtra = "Hahahohohihi";
            //    });

            var results = this.client.Query<ICustomTestEntity2>()
                .Where(
                    x =>
                    x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob" && x.SomethingExtra == "Hahahohohihi")
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(subtypedDictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(subtypedDictionaryContainer.Map["CustomString"]));
        }


        [Test]
        public void QueryCustomTestEntity_ReturnsCustomTestEntity()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = this.client.DictionaryContainers.Post(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });

            var results = this.client.Query<ICustomTestEntity>()
                .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }


        [Test]
        public void QuerySubclassedMusicalCritter_WhereFirst_ReturnsCorrectMusicalCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.OfType<MusicalCritter>().Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                this.client.Query<IMusicalCritter>().First(
                    x => x.Name == critter.Name && x.Guid == critter.Guid && x.BandName == critter.BandName);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }
    }
}