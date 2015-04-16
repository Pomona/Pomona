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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.Serialization.Patch;
using Pomona.TestHelpers;
using Pomona.UnitTests.TestResources;

namespace Pomona.UnitTests.Patch
{
    [TestFixture]
    public class DeltaTests
    {
        private readonly ClientTypeMapper typeMapper = new ClientTypeMapper(typeof(ITestResource).WrapAsEnumerable());


        private ITestResource GetObjectProxy(Action<TestResource> modifyOriginal = null)
        {
            var original = new TestResource
            {
                Info = "Hei",
                Children =
                {
                    new TestResource { Info = "Childbar", Id = 1 },
                    new TestResource { Info = "ChildToRemove", Id = 2 }
                },
                Spouse = new TestResource { Info = "Jalla", Id = 3 },
                Friend = new TestResource { Info = "good friend", Id = 4 },
                Id = 5
            };

            if (modifyOriginal != null)
                modifyOriginal(original);

            var proxy = (ITestResource)ObjectDeltaProxyBase.CreateDeltaProxy(original,
                                                                             this.typeMapper.FromType(
                                                                                 typeof(ITestResource)),
                                                                             this.typeMapper,
                                                                             null,
                                                                             typeof(ITestResource));

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
            proxy.Children.Add(new TestResourcePostForm { Info = "Version2" });
            proxy.Spouse = new TestResourcePostForm { Info = "BetterWife" };
            proxy.Friend.Info = "ModifiedFriend";

            var childCollectionDelta = (CollectionDelta<ITestResource>)proxy.Children;
            Assert.That(childCollectionDelta.RemovedItems.Count(), Is.EqualTo(1));
            Assert.That(childCollectionDelta.RemovedItems.First().Info, Is.EqualTo("ChildToRemove"));
            return proxy;
        }


        private JObject CreateJsonPatch(ITestResource proxy)
        {
            var pomonaClient = Substitute.For<IPomonaClient>();
            var jsonSerializer =
                (ITextSerializer)
                    (new PomonaJsonSerializerFactory().GetSerializer(
                        new ClientSerializationContextProvider(this.typeMapper,
                                                               pomonaClient,
                                                               pomonaClient)));
            using (var stringWriter = new StringWriter())
            {
                jsonSerializer.Serialize(stringWriter,
                                         proxy,
                                         new SerializeOptions() { ExpectedBaseType = typeof(ITestResource) });
                Console.WriteLine(stringWriter.ToString());
                return JObject.Parse(stringWriter.ToString());
                // TODO: More assertions here!
            }
        }


        [Test]
        public void AddItemToChildren_IsInAddedItemsAndMarksParentAsDirty()
        {
            var proxy = GetObjectProxy();
            proxy.Children.Add(new TestResourcePostForm() { Info = "AddedChild" });
            var childCollectionDelta = (CollectionDelta<ITestResource>)proxy.Children;
            Assert.That(childCollectionDelta.AddedItems.Count(), Is.EqualTo(1));
            Assert.That(childCollectionDelta.AddedItems.Cast<ITestResource>().First().Info, Is.EqualTo("AddedChild"));
            Assert.That(childCollectionDelta.RemovedItems.Count(), Is.EqualTo(0));
            Assert.That(childCollectionDelta.ModifiedItems.Count(), Is.EqualTo(0));
            Assert.That(((Delta)proxy).IsDirty);
        }


        [Test]
        public void ApplyChanges_IsSuccessful()
        {
            var expected = GetObjectWithAllDeltaOperations();
            var proxy = (IDelta<ITestResource>)GetObjectWithAllDeltaOperations();

            proxy.Apply();

            var actual = proxy.Original;
            Assert.That(actual.Friend.Info, Is.EqualTo(expected.Friend.Info));
            Assert.That(actual.Info, Is.EqualTo(expected.Info));
        }


        [Test]
        public void JsonPatch_DictionaryWithMultipleValues_SetKeyToSameValueAsBefore_PropertyIsNotPartOfPatch()
        {
            var proxy = GetObjectProxy(tr =>
            {
                tr.Dictionary["lala"] = "bah";
            });
            proxy.Dictionary["lala"] = "bah";
            proxy.Dictionary["whoopsi"] = "nain";
            var jobject = CreateJsonPatch(proxy);
            var jdict = jobject.AssertHasPropertyWithObject("dictionary");
            jdict.AssertDoesNotHaveProperty("lala");
            Assert.That(jdict.AssertHasPropertyWithString("whoopsi"), Is.EqualTo("nain"));
        }


        [Test]
        public void JsonPatch_DictionaryWithSingleChange_SetKeyToSameValueAsBefore_DictionaryIsNotPartOfPatch()
        {
            var proxy = GetObjectProxy(tr => tr.Dictionary["lala"] = "bah");
            proxy.Dictionary["lala"] = "bah";
            var jobject = CreateJsonPatch(proxy);
            jobject.AssertDoesNotHaveProperty("dictionary");
        }


        [Test]
        public void JsonPatch_DoesNotSerializePropertiesChangedToEqualValue()
        {
            var proxy = GetObjectProxy();
            proxy.Info = proxy.Info;
            var jobject = CreateJsonPatch(proxy);
            jobject.AssertDoesNotHaveProperty("info");
        }


        [Test]
        public void JsonPatch_HasChangedValueProperty()
        {
            var proxy = GetObjectWithAllDeltaOperations();
            var jobject = CreateJsonPatch(proxy);
            jobject.AssertHasPropertyWithValue("info", "Lalalala");
            var spouseReplacement = jobject.AssertHasPropertyWithObject("!spouse");
            spouseReplacement.AssertHasPropertyWithValue("info", "BetterWife");
        }


        [Test]
        public void JsonPatch_SerializesReplacedObjectPropertyCorrectly()
        {
            var proxy = GetObjectWithAllDeltaOperations();
            var jobject = CreateJsonPatch(proxy);
            var spouseReplacement = jobject.AssertHasPropertyWithObject("!spouse");
            spouseReplacement.AssertHasPropertyWithValue("info", "BetterWife");
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
        public void ModifyProperty_ChangedFromNonNullToNull_IsInModifiedProperties()
        {
            var proxy = GetObjectProxy();
            // Set to same value as it was
            proxy.Info = null;
            Assert.That(((ObjectDelta)proxy).ModifiedProperties,
                        Contains.Item(new KeyValuePair<string, object>("Info", null)));
        }


        [Test]
        public void ModifyProperty_ChangedFromNullToNull_IsNotInModifiedProperties()
        {
            var proxy = GetObjectProxy(orig => orig.Info = null);
            // Set to same value as it was
            proxy.Info = null;
            Assert.That(((ObjectDelta)proxy).ModifiedProperties, Is.Empty);
        }


        [Test]
        public void ModifyProperty_ChangedToNewValueThenBackToOriginalValue_IsNotInModifiedProperties()
        {
            var proxy = GetObjectProxy();
            // Set to same value as it was
            var origInfo = proxy.Info;
            proxy.Info = null;
            proxy.Info = "lalala";
            proxy.Info = origInfo;
            Assert.That(((ObjectDelta)proxy).ModifiedProperties, Is.Empty);
        }


        [Test]
        public void ModifyProperty_SetToSameValueAsOriginal_IsNotInModifiedItems()
        {
            var proxy = GetObjectProxy();
            // Set to same value as it was
            proxy.Info = proxy.Info;
            Assert.That(((ObjectDelta)proxy).ModifiedProperties, Is.Empty);
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
    }
}