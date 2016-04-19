#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Common.Internals;

namespace Pomona.UnitTests.Internals
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void Pad_2To7_Return7()
        {
            var enumerable = new[] { "0", "1" };
            var result = enumerable.Pad(7, "0").ToArray();

            Assert.That(result, Has.Length.EqualTo(7));
        }


        [Test]
        public void Pad_EmptyTo4_Return4()
        {
            var enumerable = Enumerable.Empty<string>();
            var result = enumerable.Pad(4, "0").ToArray();

            Assert.That(result, Has.Length.EqualTo(4));
        }
    }
}