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
using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.ModelProxies;
using Pomona.Example.Models;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class TypeMapperTests
    {
        private TypeMapper typeMapper;

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new TypeMapper(new CritterPomonaConfiguration());
        }

        #endregion

        [Test]
        public void AnonymousCompilerGeneratedType_IsMappedAsValueObject()
        {
            var anonObject = new { Foo = "hoohoo" };
            var type = this.typeMapper.FromType(anonObject.GetType());
            Assert.That(type, Is.TypeOf<ComplexType>());
            Assert.That(((ComplexType)type).MappedAsValueObject, Is.True);
        }


        [Test]
        public void ChangePluralNameWorksCorrectly()
        {
            Assert.That(((ComplexType)this.typeMapper.FromType<RenamedThing>()).PluralName,
                        Is.EqualTo("ThingsWithNewName"));
        }


        [Test]
        public void ChangeTypeNameWorksCorrectly()
        {
            Assert.That(this.typeMapper.FromType<RenamedThing>().Name, Is.EqualTo("GotNewName"));
        }


        [Test]
        public void DoesNotDuplicatePropertiesWhenDerivedFromHiddenBaseClassInMiddle()
        {
            var tt = this.typeMapper.FromType<InheritsFromHiddenBase>();
            Assert.That(tt.Properties.Count(x => x.Name == "Id"), Is.EqualTo(1));
            var idProp = tt.Properties.First(x => x.Name == "Id");
            Assert.That(idProp.DeclaringType, Is.EqualTo(this.typeMapper.FromType<EntityBase>()));
        }


        [Test]
        public void GetClassMapping_ByInvalidName_ThrowsUnknownTypeException()
        {
            Assert.Throws<UnknownTypeException>(() => typeMapper.FromType("WTF"));
        }


        [Test]
        public void GetClassMapping_ByValidName_ReturnsCorrectType()
        {
            var critterType = typeMapper.FromType("Critter");
            Assert.IsNotNull(critterType);
            Assert.That(critterType.Type, Is.EqualTo(typeof(Critter)));
        }


        [Test]
        public void GetTypeForProxyTypeInheritedFromMappedType_ReturnsMappedBaseType()
        {
            Assert.That(this.typeMapper.FromType(typeof(BearProxy)).Type, Is.EqualTo(typeof(Bear)));
        }


        [Test]
        public void Property_removed_from_filter_in_GetAllPropertiesOfType_is_not_mapped()
        {
            var type = this.typeMapper.FromType<Critter>();
            Assert.That(type.Properties.Where(x => x.Name == "PropertyExcludedByGetAllPropertiesOfType"), Is.Empty);
        }


        [Test]
        public void InterfaceIGrouping_IsMappedAsValueObject()
        {
            var type = this.typeMapper.FromType(typeof(IGrouping<string, string>));
            Assert.That(type, Is.TypeOf<ComplexType>());
            Assert.That(((ComplexType)type).MappedAsValueObject, Is.True);
        }


        [Test]
        public void PropertyOfExposedInterfaceFromNonExposedBaseInterfaceGotCorrectDeclaringType()
        {
            var tt = this.typeMapper.FromType<IExposedInterface>();
            var prop = tt.Properties.SingleOrDefault(x => x.Name == "PropertyFromInheritedInterface");
            Assert.That(prop, Is.Not.Null, "Unable to find property PropertyFromInheritedInterface");
            Assert.That(prop.DeclaringType, Is.EqualTo(tt));
        }


        [Test]
        public void Property_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            var tt =
                (ComplexProperty)
                    this.typeMapper.FromType<Critter>().Properties.First(
                        x => x.Name == "PublicAndReadOnlyThroughApi");
            Assert.That(!tt.AccessMode.HasFlag(HttpMethod.Post));
        }


        [Test]
        public void Property_WithFluentlyAddedAttribute_GotAttributeAddedToPropertySpec()
        {
            var tt = this.typeMapper.FromType<Critter>();
            var prop = tt.Properties.SingleOrDefault(x => x.Name == "PropertyWithAttributeAddedFluently");
            Assert.That(prop, Is.Not.Null, "Unable to find property PropertyWithAttributeAddedFluently");
            Assert.That(prop.DeclaredAttributes.OfType<ObsoleteAttribute>().Any(), Is.True);
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