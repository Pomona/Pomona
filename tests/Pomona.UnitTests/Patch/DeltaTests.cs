#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;

namespace Pomona.UnitTests.Patch
{
    [TestFixture]
    public class DeltaTests
    {
        public class TestResourcePostForm : PostResourceBase, ITestResource
        {
            private static readonly PropertyWrapper<ITestResource, int> idPropWrapper =
                new PropertyWrapper<ITestResource, int>("Id");
            readonly static PropertyWrapper<ITestResource, string> infoPropWrapper = new PropertyWrapper<ITestResource, string>("Info");
            readonly static PropertyWrapper<ITestResource, ITestResource> spousePropWrapper = new PropertyWrapper<ITestResource, ITestResource>("Spouse");
            readonly static PropertyWrapper<ITestResource, ITestResource> friendPropWrapper = new PropertyWrapper<ITestResource, ITestResource>("Friend");
            readonly static PropertyWrapper<ITestResource, IList<ITestResource>> childrenPropWrapper = new PropertyWrapper<ITestResource, IList<ITestResource>>("Children");

            public int Id { get { return base.OnGet(idPropWrapper); } set { base.OnSet(idPropWrapper, value); } }
            public string Info { get { return base.OnGet(infoPropWrapper); } set { base.OnSet(infoPropWrapper, value); } }
            public ITestResource Spouse { get { return base.OnGet(spousePropWrapper); } set { base.OnSet(spousePropWrapper, value); } }
            public ITestResource Friend { get { return base.OnGet(friendPropWrapper); } set { base.OnSet(friendPropWrapper, value); } }
            public IList<ITestResource> Children { get { return base.OnGet(childrenPropWrapper); } set { base.OnSet(childrenPropWrapper, value); } }
        }

        public class TestResource : ITestResource
        {
            private readonly List<ITestResource> children = new List<ITestResource>();
            public int Id { get; set; }
            public string Info { get; set; }
            public ITestResource Spouse { get; set; }
            public ITestResource Friend { get; set; }

            public IList<ITestResource> Children
            {
                get { return children; }
            }
        }

        [ResourceInfo(InterfaceType = typeof (ITestResource), JsonTypeName = "TestResource",
            PocoType = typeof (TestResource), PostFormType  = typeof(TestResourcePostForm),  UriBaseType = typeof (ITestResource), UrlRelativePath = "test-resources")]
        public interface ITestResource : IClientResource
        {
            [ResourceIdProperty]
            int Id { get; set; }
            string Info { get; set; }

            ITestResource Spouse { get; set; }
            ITestResource Friend { get; set; }
            IList<ITestResource> Children { get; }
        }

        private readonly ITypeMapper typeMapper = new ClientTypeMapper(typeof (ITestResource).WrapAsEnumerable());

        private ITestResource GetObjectProxy()
        {
            var original = new TestResource
                {
                    Info = "Hei",
                    Children = { new TestResource { Info = "Childbar", Id = 1}, new TestResource { Info = "ChildToRemove", Id = 2} },
                    Spouse = new TestResource { Info = "Jalla", Id = 3},
                    Friend = new TestResource { Info = "good friend", Id = 4},
                    Id = 5
                };

            var proxy = (ITestResource)ObjectDeltaProxyBase.CreateDeltaProxy(original,
                                                                             typeMapper.GetClassMapping(
                                                                                 typeof (ITestResource)),
                                                                             typeMapper, null);

            Assert.IsFalse(((Delta)proxy).IsDirty);
            return proxy;
        }

        private ITestResource GetObjectWithAllDeltaOperations()
        {
            var proxy = GetObjectProxy();

            proxy.Info = "Lalalala";
            proxy.Children.First().Info = "Modified child";
            var childToRemove = proxy.Children.First(x => x.Info == "ChildToRemove");
            proxy.Children.Remove(childToRemove);
            proxy.Children.Add(new TestResourcePostForm() { Info = "Version2" });
            proxy.Spouse = new TestResourcePostForm() { Info = "BetterWife" };
            proxy.Friend.Info = "ModifiedFriend";

            var childCollectionDelta = (CollectionDelta<ITestResource>)proxy.Children;
            Assert.That(childCollectionDelta.RemovedItems.Count(), Is.EqualTo(1));
            Assert.That(childCollectionDelta.RemovedItems.First().Info, Is.EqualTo("ChildToRemove"));
            return proxy;
        }

        [Test]
        public void AddItemToChildren_IsInAddedItemsAndMarksParentAsDirty()
        {
            var proxy = GetObjectProxy();
            proxy.Children.Add(new TestResource { Info = "AddedChild" });
            var childCollectionDelta = (CollectionDelta<ITestResource>)proxy.Children;
            Assert.That(childCollectionDelta.AddedItems.Count(), Is.EqualTo(1));
            Assert.That(childCollectionDelta.AddedItems.First().Info, Is.EqualTo("AddedChild"));
            Assert.That(childCollectionDelta.RemovedItems.Count(), Is.EqualTo(0));
            Assert.That(childCollectionDelta.ModifiedItems.Count(), Is.EqualTo(0));
            Assert.That(((Delta)proxy).IsDirty);
        }

        [Test]
        public void CreateDeltaProxy_IsSuccessful_NOCOMMIT()
        {
            var proxy = GetObjectWithAllDeltaOperations();

            ((Delta)proxy).Apply();

            var original = ((Delta)proxy).Original;
        }

        [Test]
        public void ModifyChildItem_IsInModifiedItemsAndMarksParentAsDirty()
        {
            var proxy = GetObjectProxy();
            proxy.Children.First().Info = "WASMODIFIED";
            var childCollectionDelta = (CollectionDelta<ITestResource>)proxy.Children;
            Assert.That(childCollectionDelta.AddedItems.Count(), Is.EqualTo(0));
            Assert.That(childCollectionDelta.RemovedItems.Count(), Is.EqualTo(0));
            Assert.That(childCollectionDelta.ModifiedItems.Count(), Is.EqualTo(1));
            Assert.That(childCollectionDelta.ModifiedItems.First().Info, Is.EqualTo("WASMODIFIED"));
            Assert.That(((Delta)proxy).IsDirty);
        }

        [Test]
        public void RemoveItemFromChildren_IsInRemovedItemsAndMarksParentAsDirty()
        {
            var proxy = GetObjectProxy();
            var childCollectionDelta = (CollectionDelta<ITestResource>)proxy.Children;
            var childToRemove = proxy.Children.First(x => x.Info == "ChildToRemove");
            proxy.Children.Remove(childToRemove);
            Assert.That(childCollectionDelta.AddedItems.Count(), Is.EqualTo(0));
            Assert.That(childCollectionDelta.RemovedItems.Count(), Is.EqualTo(1));
            Assert.That(childCollectionDelta.RemovedItems.First().Info, Is.EqualTo("ChildToRemove"));
            Assert.That(childCollectionDelta.ModifiedItems.Count(), Is.EqualTo(0));
            Assert.That(((Delta)proxy).IsDirty);
        }

        [Test]
        public void TestSerialization_NOCOMMIT()
        {
            var jsonSerializer = new PomonaJsonSerializerFactory().GetSerialier();
            using (var stringWriter = new StringWriter())
            {
                jsonSerializer.Serialize(new ClientSerializationContext(typeMapper), GetObjectWithAllDeltaOperations(),
                                         stringWriter, typeMapper.GetClassMapping(typeof (ITestResource)));
                Console.WriteLine(stringWriter.ToString());
            }

            Assert.Fail();
        }
    }
}