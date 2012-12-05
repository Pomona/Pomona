using System;
using System.Linq;

using Critters.Client;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.TestHelpers;

namespace CritterClientTests.Linq
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
        public void QueryWithMultipleSkips_ThrowsSomeException()
        {
            Assert.Fail("Test not written, behaviour uncertain (what kind of exception, and when?)");
        }


        [Test]
        public void QueryWithMultipleTakes_ThrowsSomeException()
        {
            Assert.Fail("Test not written, behaviour uncertain (what kind of exception, and when?)");
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
        public void QueryWithSkipBeforeOrderBy_ThrowsSomeException()
        {
            Assert.Fail("Test not written, behaviour uncertain (what kind of exception, and when?)");
        }


        [Test]
        public void QueryWithTakeBeforeOrderBy_ThrowsSomeException()
        {
            Assert.Fail("Test not written, behaviour uncertain (what kind of exception, and when?)");
        }
    }
}