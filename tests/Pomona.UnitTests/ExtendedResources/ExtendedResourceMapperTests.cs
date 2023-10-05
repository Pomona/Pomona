﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.ExtendedResources;
using Pomona.UnitTests.TestResources;

namespace Pomona.UnitTests.ExtendedResources
{
    [TestFixture]
    public class ExtendedResourceMapperTests
    {
        private ExtendedResourceMapper extendedMapper;
        private ClientTypeMapper typeMapper;


        [Test]
        public void GetExtendedResourceInfo_ForCustomTestResourceWithSelfReference_PropertyHasSameExtendedResourceInfo()
        {
            var resInfo = this.extendedMapper.GetExtendedResourceInfo(typeof(ICustomTestResourceWithSelfReference));
            // When caching of ExtendedResourceInfo works as expected the ExtendedResourceInfo of the property should be
            // exactly the same as the referencing one.
            Assert.That(resInfo.ExtendedProperties.OfType<ExtendedOverlayProperty>().Single().Info, Is.EqualTo(resInfo));
        }


        [Test]
        public void GetExtendedResourceInfo_ForExtendedResourceWithNonNullableProperty_ThrowsExtendedResourceMappingException()
        {
            var ex = Assert.Throws<ExtendedResourceMappingException>(() =>
            {
                this.extendedMapper.GetExtendedResourceInfo(typeof(ICustomTestResourceThatGotNonNullableInteger));
            });
            Assert.That(ex.Message, Is.EqualTo(
                $"Unable to map property {"NotNullable"} of type {typeof(ICustomTestResourceThatGotNonNullableInteger).FullName} to underlying dictionary property {"Attributes"} of {typeof(ITestResource).FullName}. Only nullable value types can be mapped to a dictionary."));
        }


        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new ClientTypeMapper(new[] { typeof(ITestResource) });
            this.extendedMapper = new ExtendedResourceMapper(this.typeMapper);
        }


        private interface ICustomTestResourceThatGotNonNullableInteger : ITestResource
        {
            int NotNullable { get; set; }
        }

        private interface ICustomTestResourceWithSelfReference : ITestResource
        {
            new ICustomTestResourceWithSelfReference Friend { get; }
        }
    }
}

