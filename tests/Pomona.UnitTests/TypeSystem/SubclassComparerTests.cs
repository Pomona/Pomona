#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Pomona.TypeSystem;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class SubclassComparerTests
    {
        private static IEnumerable<Type> ShuffledTypes
        {
            get { return new[] { typeof(B), typeof(A), typeof(D), typeof(C) }; }
        }


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