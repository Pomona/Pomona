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

using System.Linq;

using NUnit.Framework;

using ClientEnum = Critters.Client.CustomStringEnum;

namespace Pomona.SystemTests.CodeGen
{
    [TestFixture]
    public class StringEnumTests
    {
        [Test]
        public void AllValues_Has_All_Values()
        {
            Assert.That(ClientEnum.AllValues.Select(x => x.Value).ToList(),
                        Is.EquivalentTo(new string[] { "Mouse", "Rat", "Cat" }));
        }


        [Test]
        public void Cast_From_String_Works_As_It_Should()
        {
            Assert.That((ClientEnum)"Rat", Is.EqualTo(ClientEnum.Rat));
        }


        [Test]
        public void Cast_To_String_Works_As_It_Should()
        {
            Assert.That((string)ClientEnum.Mouse, Is.EqualTo("Mouse"));
        }


        [Test]
        public void Comparison_Between_Enum_Members_Is_Case_Insensitive()
        {
            Assert.That((ClientEnum)"rAt", Is.EqualTo(ClientEnum.Rat));
        }


        [Test]
        public void Default_Value_Is_Same_As_Source_Enum_Value()
        {
            ClientEnum cse = default(ClientEnum);
            Assert.That(cse == ClientEnum.Cat);
        }


        [Test]
        public void Member_In_Values_Has_KnownValue_Set_To_False()
        {
            Assert.That(((ClientEnum)"rat").IsKnown, Is.True);
            Assert.That(ClientEnum.Rat.IsKnown, Is.True);
        }


        [Test]
        public void Member_Not_In_Values_Has_KnownValue_Set_To_False()
        {
            Assert.That(((ClientEnum)"alien").IsKnown, Is.False);
        }


        [Test]
        public void Parsed_Value_Keeps_Correct_Casing()
        {
            var parsed = (ClientEnum)"rAt";
            Assert.That(parsed.ToString(), Is.EqualTo("Rat"));
        }
    }
}