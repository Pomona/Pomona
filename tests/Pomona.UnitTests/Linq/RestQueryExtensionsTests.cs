#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;

namespace Pomona.UnitTests.Linq
{
    [TestFixture]
    public class RestQueryExtensionsTests
    {
        [Test]
        public void ToQueryResult_OnEnumerableAsQueryable_IsSuccessful()
        {
            var results = new int[] { 1, 2, 3, 4 }.AsQueryable().ToQueryResult();
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(4));
            Assert.That(results, Is.TypeOf<QueryResult<int>>());
        }
    }
}

