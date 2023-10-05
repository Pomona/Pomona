#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Common.Linq.Queries.Rewriters;

namespace Pomona.UnitTests.Linq.Queries.Rewriters
{
    [TestFixture]
    public class MergeWhereRewriterTests : RewriterTestBase<MergeWhereRewriter>
    {
        [Test]
        public void Visit_Where_Returns_Unmodified_Where()
        {
            AssertDoesNotRewrite(() => this.Animals.Where(x => x.Id == 2).Select(x => x).Where(x => x.Id == 55));
        }


        [Test]
        public void Visit_Where_Where_Returns_Merged_Where()
        {
            AssertRewrite(() => this.Animals.Where(x => x.Id > 21).Where(x => x.Id < 549),
                          () => this.Animals.Where(x => x.Id > 21 && x.Id < 549));
        }
    }
}

