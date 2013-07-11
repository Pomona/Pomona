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
        public int TestIntProperty { get; set; }

        [Test]
        public void QueryCritter_AnyWithExistingName_ReturnsTrue()
        {
            // Just take some random critter
            var critter = CritterEntities.First();
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

        [Test]
        public void QueryCritter_FirstLazy_ReturnsLazyCritter()
        {
            var expected = CritterEntities.First(x => x.Id%3 == 0);
            var lazyCritter = client.Query<ICritter>().Where(x => x.Id%3 == 0).FirstLazy();
            var beforeLoadUri = ((IHasResourceUri) lazyCritter).Uri;
            Assert.That(beforeLoadUri, Is.StringContaining("$filter=(id+mod+3)+eq+0"));
            Console.WriteLine(beforeLoadUri);
            // Should load uri when retrieving name
            var name = lazyCritter.Name;
            var afterLoadUri = ((IHasResourceUri) lazyCritter).Uri;
            Assert.That(afterLoadUri, Is.Not.StringContaining("$filter=(id+mod+3)+eq+0"));
            Console.WriteLine(afterLoadUri);
            Assert.That(name, Is.EqualTo(expected.Name));
        }

        [Test]
        public void QueryCritter_GetMaxId_ReturnsMaxId()
        {
            var expected = DataSource.List<Critter>().Max(x => x.Id);
            Assert.That(client.Critters.Query().Max(x => x.Id), Is.EqualTo(expected));
            Assert.That(client.Critters.Query().Select(x => x.Id).Max(), Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_GetMinId_ReturnsMinId()
        {
            var expected = DataSource.List<Critter>().Min(x => x.Id);

            Assert.That(client.Critters.Query().Min(x => x.Id), Is.EqualTo(expected));
            Assert.That(client.Critters.Query().Select(x => x.Id).Min(), Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_GetSumOfDecimalProperty()
        {
            var expected = CritterEntities.Sum(x => (decimal) x.Id);
            var actual = client.Query<ICritter>().Sum(x => (decimal) x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_GetSumOfDoubleProperty()
        {
            var expected = CritterEntities.Sum(x => (double) x.Id);
            var actual = client.Query<ICritter>().Sum(x => (double) x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_GetSumOfIntProperty()
        {
            var expected = CritterEntities.Sum(x => x.Name.Length);
            var actual = client.Query<ICritter>().Sum(x => x.Name.Length);
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
        public void QueryCritter_OfType()
        {
            var critters =
                client.Query<ICritter>()
                      .Where(x => x.Id > 0)
                      .OfType<IMusicalCritter>()
                      .Where(x => x.Instrument.Type != "stupid")
                      .ToList();
            Assert.That(critters.Count, Is.GreaterThan(0));
        }

        [Test]
        public void QueryCritter_OrderByAfterSelect_ReturnsCorrectValues()
        {
            var expected =
                CritterEntities
                    .Select(x => new {NameLength = x.Name.Length})
                    .OrderBy(x => x.NameLength)
                    .Take(10)
                    .ToList();
            var actual =
                client.Critters.Query()
                      .Select(x => new {NameLength = x.Name.Length})
                      .OrderBy(x => x.NameLength)
                      .Take(10)
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
        public void QueryCritter_SelectDecimalThenSum()
        {
            var expected = CritterEntities.Select(x => (decimal) x.Id).Sum();
            var actual = client.Query<ICritter>().Select(x => (decimal) x.Id).Sum();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_SelectDoubleThenSum()
        {
            var expected = CritterEntities.Select(x => (double) x.Id).Sum();
            var actual = client.Query<ICritter>().Select(x => (double) x.Id).Sum();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_SelectIntThenSum()
        {
            var expected = CritterEntities.Select(x => x.Name.Length).Sum();
            var actual = client.Query<ICritter>().Select(x => x.Name.Length).Sum();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryCritter_SelectThenWhereThenSelect_ReturnsCorrectValues()
        {
            var expected = CritterEntities
                .Select(x => new {c = x, isHeavyArmed = x.Weapons.Count > 2, farmName = x.Farm.Name})
                .Where(x => x.isHeavyArmed)
                .Select(x => new {critterName = x.c.Name, x.farmName})
                .Take(5)
                .ToList();

            var actual = client.Query<ICritter>()
                               .Select(x => new {c = x, isHeavyArmed = x.Weapons.Count > 2, farmName = x.Farm.Name})
                               .Where(x => x.isHeavyArmed)
                               .Select(x => new {critterName = x.c.Name, x.farmName})
                               .Take(5)
                               .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }

        [Test]
        public void QueryCritter_ToUri_ReturnsUriForQuery()
        {
            var uri = client.Query<ICritter>().Where(x => x.Name == "holahola").ToUri();
            Assert.That(uri.PathAndQuery, Is.EqualTo("/critters?$filter=name+eq+'holahola'"));
        }

        [Test]
        public void QueryCritter_WhereExpressionCapturingPropertyFromClass_EvaluatesToConstantCorrectly()
        {
            var critter = DataSource.CreateRandomCritter();
            TestIntProperty = critter.Id;
            var result = client.Critters.Query(x => x.Id == TestIntProperty).FirstOrDefault();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(TestIntProperty));
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
        public void QueryCritter_WhereFirstOrDefaultFromWeapons_ReturnsCorrectValues_ManyTimes()
        {
            var expected =
                CritterEntities.Where(
                    x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                               .Take(5)
                               .ToList();

            for (var i = 0; i < 100; i++)
            {
                var actual =
                    client.Query<ICritter>()
                          .Where(x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                          .Expand(x => x.Weapons)
                          .Take(5)
                          .ToList();
                Assert.That(actual.Select(x => x.Id), Is.EquivalentTo(expected.Select(x => x.Id)));
            }
        }


        [Test]
        public void QueryCritter_WhereFirst_ReturnsCorrectCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(1).Take(1).First();
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
            var critter = CritterEntities.OfType<MusicalCritter>().Take(1).First();
            // Search by its name
            var critterResource =
                client.Query<IMusicalCritter>().First(
                    x => x.Name == critter.Name && x.Guid == critter.Guid && x.BandName == critter.BandName);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }

        [Test]
        public void Query_UsingFirstOrDefault_WithNoMatches_ReturnsNull()
        {
            var result = client.Critters.Query().Where(x => x.Name == Guid.NewGuid().ToString()).FirstOrDefault();

            Assert.That(result, Is.Null);
        }
    }
}