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
using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common.Linq;
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
            int? Number { get; set; }
            string Text { get; set; }
            DateTime? Time { get; set; }
        }

        public interface ICustomTestEntityWithBoolean : IStringToObjectDictionaryContainer
        {
            bool? TheBool { get; set; }
        }

        public interface ITestClientResource : IStringToObjectDictionaryContainer
        {
            string Jalla { get; set; }
        }

        public interface IDecoratedWeapon : IWeapon
        {
        }

        public interface IDecoratedCritter : ICritter
        {
        }

        public interface IDecoratedMusicalWeapon : IWeapon
        {
        }

        public interface IDecoratedMusicalFarm : IFarm
        {
        }

        public interface IDecoratedMusicalCritter : IMusicalCritter
        {
            new IDecoratedMusicalFarm Farm { get; set; }
            new IList<IDecoratedMusicalWeapon> Weapons { get; set; }
        }

        public interface ITestParentClientResource : IHasReferenceToDictionaryContainer
        {
            new ITestClientResource Container { get; set; }
            new IList<ITestClientResource> OtherContainers { get; set; }
        }


        [Test]
        public void PatchCustomClientSideResource_SetAttribute_UpdatesAttribute()
        {
            var entity = new StringToObjectDictionaryContainer
            {
                Map = { { "Text", "testtest" }, { "NoModify", "Blablabla" } }
            };
            Save(entity);

            var resource = Client.Query<ICustomTestEntity3>().First(x => x.Id == entity.Id);

            var patchedResource =
                Client.Patch(resource, x => { x.Text = "UPDATED!"; });

            Assert.That(patchedResource.Text, Is.EqualTo("UPDATED!"));
        }


        [Test]
        public void PostCustomTestEntity()
        {
            var response = (ICustomTestEntity3)Client.Post<ICustomTestEntity3>(x =>
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
                Map = { { "CustomString", "Lalalala" }, { "OtherCustom", "Blob rob" } },
                SomethingExtra = "Hahahohohihi"
            };

            Repository.Save<DictionaryContainer>(subtypedDictionaryContainer);

            // Post does not yet work on subtypes
            //this.client.DictionaryContainers.Post<ISubtypedDictionaryContainer>(
            //    x =>
            //    {
            //        x.Map.Add("CustomString", "Lalalala");
            //        x.Map.Add("OtherCustom", "Blob rob");
            //        x.SomethingExtra = "Hahahohohihi";
            //    });

            var results = Client.Query<ICustomTestEntity2>()
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
        public void QueryCustomTestEntity3_ToQueryResult_ReturnsQueryResultOfCustomTestEntity()
        {
            var timeValue = new DateTime(2042, 2, 4, 6, 3, 2);
            var dictContainer = Repository.Save(new StringToObjectDictionaryContainer
            {
                Map = { { "Text", "foobar" }, { "Number", 32 }, { "Time", timeValue } }
            });

            var results = Client.Query<ICustomTestEntity3>()
                .Where(x => x.Number > 5 && x.Text == "foobar" && x.Time == timeValue)
                .IncludeTotalCount()
                .ToQueryResult();

            Assert.That(results, Has.Count.EqualTo(1));
            var result = results.First();
            Assert.That(result.Number, Is.EqualTo(32));
            Assert.That(result.Text, Is.EqualTo("foobar"));
            Assert.That(result.Time, Is.EqualTo(timeValue));
            Assert.That(result.Id, Is.EqualTo(dictContainer.Id));
        }


        [Test]
        public void QueryCustomTestEntity3_WhereDictIsStringToObject_ReturnsCustomTestEntity3()
        {
            var timeValue = new DateTime(2042, 2, 4, 6, 3, 2);
            var dictContainer = Repository.Save(new StringToObjectDictionaryContainer
            {
                Map = { { "Text", "foobar" }, { "Number", 32 }, { "Time", timeValue } }
            });

            var results = Client.Query<ICustomTestEntity3>()
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
        public void QueryCustomTestEntityWithBoolean_ReturnsCustomTestEntity()
        {
            var dictContainer =
                Repository.Save(new StringToObjectDictionaryContainer { Map = { { "TheBool", true } } });

            var results = Client.Query<ICustomTestEntityWithBoolean>()
                .Where(x => x.TheBool == true && x.TheBool.HasValue && x.TheBool.Value)
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];
            Assert.That(result.TheBool, Is.True);
        }


        [Test]
        public void QueryCustomTestEntity_ReturnsCustomTestEntity()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(ICustomTestEntity), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = Client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });

            var results = Client.Query<ICustomTestEntity>()
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

            var dictionaryContainer = Client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });

            var result =
                Client.Query<ICustomTestEntity>()
                    .FirstOrDefault(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob");

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }


        [Test]
        public void QueryCustomTestEntity_UsingGroupBy_ReturnsCustomTestEntity()
        {
            Client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });

            var result =
                Client.Query<ICustomTestEntity>()
                    .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                    .GroupBy(x => x.CustomString)
                    .Select(x => new { x.Key })
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


        [Test]
        public void Query_ClientResourceWithReferenceToAnotherClientResource_First()
        {
            var child = Save(new StringToObjectDictionaryContainer { Map = { { "Jalla", "booohoo" } } });
            var parent = Save(new HasReferenceToDictionaryContainer { Container = child });

            var resource =
                Client.Query<ITestParentClientResource>()
                    .First(x => x.Id == parent.Id && x.Container.Jalla == "booohoo");
            Assert.That(resource.Container, Is.Not.Null);
            Assert.That(resource.Container.Jalla, Is.EqualTo("booohoo"));
        }


        [Test]
        public void Query_ClientResourceWithReferenceToListOfClientResources_First()
        {
            var child = Save(new StringToObjectDictionaryContainer { Map = { { "Jalla", "booohoo" } } });
            var otherChild = Save(new StringToObjectDictionaryContainer { Map = { { "Jalla", "blabla" } } });
            var parent =
                Save(new HasReferenceToDictionaryContainer { Container = child, OtherContainers = { otherChild } });

            var resource =
                Client.Query<ITestParentClientResource>()
                    .First(
                        x =>
                            x.Id == parent.Id && x.Container.Jalla == "booohoo" &&
                            x.OtherContainers.Any(y => y.Jalla == "blabla"));

            Assert.That(resource.Container, Is.Not.Null);
            Assert.That(resource.Container.Jalla, Is.EqualTo("booohoo"));
            Assert.That(resource.OtherContainers[0].Jalla, Is.EqualTo("blabla"));
        }


        [Test]
        public void Query_ClientSideResourceReturningNoResults_FirstOrDefaultReturnsNull()
        {
            Assert.That(Client.Query<ITestClientResource>().FirstOrDefault(x => x.Jalla == Guid.NewGuid().ToString()),
                Is.Null);
        }


        [Test]
        public void
            Query_ExtendedResourceSubclassedOnServer_ThatGotListOfAnotherTypeOfExtendedResources_WrapsResourcesCorrectly
            ()
        {
            var extendedMusicalCritter = Client.Critters.Query<IDecoratedMusicalCritter>().First();
            var weapons = extendedMusicalCritter.Weapons;
            Assert.That(weapons.Count, Is.EqualTo(((ICritter)extendedMusicalCritter).Weapons.Count));
        }


        [Test]
        public void
            Query_ExtendedResourceSubclassedOnServer_ThatGotReferenceToAnotherTypeOfExtendedResources_WrapsResourceCorrectly
            ()
        {
            var extendedMusicalCritter = Client.Critters.Query<IDecoratedMusicalCritter>().First();
            Assert.That(extendedMusicalCritter.Farm, Is.Not.Null);
        }
    }
}