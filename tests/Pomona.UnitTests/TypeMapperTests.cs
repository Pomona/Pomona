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

using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class TypeMapperTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new TypeMapper(new CritterPomonaConfiguration());
        }

        #endregion

        private TypeMapper typeMapper;


        [Test]
        public void ChangePluralNameWorksCorrectly()
        {
            Assert.That(((TransformedType)this.typeMapper.GetClassMapping<RenamedThing>()).PluralName,
                Is.EqualTo("ThingsWithNewName"));
        }


        [Test]
        public void ChangeTypeNameWorksCorrectly()
        {
            Assert.That(this.typeMapper.GetClassMapping<RenamedThing>().Name, Is.EqualTo("GotNewName"));
        }


        [Test]
        public void DoesNotDuplicatePropertiesWhenDerivedFromHiddenBaseClassInMiddle()
        {
            var tt = this.typeMapper.GetClassMapping<InheritsFromHiddenBase>();
            Assert.That(tt.Properties.Count(x => x.Name == "Id"), Is.EqualTo(1));
            var idProp = tt.Properties.First(x => x.Name == "Id");
            Assert.That(idProp.DeclaringType, Is.EqualTo(this.typeMapper.GetClassMapping<EntityBase>()));
        }


        [Test]
        public void Property_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            var tt =
                (PropertyMapping)
                    this.typeMapper.GetClassMapping<Critter>().Properties.First(
                        x => x.Name == "PublicAndReadOnlyThroughApi");
            Assert.That(!tt.AccessMode.HasFlag(HttpMethod.Post));
        }


        [Test]
        public void StaticProperty_IsExcludedByDefault()
        {
            Assert.That(
                this.typeMapper.FromType(typeof(Critter)).Properties.Where(x => x.Name == "TheIgnoredStaticProperty"),
                Is.Empty);
        }
    }
}