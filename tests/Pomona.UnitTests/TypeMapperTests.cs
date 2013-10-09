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

using System.Linq;
using NUnit.Framework;
using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class TypeMapperTests
    {
        [SetUp]
        public void SetUp()
        {
            typeMapper = new TypeMapper(new CritterPomonaConfiguration());
        }

        private TypeMapper typeMapper;

        [Test]
        public void Property_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            var tt = (PropertyMapping)typeMapper.GetClassMapping<Critter>().Properties.First(x => x.Name == "PublicAndReadOnlyThroughApi");
            Assert.That(tt.IsWriteable, Is.False);
            Assert.That(tt.CreateMode, Is.EqualTo(PropertyCreateMode.Excluded));
        }

        [Test]
        public void DoesNotDuplicatePropertiesWhenDerivedFromHiddenBaseClassInMiddle()
        {
            var tt = typeMapper.GetClassMapping<InheritsFromHiddenBase>();
            Assert.That(tt.Properties.Count(x => x.Name == "Id"), Is.EqualTo(1));
            var idProp = tt.Properties.First(x => x.Name == "Id");
            Assert.That(idProp.DeclaringType, Is.EqualTo(typeMapper.GetClassMapping<EntityBase>()));
        }

        [Test]
        public void ConvertToInternalPropertyPath_MapsRenamedPropertyNamesCorrect()
        {
            var transformedType = (TransformedType) typeMapper.GetClassMapping<ThingWithRenamedProperties>();
            var internalPath = typeMapper.ConvertToInternalPropertyPath(
                transformedType,
                "DiscoFunky.BeautifulAndExposed");
            Assert.AreEqual("Junky.ReallyUglyPropertyName", internalPath);

            var internalPathToCollection = typeMapper.ConvertToInternalPropertyPath(
                transformedType,
                "PrettyThings.BeautifulAndExposed");
            Assert.AreEqual("RelatedJunks.ReallyUglyPropertyName", internalPathToCollection);
        }


        [Test]
        public void DoesNotCreateTransformedTypeForExcludedClass()
        {
            Assert.That(
                typeMapper.TransformedTypes.Any(x => x.Name == "ExcludedThing"),
                Is.False,
                "Excluded thing should not have been part of transformed types");
        }
    }
}