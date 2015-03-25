#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Common.Loading;
using Pomona.Common.Web;
using Pomona.Example.Models;
using Pomona.TestHelpers;

namespace Pomona.SystemTests.Linq
{
    [TestFixture]
    public class LinqQueryTests : ClientTestsBase
    {
        public int TestIntProperty { get; set; }


        [Test]
        public void Query_Critter_ToJson_ReturnsJObject()
        {
            var critter = Client.Critters.Query().Where(x => x.Id > 3).ToJson();
            var items = critter.AssertHasPropertyWithArray("items");
        }


        [Test]
        public void Query_UsingFirstOrDefault_WithNoMatches_ReturnsNull()
        {
            var result = Client.Critters.Query().Where(x => x.Name == Guid.NewGuid().ToString()).FirstOrDefault();

            Assert.That(result, Is.Null);
        }


        [Test]
        public void QueryCritter_Any_ReturnsTrue()
        {
            var any = Client.Critters.Query().Any();
            Assert.That(any, Is.True);
        }


        [Test]
        public void QueryCritter_AnyWithExistingName_ReturnsTrue()
        {
            // Just take some random critter
            var critter = CritterEntities.First();
            var hasCritterWithGuid =
                Client.Critters.Query().Any(x => x.Name == critter.Name);
            Assert.That(hasCritterWithGuid, Is.True);
        }


        [Test]
        public void QueryCritter_AnyWithNameEqualToRandomGuid_ReturnsFalse()
        {
            var hasCritterWithGuid =
                Client.Query<ICritter>().Any(x => x.Name == Guid.NewGuid().ToString());
            Assert.That(hasCritterWithGuid, Is.False);
        }


