#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;

using Critters.Client;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.TestHelpers;

namespace Pomona.SystemTests.Linq
{
    [TestFixture]
    public class RestQueryTreeParserTests
    {
        [Test]
        public void QueryWithMultipleSkips_ThrowsNotSupportedException()
        {
            Assert.That(() => Parse<ICritter>(x => x.Skip(10).Skip(10)), Throws.TypeOf<NotSupportedException>());
        }


        [Test]
        public void QueryWithMultipleTakes_ThrowsNotSupportedException()
        {
            Assert.That(() => Parse<ICritter>(x => x.Take(10).Take(10)), Throws.TypeOf<NotSupportedException>());
        }


        [Test]
        public void QueryWithMultipleWheres_PredicateIsCorrectlyCombined()
        {
            var parser =
                Parse<ICritter>(x => x.Where(y => y.Id == 1).Where(y => y.Hat.Id == 2).Where(y => y.Farm.Id == 3));
            parser.WherePredicate.AssertEquals<Func<ICritter, bool>>(y => y.Id == 1 && y.Hat.Id == 2 && y.Farm.Id == 3);
        }


        [Test]
        public void QueryWithRequestModifications_RequestModificationDelegateIsAddedToList()
        {
            var parser =
                Parse<ICritter>(x => x.WithOptions(y => y.ModifyRequest(z => z.RequestUri = new Uri("http://nanah"))));
            Assert.That(parser.RequestOptionActions, Has.Count.EqualTo(1));
        }


        [Test]
        public void QueryWithSelectAfterSelect_SelectIsCorrectlyMerged()
        {
            var parser =
                Parse<ICritter>(
                    x => x.Where(y => y.Id == 1).Select(y => new { f = y.Farm, c = y }).Select(y => y.f.Name));
            parser.SelectExpression.AssertEquals<Func<ICritter, string>>(y => y.Farm.Name);
        }


        [Test]
        public void QueryWithSingleWhere_PredicateIsCorrect()
        {
            var parser = Parse<ICritter>(x => x.Where(y => y.Id == 5));
            parser.WherePredicate.AssertEquals<Func<ICritter, bool>>(y => y.Id == 5);
        }


        [Test]
        public void QueryWithSkipBeforeOrderBy_ThrowsNotSupportedException()
        {
            Assert.That(() => Parse<ICritter>(x => x.Skip(10).OrderBy(y => y.CreatedOn)),
                        Throws.TypeOf<NotSupportedException>());
        }


        [Test]
        public void QueryWithTakeBeforeOrderBy_ThrowsNotSupportedException()
        {
            Assert.That(() => Parse<ICritter>(x => x.Take(10).OrderBy(y => y.CreatedOn)),
                        Throws.TypeOf<NotSupportedException>());
        }


        [Test]
        public void QueryWithWhereAfterSelect_PredicateIsCorrectlyMerged()
        {
            var parser =
                Parse<ICritter>(
                    x =>
                        x.Where(y => y.Id == 1).Select(y => new { CritterName = y.Name.ToUpper() }).Where(
                            y => y.CritterName.StartsWith("BAH")));
            parser.WherePredicate.AssertEquals<Func<ICritter, bool>>(
                y => y.Id == 1 && y.Name.ToUpper().StartsWith("BAH"));
        }


        private RestQueryableTreeParser Parse<T>(Func<IQueryable<T>, IQueryable> bah)
        {
            var queryable = new RestQueryProvider(Substitute.For<IPomonaClient>()).CreateQuery<T>("http://example/path");
            var q = bah(queryable);
            var treeParser = new RestQueryableTreeParser();
            treeParser.Visit(q.Expression);
            return treeParser;
        }
    }
}
