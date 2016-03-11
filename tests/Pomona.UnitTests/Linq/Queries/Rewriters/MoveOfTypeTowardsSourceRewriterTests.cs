#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Common.Linq.Queries.Rewriters;

namespace Pomona.UnitTests.Linq.Queries.Rewriters
{
    [TestFixture]
    public class MoveOfTypeTowardsSourceRewriterTests : RewriterTestBase<MoveOfTypeTowardsSourceRewriter>
    {
        [Test]
        public void Visit_Redundant_OfType_Casting_To_Same_Type_Removes_OfType_And_Returns_Source()
        {
            AssertRewrite(() => this.Animals.OfType<Animal>(),
                          () => this.Animals);
        }


        [Test]
        public void Visit_Where_OfType_When_Casting_To_Inherited_Type_Returns_OfType_Where()
        {
            AssertRewrite(() => this.Animals.Where(x => x.Id == 33).OfType<Dog>(),
                          () => this.Animals.OfType<Dog>().Where(x => x.Id == 33));
        }


        [Test]
        public void Visit_Where_OfType_When_Casting_To_Non_Inherited_Type_Returns_Umodified_Expression()
        {
            AssertDoesNotRewrite(() => this.Animals.Where(x => x.Id == 33).OfType<object>());
        }
    }
}