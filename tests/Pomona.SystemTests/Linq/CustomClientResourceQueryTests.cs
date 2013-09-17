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

using System;
using System.Linq;
using Critters.Client;
using NUnit.Framework;
using Pomona.Example.Models;

namespace Pomona.SystemTests.Linq
{
    [TestFixture]
    public class CustomClientResourceQueryTests : ClientTestsBase
    {
        public interface ICustomTestEntity : IDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }

        public interface ICustomTestEntity2 : ISubtypedDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }

        public interface ICustomTestEntity3 : IStringToObjectDictionaryContainer
        {
            string Text { get; set; }
            int? Number { get; set; }
            DateTime? Time { get; set; }
        }

        public interface ITestClientResource : IStringToObjectDictionaryContainer
        {
            string Jalla { get; set; }
        }

        public interface ITestParentClientResource : IHasReferenceToDictionaryContainer
        {
            new ITestClientResource Container { get; set; }
        }

        [Test]
        public void PatchCustomClientSideResource_SetAttribute_UpdatesAttribute()
        {
            var entity = new StringToObjectDictionaryContainer
                {
                    Map = {{"Text", "testtest"}}
                };
            Save(entity);

            var resource = client.Query<ICustomTestEntity3>().First(x => x.Id == entity.Id);

            var patchedResource =
                client.Patch(resource, x => { x.Text = "UPDATED!"; });


            Assert.That(patchedResource.Text, Is.EqualTo("UPDATED!"));
        }

        [Test]
        public void PostCustomTestEntity()
        {
            var response = (ICustomTestEntity3) client.Post<ICustomTestEntity3>(x =>
                {
                    x.Number = 123;
                    x.Text = "foobar";
                    x.Time = new DateTime(2030, 3, 4, 5, 3, 2);
                });

            Assert.That(response.Number, Is.EqualTo(123));
            Assert.That(response.Text, Is.EqualTo("foobar"));
            Assert.That(response.Time, Is.EqualTo(new DateTime(2030, 3, 4, 5, 3, 2)));
        }

        [Test]
        public void QueryCustomTestEntity2_WhereDictIsOnBaseInterface_ReturnsCustomTestEntity2()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));
            var subtypedDictionaryContainer = new SubtypedDictionaryContainer
                {
                    Map = {{"CustomString", "Lalalala"}, {"OtherCustom", "Blob rob"}},
                    SomethingExtra = "Hahahohohihi"
                };

            this.Repository.Save<DictionaryContainer>(subtypedDictionaryContainer);

            // Post does not yet work on subtypes
            //this.client.DictionaryContainers.Post<ISubtypedDictionaryContainer>(
            //    x =>
            //    {
            //        x.Map.Add("CustomString", "Lalalala");
            //        x.Map.Add("OtherCustom", "Blob rob");
            //        x.SomethingExtra = "Hahahohohihi";
            //    });

            var results = client.Query<ICustomTestEntity2>()
                                .Where(
                                    x =>
                                    x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob" &&
                                    x.SomethingExtra == "Hahahohohihi")
                                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(subtypedDictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(subtypedDictionaryContainer.Map["CustomString"]));
        }

        [Test]
        public void QueryCustomTestEntity3_WhereDictIsStringToObject_ReturnsCustomTestEntity3()
        {
            var timeValue = new DateTime(2042, 2, 4, 6, 3, 2);
            var dictContainer = this.Repository.Save(new StringToObjectDictionaryContainer
                {
                    Map = {{"Text", "foobar"}, {"Number", 32}, {"Time", timeValue}}
                });

            var results = client.Query<ICustomTestEntity3>()
                                .Where(x => x.Number > 5 && x.Text == "foobar" && x.Time == timeValue)
                                .ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            var result = results.First();
            Assert.That(result.Number, Is.EqualTo(32));
            Assert.That(result.Text, Is.EqualTo("foobar"));
            Assert.That(result.Time, Is.EqualTo(timeValue));
            Assert.That(result.Id, Is.EqualTo(dictContainer.Id));
        }


        [Test]
        public void QueryCustomTestEntity_ReturnsCustomTestEntity()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                    {
                        x.Map.Add("CustomString", "Lalalala");
                        x.Map.Add("OtherCustom", "Blob rob");
                    });

            var results = client.Query<ICustomTestEntity>()
                                .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }


        [Test]
        public void QueryCustomTestEntity_UsingFirstOrDefault_ReturnsCustomTestEntity()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                    {
                        x.Map.Add("CustomString", "Lalalala");
                        x.Map.Add("OtherCustom", "Blob rob");
                    });

            var result =
                client.Query<ICustomTestEntity>()
                      .FirstOrDefault(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob");

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }

        [Test]
        public void QueryCustomTestEntity_UsingGroupBy_ReturnsCustomTestEntity()
        {
            client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                    {
                        x.Map.Add("CustomString", "Lalalala");
                        x.Map.Add("OtherCustom", "Blob rob");
                    });

            var result =
                client.Query<ICustomTestEntity>()
                      .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                      .GroupBy(x => x.CustomString)
                      .Select(x => new {x.Key})
                      .ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo("Lalalala"));
        }

        [Category("TODO")]
        [Test(Description = "TODO: Reminder for func to be implemented.")]
        public void Query_ClientResourceWithNonNullableProperty_ThrowsSaneException_ExplainingWhyThisIsNotPossible()
        {
            Assert.Fail("Test not implemented, correct behaviour not yet defined.");
        }

        [Category("TODO")]
        [Test(Description = "TODO: Functionality not yet implemented.")]
        public void Query_ClientResourceWithReferenceToAnotherClientResource_First()
        {
            var child = Save(new StringToObjectDictionaryContainer {Map = {{"Jalla", "booohoo"}}});
            var parent = Save(new HasReferenceToDictionaryContainer {Container = child});

            var resource = client.Query<ITestParentClientResource>().First(x => x.Id == parent.Id);
            Assert.That(resource.Container, Is.Not.Null);
            Assert.That(resource.Container.Jalla, Is.EqualTo("booohoo"));

            Assert.Fail("Test not completed");
        }
    }
}