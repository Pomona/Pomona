#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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