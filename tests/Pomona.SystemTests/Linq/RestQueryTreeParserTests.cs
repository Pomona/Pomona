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
            var queryable = new RestQueryProvider(Substitute.For<IPomonaClient>()).CreateQuery<T>("http://example/path");
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
    }
}