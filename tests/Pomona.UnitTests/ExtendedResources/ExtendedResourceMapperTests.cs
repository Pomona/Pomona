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

using Newtonsoft.Json.Linq;

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
        public void GetExtendedResourceInfo_ForCustomTestResourceThatGotSerializedAttributeProperty_PropertyIsOfRightType()
        {
            var resInfo = this.extendedMapper.GetExtendedResourceInfo(typeof(ICustomTestResourceThatGotSerializedAttributeProperty));
            Assert.That(resInfo.ExtendedProperties.Single(x => x.Property.Name == "SomeObject"),
                        Is.InstanceOf<SerializedExtendedAttributeProperty<object, FooBar>>());
        }


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


        [Test]
        public void Unwrap_resource_with_serialized_attribute_property()
        {
            var form = new TestResourcePostForm();
            form.Attributes["SomeObject"] = "{\"Bar\":\"hahaha\",\"Foo\":\"hihihi\"}";
            var wrapped =
                (ICustomTestResourceThatGotSerializedAttributeProperty)
                    this.extendedMapper.WrapForm(form, typeof(ICustomTestResourceThatGotSerializedAttributeProperty));
            Assert.That(wrapped.SomeObject, Is.Not.Null);
            Assert.That(wrapped.SomeObject.Bar, Is.EqualTo("hahaha"));
            Assert.That(wrapped.SomeObject.Foo, Is.EqualTo("hihihi"));
        }


        [Test]
        public void Wrap_resource_with_serialized_attribute_property()
        {
            var wrapped =
                (ICustomTestResourceThatGotSerializedAttributeProperty)
                    this.extendedMapper.WrapForm(new TestResourcePostForm(), typeof(ICustomTestResourceThatGotSerializedAttributeProperty));
            wrapped.SomeObject = new FooBar() { Bar = "hahaha", Foo = "hihihi" };
            var serialized = (string)wrapped.Attributes["SomeObject"];
            Assert.That(JToken.DeepEquals(JToken.Parse("{\"Bar\":\"hahaha\",\"Foo\":\"hihihi\"}"), JToken.Parse(serialized)));
        }


        public class FooBar
        {
            public string Bar { get; set; }
            public string Foo { get; set; }
        }

        private interface ICustomTestResourceThatGotNonNullableInteger : ITestResource
        {
            int NotNullable { get; set; }
        }

        public interface ICustomTestResourceThatGotSerializedAttributeProperty : ITestResource
        {
            [SerializedAsJson]
            FooBar SomeObject { get; set; }
        }

        private interface ICustomTestResourceWithSelfReference : ITestResource
        {
            new ICustomTestResourceWithSelfReference Friend { get; }
        }
    }
}