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

using NUnit.Framework;

using Pomona.CodeGen;

namespace Pomona.UnitTests.CodeGen
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public void PadTo_1_1_1_1_Returns_1_1_1_1()
        {
            const string s = "1.1.1.1";
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("1.1.1.1"));
        }


        [Test]
        public void PadTo_1_1_1_Returns_1_1_1_0()
        {
            const string s = "1.1.1";
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("1.1.1.0"));
        }


        [Test]
        public void PadTo_1_1_Returns_1_1_0_0()
        {
            const string s = "1.1";
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("1.1.0.0"));
        }


        [Test]
        public void PadTo_1_Returns_1_0_0_0()
        {
            const string s = "1";
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("1.0.0.0"));
        }


        [Test]
        public void PadTo_EmptyString_Returns_0_0_0_0()
        {
            string s = String.Empty;
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("0.0.0.0"));
        }


        [Test]
        public void PadTo_Null_Returns_0_0_0_0()
        {
            string s = null;
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("0.0.0.0"));
        }


        [Test]
        public void PadTo_WhiteSpace_Returns_0_0_0_0()
        {
            const string s = "    ";
            var result = s.PadTo(4);

            Assert.That(result, Is.EqualTo("0.0.0.0"));
        }
    }
}