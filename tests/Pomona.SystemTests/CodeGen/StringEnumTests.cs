#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

