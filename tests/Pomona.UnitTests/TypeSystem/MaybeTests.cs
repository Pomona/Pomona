#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using NUnit.Framework;

using Pomona.Common.TypeSystem;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class MaybeTests
    {
        public class InheritedClass : SuperClass
        {
        }

        public class SuperClass
        {
        }


        [Test]
        public void Maybe_FromEmptyNullable_HasNoValue()
        {
            Assert.That(default(DateTime?).Maybe().HasValue, Is.False);
        }


        [Test]
        public void Maybe_FromNull_HasNoValue()
        {
            Assert.That(((SuperClass)null).Maybe().HasValue, Is.False);
        }


        [Test]
        public void OrDefault_FromMaybeWithoutValue_ReturnsDefaultValue()
        {
            Assert.That(Maybe<string>.Empty.OrDefault(() => "ImDefault"), Is.EqualTo("ImDefault"));
            Assert.That(Maybe<string>.Empty.OrDefault(), Is.EqualTo(null));
        }


        [Test]
        public void Select_NonNullValue_HasValue()
        {
            Assert.That("blah".Maybe().Select(x => x).HasValue, Is.True);
        }


        [Test]
        public void Select_NullValue_HasNoValue()
        {
            Assert.That("blah".Maybe().Select(x => (string)null).HasValue, Is.False);
        }
    }
}