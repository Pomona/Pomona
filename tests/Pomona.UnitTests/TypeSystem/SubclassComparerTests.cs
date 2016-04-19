#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Pomona.TypeSystem;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class SubclassComparerTests
    {
        private static IEnumerable<Type> ShuffledTypes => new[] { typeof(B), typeof(A), typeof(D), typeof(C) };


        [Test]
        public void OrderBy_MostSubclassedIsLast()
        {
            Assert.That(ShuffledTypes.OrderBy(x => x, new SubclassComparer()),
                        Is.EqualTo(new[] { typeof(A), typeof(B), typeof(C), typeof(D) }));
        }


        [Test]
        public void OrderByDescending_MostSubclassedIsFirst()
        {
            Assert.That(ShuffledTypes.OrderByDescending(x => x, new SubclassComparer()),
                        Is.EqualTo(new[] { typeof(D), typeof(C), typeof(B), typeof(A) }));
        }


        private class A
        {
        }

        private class B : A
        {
        }

        private class C : B
        {
        }

        private class D : C
        {
        }
    }
}