        [Test]
        public void QueryCritter_Count_ReturnsCount()
        {
            var expected = Repository.List<Critter>().Count;
            Assert.That(Client.Critters.Query().Count(), Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_FirstLazy_ReturnsLazyCritter()
        {
            var randCritter = Repository.CreateRandomCritter(new Random());
            var expected = CritterEntities.First(x => x.Id == randCritter.Id);
            var lazyCritter = Client.Query<ICritter>()
                                    .Where(x => x.Id == randCritter.Id)
                // Explicit call to FirstLazy should succeed.
                                    .FirstLazy();
            var beforeLoadUri = ((IHasResourceUri)lazyCritter).Uri;
            var predicate = string.Format("$filter=id+eq+{0}", randCritter.Id);
            Assert.That(beforeLoadUri, Is.StringContaining(predicate));
            Console.WriteLine(beforeLoadUri);
            // Should load uri when retrieving name
            var name = lazyCritter.Name;
            var afterLoadUri = ((IHasResourceUri)lazyCritter).Uri;
            Assert.That(afterLoadUri, Is.Not.StringContaining(predicate));
            Console.WriteLine(afterLoadUri);
            Assert.That(name, Is.EqualTo(expected.Name));
        }


        [Test]
        public void QueryCritter_GetMaxId_ReturnsMaxId()
        {
            var expected = Repository.List<Critter>().Max(x => x.Id);
            Assert.That(Client.Critters.Query().Max(x => x.Id), Is.EqualTo(expected));
            Assert.That(Client.Critters.Query().Select(x => x.Id).Max(), Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetMinId_ReturnsMinId()
        {
            var expected = Repository.List<Critter>().Min(x => x.Id);

            Assert.That(Client.Critters.Query().Min(x => x.Id), Is.EqualTo(expected));
            Assert.That(Client.Critters.Query().Select(x => x.Id).Min(), Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetSumOfDecimal()
        {
            var expected = CritterEntities.Sum(x => (decimal)x.Id);
            var actual = Client.Query<ICritter>().Sum(x => (decimal)x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetSumOfDouble()
        {
            var expected = CritterEntities.Sum(x => (double)x.Id);
            var actual = Client.Query<ICritter>().Sum(x => (double)x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetSumOfInt()
        {
            var expected = CritterEntities.Sum(x => x.Name.Length);
            var actual = Client.Query<ICritter>().Sum(x => x.Name.Length);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetSumOfNullableDecimal()
        {
            var expected = CritterEntities.Sum(x => (decimal?)x.Id);
            var actual = Client.Query<ICritter>().Sum(x => (decimal?)x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetSumOfNullableDouble()
        {
            var expected = CritterEntities.Sum(x => (double?)x.Id);
            var actual = Client.Query<ICritter>().Sum(x => (double?)x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GetSumOfNullableInt()
        {
            var expected = CritterEntities.Sum(x => (int?)x.Id);
            var actual = Client.Query<ICritter>().Sum(x => (int?)x.Id);
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_GroupByMultiplePropertiesThenSelectAnonymousClass_ReturnsCorrectValues()
        {
            // Just take some random critter
            // Search by its name
            var expected =
                CritterEntities
                    .Where(x => x.Id % 2 == 0)
                    .GroupBy(x => new { x.Farm.Id, x.Weapons.Count })
                    .Select(
                        x => new
                        {
                            x.Key,
                            Count = x.Count(),
                            WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                        })
                    .Take(1)
                    .ToList();

            var actual =
                Client.Query<ICritter>()
                      .Where(x => x.Id % 2 == 0)
                      .GroupBy(x => new { x.Farm.Id, x.Weapons.Count })
                      .Select(
                          x => new
                          {
                              x.Key,
                              Count = x.Count(),
                              WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                          })
                      .Take(1)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_GroupByReferencedResource_ReturnsCorrectValues()
        {
            var expected =
                CritterEntities
                    .Where(x => x.Farm != null)
                    .GroupBy(x => x.Farm)
                    .Select(x => new { FarmId = x.Key.Id, CritterCount = x.Count() });
            var actual =
                Client.Critters
                      .Where(x => x.Farm != null)
                      .GroupBy(x => x.Farm)
                      .Select(x => new { FarmId = x.Key.Id, CritterCount = x.Count() })
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
                            x.Key,
                            Count = x.Count(),
                            WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                        })
                    .Take(1)
                    .ToList();

            var actual =
                Client.Query<ICritter>()
                      .Where(x => x.Id % 2 == 0)
                      .GroupBy(x => x.Farm.Id)
                      .Select(
                          x => new
                          {
                              x.Key,
                              Count = x.Count(),
                              WeaponSum = x.Sum(y => y.Weapons.Sum(z => z.Strength))
                          })
                      .Take(1)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
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
                Client.Query<ICritter>()
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
        public void QueryCritter_LazyDisabled_ThrowsLazyLoadException()
        {
            Client.Settings.LazyMode = LazyMode.Disabled;
            var result = Client.Critters.Query().Take(1).First();

            var exception = Assert.Throws<LazyLoadingDisabledException>(() =>
            {
                var hatType = result.Hat.HatType;
            });

            Console.WriteLine(exception);

            Assert.That(exception.Message, Is.StringContaining("hat"));
        }


        [Test]
        public void QueryCritter_OfType()
        {
            var critters =
                Client.Query<ICritter>()
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
                    .Select(x => new { NameLength = x.Name.Length })
                    .OrderBy(x => x.NameLength)
                    .Take(10)
                    .ToList();
            var actual =
                Client.Critters.Query()
                      .Select(x => new { NameLength = x.Name.Length })
                      .OrderBy(x => x.NameLength)
                      .Take(10)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        [Category("TODO")]
        public void QueryCritter_OrderByBeforeGroupBy_ThrowsNotSupportedException()
        {
            TestDelegate throwing = () => Client.Critters
                                                .Query()
                                                .OrderBy(x => x.Name)
                                                .GroupBy(x => x.Name)
                                                .Select(x => x.Key)
                                                .ToList();

            var exception = Assert.Throws<NotSupportedException>(throwing);
            Assert.That(exception.Message, Is.StringContaining("OrderBy"));
        }


        [Test]
        [Category("TODO")]
        public void QueryCritter_OrderByBeforeGroupByWithoutSelect_ThrowsNotSupportedException()
        {
            TestDelegate throwing = () => Client.Critters
                                                .Query()
                                                .OrderBy(x => x.Name)
                                                .GroupBy(x => x.Name)
                                                .ToList();

            var exception = Assert.Throws<NotSupportedException>(throwing);
            Assert.That(exception.Message, Is.StringContaining("OrderBy"));
        }


        [Test]
        [Category("TODO")]
        public void QueryCritter_OrderByWithCustomComparer_ThrowsNotSupportedException()
        {
            TestDelegate throwing = () => Client.Critters
                                                .Query()
                                                .OrderBy(x => x.Name, new CustomComparer())
                                                .ToList();

            var exception = Assert.Throws<NotSupportedException>(throwing);
            Assert.That(exception.Message, Is.StringContaining("Comparer"));
        }


        [Test]
        public void QueryCritter_QueryingPropertyOfBaseClass_ReflectedTypeOfPropertyInPomonaQueryIsCorrect()
        {
            // Fix: We don't want the parsed expression trees to give us members with "ReflectedType" set to inherited type, but same as DeclaringType.

            // Result of below query not important..
            Client.Critters.Query().Where(x => x.Id == 666).ToList();

            var query = Repository.QueryLog.Last();
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
            var expected = CritterEntities.Select(x => (decimal)x.Id).Sum();
            var actual = Client.Query<ICritter>().Select(x => (decimal)x.Id).Sum();
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_SelectDoubleThenSum()
        {
            var expected = CritterEntities.Select(x => (double)x.Id).Sum();
            var actual = Client.Query<ICritter>().Select(x => (double)x.Id).Sum();
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_SelectIntThenSum()
        {
            var expected = CritterEntities.Select(x => x.Name.Length).Sum();
            var actual = Client.Query<ICritter>().Select(x => x.Name.Length).Sum();
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_SelectThenWhereThenSelect_ReturnsCorrectValues()
        {
            var expected = CritterEntities
                .Select(x => new { c = x, isHeavyArmed = x.Weapons.Count > 2, farmName = x.Farm.Name })
                .Where(x => x.isHeavyArmed)
                .Select(x => new { critterName = x.c.Name, x.farmName })
                .Take(5)
                .ToList();

            var actual = Client.Query<ICritter>()
                               .Select(x => new { c = x, isHeavyArmed = x.Weapons.Count > 2, farmName = x.Farm.Name })
                               .Where(x => x.isHeavyArmed)
                               .Select(x => new { critterName = x.c.Name, x.farmName })
                               .Take(5)
                               .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_SelectToStringObjectDictionary_ReturnsCorrectValues()
        {
            var expected = CritterEntities
                .Select(
                    x =>
                        new Dictionary<string, object>
                        {
                            { "critterId", x.Id },
                            { "critterName", x.Name }
                        })
                .First();
            var actual =
                Client.Query<ICritter>()
                      .Select(
                          x =>
                              new Dictionary<string, object>
                              {
                                  { "critterId", x.Id },
                                  { "critterName", x.Name }
                              })
                      .First();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_SelectToTuple_ReturnsCorrectValues()
        {
            var expected = CritterEntities
                .Select(x => new Tuple<int, string>(x.Id, x.Name))
                .ToList();
            var actual =
                Client.Query<ICritter>()
                      .Select(x => new Tuple<int, string>(x.Id, x.Name))
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_SelectWriteOnlyProperty_ThrowsBadRequestException()
        {
            var critter = Repository.CreateRandomCritter();
            critter.Password = "HUSH";
            Assert.Throws<BadRequestException>(() => Client.Critters.Query().Select(x => x.Password).ToList());
        }


        [Test]
        public void QueryCritter_ToUri_ReturnsUriForQuery()
        {
            var uri = Client.Query<ICritter>().Where(x => x.Name == "holahola").Take(10).ToUri();
            Assert.That(uri.PathAndQuery, Is.EqualTo("/critters?$filter=name+eq+'holahola'&$top=10"));
        }


        [Test]
        public void QueryCritter_WhereExpressionCapturingPropertyFromClass_EvaluatesToConstantCorrectly()
        {
            var critter = Repository.CreateRandomCritter();
            TestIntProperty = critter.Id;
            var result = Client.Critters.Query(x => x.Id == TestIntProperty).FirstOrDefault();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(TestIntProperty));
        }


        [Test]
        public void QueryCritter_WhereFirst_ReturnsCorrectCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(1).Take(1).First();
            // Search by its name
            var critterResource =
                Client.Query<ICritter>().First(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
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
                Client.Query<ICritter>()
                      .Where(x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                      .Expand(x => x.Weapons)
                      .Take(5)
                      .ToList();

            Assert.That(actual.Select(x => x.Id), Is.EquivalentTo(expected.Select(x => x.Id)));
        }


        [Ignore("For performance testing!")]
        [Test]
        public void QueryCritter_WhereFirstOrDefaultFromWeapons_ReturnsCorrectValues_ManyTimes()
        {
            RequestTraceEnabled = false;
            var expected =
                CritterEntities.Where(
                    x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                               .Take(5)
                               .ToList();

            for (var i = 0; i < 100; i++)
            {
                var actual =
                    Client.Query<ICritter>()
                          .Where(x => x.Weapons.FirstOrDefault() != null && x.Weapons.FirstOrDefault().Strength > 0.5)
                          .Expand(x => x.Weapons)
                          .Take(5)
                          .ToList();
                Assert.That(actual.Select(x => x.Id), Is.EquivalentTo(expected.Select(x => x.Id)));
            }
        }


        [Category("TODO")]
        [Test(Description = "Have to find out how references should be serialized in queries.")]
        public void QueryCritter_WhereReferencedResourceEqualsALoadedResource_ReturnsCorrectValues()
        {
            var farmEntity = CritterEntities.Select(x => x.Farm).First(x => x != null);
            var farmResource = Client.Farms.Get(farmEntity.Id);

            var expected =
                CritterEntities
                    .Where(x => x.Farm == farmEntity)
                    .Select(x => x.Id)
                    .ToList();
            var actual =
                Client.Critters
                      .Where(x => x.Farm == farmResource)
                      .Select(x => x.Id)
                      .ToList();

            Assert.That(actual.SequenceEqual(expected));
        }


        [Test]
        public void QueryCritter_WhereSingle_ReturnsCorrectCritter()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(1).Take(1).First();
            // Search by its name
            var critterResource =
                Client.Query<ICritter>().Single(x => x.Name == critter.Name && x.Guid == critter.Guid);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WhereSingle_ThrowsExceptionOnMultipleMatches()
        {
            Assert.Throws<InvalidOperationException>(() => Client.Query<ICritter>().Single());
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
                Client.Query<ICritter>()
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
                Client.Query<ICritter>().OrderBy(x => x.Name).Select(x => x.Name).Take(10000).ToList().ToList();
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void QueryCritter_WithAttributeEquals_ReturnsCorrectCritter()
        {
            var critter =
                Client.Critters.Query().Where(
                    x => x.SimpleAttributes.Any(y => y.Key == "AttrKey" && y.Value == "dde")).ToList();
        }


        [Test]
        public void QueryCritter_WithExpandedProperty_HasPropertyExpanded()
        {
            var result = Client.Critters.Query().Expand(x => x.Hat).Take(1).First();
            Assert.That(result.Hat, Is.TypeOf<HatResource>());
        }


        [Test]
        public void QueryCritter_WithExpandedPropertyOfAnonymousClass_HasPropertyExpanded()
        {
            var result =
                Client.Critters.Query()
                      .Select(x => new { TheHat = x.Hat, x.Name })
                      .OrderBy(x => x.Name)
                      .Expand(x => x.TheHat)
                      .Take(1)
                      .First();
            Assert.That(result.TheHat, Is.TypeOf<HatResource>());
        }


        [Test]
        public void QueryCritter_WithHandledGeneratedProperty_UsesPropertyFormula()
        {
            Client.Critters.Query(x => x.HandledGeneratedProperty == 3).ToList();
            var query = Repository.QueryLog.Last();
            Expression<Func<Critter, bool>> expectedFilter = _this => (_this.Id % 6) == 3;
            query.FilterExpression.AssertEquals(expectedFilter);
        }


        [Test]
        public void QueryCritter_WithIncludeTotalCount_IncludesTotalCount()
        {
            var expectedTotalCount = CritterEntities.Count(x => x.Id % 3 == 0);
            var results = Client
                .Critters
                .Query()
                .Where(x => x.Id % 3 == 0)
                .IncludeTotalCount()
                .ToQueryResult();
            Assert.That(results.TotalCount, Is.EqualTo(expectedTotalCount));
        }


        [Test]
        public void QueryCritter_WithNonMatchingFirst_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => Client.Query<ICritter>().First(x => false));
            Assert.That(ex.Message, Is.EqualTo("Sequence contains no matching element"));
        }


        [Test]
        public void QueryCritter_WithOptionsThatModifiesRequestUrl_ModificationAreAppliedToRequest()
        {
            // Just take some random critter
            var critter = CritterEntities.Skip(1).Take(1).First();
            // Create a query searching for a totally different url, but replace the url by using ModifyRequest
            var critterResource =
                Client.Query<ICritter>().WithOptions(
                    x => x.RewriteUrl(y => y.Replace("1234567", critter.Id.ToString()))).First(
                        x => x.Id == 1234567);
            // Check that the replacement in url worked as expected:
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }


        [Test]
        public void QueryCritter_WithPropertyNotAllowedInExpression_ThrowsBadRequestException_HavingUsefulErrorMessage()
        {
            var ex =
                Assert.Throws<BadRequestException>(
                    () => Client.Critters.First(x => x.Id > 4 && x.IsNotAllowedInFilters == "haha"));
            Assert.That(ex.Message, Is.StringContaining("isNotAllowedInFilters"));
        }


        [Test]
        public void QueryCritter_WithPropertyOfListItemsExpanded_HasPropertiesExpanded()
        {
            var result = Client.Critters.Query().Expand(x => x.Weapons.Expand(y => y.Model)).Take(1).First();
            Assert.That(result.Weapons, Is.TypeOf<List<IWeapon>>());
            Assert.That(result.Weapons.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Weapons.All(x => x is WeaponResource));
            Assert.That(result.Weapons.All(x => x.Model is WeaponModelResource));
        }


        [Test(Description =
            "We need a scenario that makes this throw so we can assert that the exception contains the information we need to debug the missing Expand()."
            )]
        [Category("TODO")]
        public void QueryCritter_WithPropertyOfListItemsUnexpanded_ThrowsResourceNotFoundException()
        {
            var result = Client.Critters.Query().Take(1).First();

            var exception =
                Assert.Throws<Common.Web.ResourceNotFoundException>(() => result.Weapons.Select(x => x.Id).ToArray());
            Assert.That(exception.Uri, Is.StringContaining(""));
        }


        [Test]
        public void QueryCritter_WithUnhandledGeneratedProperty_ThrowsExceptionUsingCritterDataSource()
        {
            Assert.That(() => Client.Critters.Query(x => x.UnhandledGeneratedProperty == "nono").ToList(),
                        Throws.Exception);
        }


        [Test]
        public void QueryCritter_WriteOnlyProperty_IsNotReturned()
        {
            var critter = Repository.CreateRandomCritter();
            critter.Password = "HUSH";
            var jobject = Client.Critters.Query().Where(x => x.Id == critter.Id).ToJson();
            var items = jobject.AssertHasPropertyWithArray("items");
            Assert.That(items.Count, Is.EqualTo(1));
            var critterObject = items[0] as JObject;
            critterObject.AssertDoesNotHaveProperty("password");

            var critters = Client.Critters.Query().Where(x => x.Id == critter.Id).ToList();
            Assert.That(critters.Count, Is.EqualTo(1));
        }


        [Test]
        public void QueryEtaggedEntity_HavingZeroResults_ProjectedByFirstOrDefault_ReturnsNull()
        {
            Assert.IsNull(Client.EtaggedEntities.Query(x => false).FirstOrDefault());
        }


        [Test]
        public void QueryHasStringToObjectDictionary_ReturnsCorrectValues()
        {
            for (var i = 1; i <= 8; i++)
                Repository.Save(new StringToObjectDictionaryContainer { Map = { { "square", i * i } } });

            // Should get 36, 49 and 64
            var results = Client.Query<IStringToObjectDictionaryContainer>()
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
                Client.Query<IMusicalCritter>().First(
                    x => x.Name == critter.Name && x.Guid == critter.Guid && x.BandName == critter.BandName);
            Assert.That(critterResource.Id, Is.EqualTo(critter.Id));
        }

        #region Setup/Teardown

        [TearDown]
        public void TearDown()
        {
            // Reset lazymode back to enabled. @asbjornu
            Client.Settings.LazyMode = LazyMode.Enabled;
        }

        #endregion

        private class CustomComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return x.CompareTo(y);
            }
        }
    }
}