#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using NUnit.Framework;

using Pomona.IO;

namespace Pomona.UnitTests.IO
{
    [TestFixture]
    public class PathHelperTests
    {
        [Test]
        [TestCase("/", "/", true)]
        [TestCase("/jalla/maka/paka", "/**", true)]
        [TestCase("/jalla", "/**", true)]
        [TestCase("/jalla", "/*", true)]
        [TestCase("/jalla/klsjkls", "/*", false)]
        [TestCase("/jalla", "/**/*.nope", false)]
        [TestCase("/pre/jalla.book", "/**/*.book", true)]
        [TestCase("/pre/jalla.book", "/*.book", false)]
        [TestCase("/pre/aisudiso/lksajdklj/kdljflkdjkjkl/monkey/*.book", "/pre/**/monkey/*.book", true)]
        [TestCase("pre/aisudiso/lksajdklj/kdljflkdjkjkl/monkey/*.book", "pre/**/monkey/*.book", true)]
        [TestCase("pre/++34+/lksajdklj/kdljflkdjkjkl/mon+key/*.book", "pre/**/mon+key/*.book", true)]
        public void MatchUrlPathSpec_MatchesPath(string input, string pattern, bool isMatch)
        {
            Assert.That(PathHelper.MatchUrlPathSpec(input, pattern), Is.EqualTo(isMatch));
        }
    }
}

