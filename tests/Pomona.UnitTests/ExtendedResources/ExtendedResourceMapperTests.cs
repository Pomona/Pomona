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
            Assert.That(ex.Message, Is.EqualTo(string.Format(
                "Unable to map property {0} of type {1} to underlying dictionary property {2} of {3}. Only nullable value types can be mapped to a dictionary.",
                "NotNullable", typeof(ICustomTestResourceThatGotNonNullableInteger).FullName, "Attributes", typeof(ITestResource).FullName)));
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