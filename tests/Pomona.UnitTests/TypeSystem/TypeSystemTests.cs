#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class TypeSystemTests
    {
        [Test]
        public void FromEnumType_ReturnsEnumTypeSpec()
        {
            var mapper = new TypeMapper(new CritterPomonaConfiguration());
            Assert.That(mapper.FromType<CustomEnum>(), Is.InstanceOf<EnumTypeSpec>());
        }


        [Test]
        public void TransformedType_RequiredProperties_ReturnsRequiredProperties()
        {
            var mapper = new TypeMapper(new CritterPomonaConfiguration());
            var type = mapper.FromType(typeof(Planet));
            var requiredProperties = type.RequiredProperties.ToList();
            Assert.That(requiredProperties.All(x => x.IsRequiredForConstructor), Is.True);
        }
    }
}
