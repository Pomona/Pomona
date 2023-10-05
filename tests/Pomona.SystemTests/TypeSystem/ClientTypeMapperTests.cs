#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.SystemTests.TypeSystem
{
    [TestFixture]
    public class ClientTypeMapperTests
    {
        private ClientTypeMapper clientTypeMapper;


        [Test]
        public void CritterType_ReturnsCorrectPluralName()
        {
            var critterType = (ResourceType)this.clientTypeMapper.FromType(typeof(ICritter));
            Assert.That(critterType.PluralName, Is.EqualTo("Critters"));
        }


        [Test]
        public void GetMappedTypeFromProxyType_ReturnsCorrectResourceType()
        {
            Assert.That(this.clientTypeMapper.FromType(typeof(CritterLazyProxy)).Type, Is.EqualTo(typeof(ICritter)));
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.clientTypeMapper = new ClientTypeMapper(typeof(ICritter).WrapAsEnumerable());
        }

        #endregion
    }
}

