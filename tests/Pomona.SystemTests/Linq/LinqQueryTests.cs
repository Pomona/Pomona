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

using System;
using System.Linq;
using System.Linq.Expressions;
using Critters.Client;
using NUnit.Framework;
using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Example.Models;
using Pomona.TestHelpers;

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

        public interface ICustomTestEntity3 : IStringToObjectDictionaryContainer
        {
            string Text { get; set; }
            int? Number { get; set; }
            DateTime? Time { get; set; }
        }


        [Test]
        public void QueryCritter_AnyWithExistingName_ReturnsTrue()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(6).Take(1).First();
            var hasCritterWithGuid =
                client.Critters.Query().Any(x => x.Name == critter.Name);
            Assert.That(hasCritterWithGuid, Is.True);
        }

        [Test]
        public void QueryCritter_AnyWithNameEqualToRandomGuid_ReturnsFalse()
        {
            var hasCritterWithGuid =
                client.Query<ICritter>().Any(x => x.Name == Guid.NewGuid().ToString());
            Assert.That(hasCritterWithGuid, Is.False);
        }

        [Category("TODO")]
        [Test(Description = "Need to implement custom projections to make sum possible")]
        public void QueryCritter_GetSumOfIntProperty()
        {
            var expected = CritterEntities.Where(x => x.Name.StartsWith("B")).Sum(x => x.Name.Length);
            var actual = client.Query<ICritter>().Where(x => x.Name.StartsWith("B")).Sum(x => x.Name.Length);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GroupByThenSelectAnonymousClassThenOrderBy_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var expected =
                CritterEntities
                    .Where(x => x.Id%2 == 0)
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
                client.Query<ICritter>()
                      .Where(x => x.Id%2 == 0)
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
                    .Where(x => x.Id%2 == 0)
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
                client.Query<ICritter>()
                      .Where(x => x.Id%2 == 0)
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
        public void QueryCritter_QueryingPropertyOfBaseClass_ReflectedTypeOfPropertyInPomonaQueryIsCorrect()
        {
            // Fix: We don't want the parsed expression trees to give us members with "ReflectedType" set to inherited type, but same as DeclaringType.

            // Result of below query not important..
            client.Critters.Query().Where(x => x.Id == 666).ToList();

            var query = DataSource.QueryLog.Last();
            var binExpr = query.FilterExpression.Body as BinaryExpression;
            Assert.That(binExpr, Is.Not.Null);
            Assert.That(binExpr.NodeType, Is.EqualTo(ExpressionType.Equal));
            var propExpr = binExpr.Left as MemberExpression;
            Assert.That(propExpr, Is.Not.Null);
            Assert.That(propExpr.Member.ReflectedType, Is.EqualTo(propExpr.Member.DeclaringType));
        }

        [Test]
        public void QueryCritter_WhereFirstOrDefaultFromWeapons_ReturnsCorrectValues()
        {
            var expected =
                CritterEntities.Where(
                    x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                               .Take(5)
                               .ToList();
            var actual =
                client.Query<ICritter>()
                      .Where(x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                      .Expand(x => x.Weapons)
                      .Take(5)
                      .ToList();

            Assert.That(actual.Select(x => x.Id), Is.EquivalentTo(expected.Select(x => x.Id)));
        }


        [Test]
        public void QueryCritter_WhereFirst_ReturnsCorrectCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                client.Query<ICritter>().First(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WhereThenSelectAnonymousClass_ReturnsCorrectValues()
        {
            var expected = CritterEntities
                .Where(x => x.Id%2 == 0)
                .Select(x => new {x.Name, Crazy = x.CrazyValue.Sickness})
                .OrderBy(x => x.Name)
                .Take(10)
                .ToList();
            var actual =
                client.Query<ICritter>()
                      .Where(x => x.Id%2 == 0)
                      .Select(x => new {x.Name, Crazy = x.CrazyValue.Sickness})
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
                client.Query<ICritter>().OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList().ToList();
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_WithAttributeEquals_ReturnsCorrectCritter()
        {
            var critter =
                client.Critters.Query().Where(
                    x => x.SimpleAttributes.Any(y => y.Key == "AttrKey" && y.Value == "dde")).ToList();
        }

        [Test]
        public void QueryCritter_WithDecompiledGeneratedProperty_UsesPropertyFormula()
        {
            client.Critters.Query(x => x.DecompiledGeneratedProperty == 0x1337).ToList();
            var query = DataSource.QueryLog.Last();
            Expression<Func<Critter, bool>> expectedFilter = _this => (_this.Id + 100) == 0x1337;
            query.FilterExpression.AssertEquals(expectedFilter);
        }


        [Test]
        public void QueryCritter_WithExpandedPropertyOfAnonymousClass_HasPropertyExpanded()
        {
            var result =
                client.Critters.Query()
                      .Select(x => new {TheHat = x.Hat, x.Name})
                      .OrderBy(x => x.Name)
                      .Expand(x => x.TheHat)
                      .Take(1)
                      .First();
            Assert.That(result.TheHat, Is.TypeOf<HatResource>());
        }


        [Test]
        public void QueryCritter_WithExpandedProperty_HasPropertyExpanded()
        {
            var result = client.Critters.Query().Expand(x => x.Hat).Take(1).First();
            Assert.That(result.Hat, Is.TypeOf<HatResource>());
        }

        [Test]
        public void QueryCritter_WithHandledGeneratedProperty_UsesPropertyFormula()
        {
            client.Critters.Query(x => x.HandledGeneratedProperty == 3).ToList();
            var query = DataSource.QueryLog.Last();
            Expression<Func<Critter, bool>> expectedFilter = _this => (_this.Id%6) == 3;
            query.FilterExpression.AssertEquals(expectedFilter);
        }

        [Test]
        public void QueryCritter_WithIncludeTotalCount_IncludesTotalCount()
        {
            var expectedTotalCount = CritterEntities.Count(x => x.Id%3 == 0);
            var results = client
                .Critters
                .Query()
                .Where(x => x.Id%3 == 0)
                .IncludeTotalCount()
                .ToQueryResult();
            Assert.That(results.TotalCount, Is.EqualTo(expectedTotalCount));
        }

        [Test]
        public void QueryCritter_WithUnhandledGeneratedProperty_ThrowsExceptionUsingCritterDataSource()
        {
            Assert.That(() => client.Critters.Query(x => x.UnhandledGeneratedProperty == "nono").ToList(),
                        Throws.Exception);
        }

        [Test]
        public void QueryCustomTestEntity2_WhereDictIsOnBaseInterface_ReturnsCustomTestEntity2()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));
            var subtypedDictionaryContainer = new SubtypedDictionaryContainer
                {
                    Map = {{"CustomString", "Lalalala"}, {"OtherCustom", "Blob rob"}},
                    SomethingExtra = "Hahahohohihi"
                };
            critterHost.DataSource.Save<DictionaryContainer>(
                subtypedDictionaryContainer);

            // Post does not yet work on subtypes
            //this.client.DictionaryContainers.Post<ISubtypedDictionaryContainer>(
            //    x =>
            //    {
            //        x.Map.Add("CustomString", "Lalalala");
            //        x.Map.Add("OtherCustom", "Blob rob");
            //        x.SomethingExtra = "Hahahohohihi";
            //    });

            var results = client.Query<ICustomTestEntity2>()
                                .Where(
                                    x =>
                                    x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob" &&
                                    x.SomethingExtra == "Hahahohohihi")
                                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(subtypedDictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(subtypedDictionaryContainer.Map["CustomString"]));
        }

        [Test]
        public void QueryCustomTestEntity3_WhereDictIsStringToObject_ReturnsCustomTestEntity3()
        {
            var timeValue = new DateTime(2042, 2, 4, 6, 3, 2);
            var dictContainer = DataSource.Save(new StringToObjectDictionaryContainer
                {
                    Map = {{"Text", "foobar"}, {"Number", 32}, {"Time", timeValue}}
                });

            var results = client.Query<ICustomTestEntity3>()
                                .Where(x => x.Number > 5 && x.Text == "foobar" && x.Time == timeValue)
                                .ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            var result = results.First();
            Assert.That(result.Number, Is.EqualTo(32));
            Assert.That(result.Text, Is.EqualTo("foobar"));
            Assert.That(result.Time, Is.EqualTo(timeValue));
            Assert.That(result.Id, Is.EqualTo(dictContainer.Id));
        }


        [Test]
        public void QueryCustomTestEntity_ReturnsCustomTestEntity()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = client.DictionaryContainers.Post(
                x =>
                    {
                        x.Map.Add("CustomString", "Lalalala");
                        x.Map.Add("OtherCustom", "Blob rob");
                    });

            var results = client.Query<ICustomTestEntity>()
                                .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }

        [Test(Description = "Known to fail, must fix!")]
        public void QueryCustomTestEntity_UsingFirstOrDefault_ReturnsCustomTestEntity()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = client.DictionaryContainers.Post(
                x =>
                    {
                        x.Map.Add("CustomString", "Lalalala");
                        x.Map.Add("OtherCustom", "Blob rob");
                    });

            var result =
                client.Query<ICustomTestEntity>()
                      .FirstOrDefault(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob");

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }

        [Test]
        public void QueryHasStringToObjectDictionary_ReturnsCorrectValues()
        {
            for (var i = 1; i <= 8; i++)
            {
                DataSource.Save(new StringToObjectDictionaryContainer {Map = {{"square", i*i}}});
            }

            // Should get 36, 49 and 64
            var results = client.Query<IStringToObjectDictionaryContainer>()
                                .Where(x => x.Map.SafeGet("square") as int? > 26)
                                .ToList();

            Assert.That(results, Has.Count.EqualTo(3));
        }


        [Test]
        public void QuerySubclassedMusicalCritter_WhereFirst_ReturnsCorrectMusicalCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.OfType<MusicalCritter>().Skip(6).Take(1).First();
            // Search by its name
            var critterResource =
                client.Query<IMusicalCritter>().First(
                    x => x.Name == critter.Name && x.Guid == critter.Guid && x.BandName == critter.BandName);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }
    }
}