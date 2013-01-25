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
        private RestQueryableTreeParser Parse<T>(Func<IQueryable<T>, IQueryable> bah)
        {
            var provider = new RestQueryProvider(Substitute.For<IPomonaClient>());
            var queryable = new RestQuery<T>(provider);
            var q = bah(queryable);
            var treeParser = new RestQueryableTreeParser();
            treeParser.Visit(q.Expression);
            return treeParser;
        }


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
    }
}