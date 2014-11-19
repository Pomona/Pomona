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

using Pomona.Common.ExtendedResources;
using Pomona.Common.Linq;
using Pomona.Example.Models;

namespace Pomona.SystemTests.Linq
{
    [TestFixture]
    public class ExtendedResourceClientTests : ClientTestsBase
    {
        public List<IExtendedResource> Query_ExtendedResource_UsingValueFromClosure_GenericMethod<T>(
            string capturedArgument)
        {
            return
                Client.DictionaryContainers.Query<IExtendedResource>().Where(x => x.CustomString == capturedArgument)
                    .ToList();
        }


        private IDictionaryContainer PostResourceWithAttributes()
        {
            return Client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });
        }


        [Category("TODO")]
        [Test(Description = "Not yet working, need a proper extended resource collection wrapper for this.")]
        public void PatchExtendedResource_AddItemToWrappedCollection()
        {
            var entity = new HasReferenceToDictionaryContainer();
            Save(entity);

            var resource = Client.Query<ITestParentClientResource>().First(x => x.Id == entity.Id);

            var patchedResource =
                Client.Patch(resource,
                             x =>
                             {
                                 var extendedForm = new StringToObjectDictionaryContainerForm()
                                     .Wrap<IStringToObjectDictionaryContainer, ITestClientResource>();
                                 x.OtherContainers.Add(
                                     extendedForm);
                                 extendedForm.Jalla = "Hahaha";
                             },
                             x => x.Expand(y => y.OtherContainers));

            Assert.That(patchedResource.OtherContainers, Has.Count.EqualTo(1));
        }


        [Test]
        public void PatchExtendedResource_SetAttribute_UpdatesAttribute()
        {
            var entity = new StringToObjectDictionaryContainer
            {
                Map = { { "Text", "testtest" }, { "NoModify", "Blablabla" } }
            };
            Save(entity);

            var resource = Client.Query<IExtendedResource3>().First(x => x.Id == entity.Id);

            var patchedResource =
                Client.Patch(resource,
                             x =>
                             {
                                 x.Text = "UPDATED!";
                             });

            Assert.That(patchedResource.Text, Is.EqualTo("UPDATED!"));
        }


        [Test]
        public void Post_ExtendedResource_Having_Non_Nullable_Integer_Property_Throws_ExtendedResourceMappingException()
        {
            var ex = Assert.Throws<ExtendedResourceMappingException>(
                () =>
                    Client.StringToObjectDictionaryContainers
                    .Post<IExtendedResourceWithNonNullableInteger, IExtendedResourceWithNonNullableInteger>(
                        p => p.NonNullableNumber = 34));
            Assert.That(ex.Message,
                        Is.EqualTo(
                            "Unable to map property NonNullableNumber of type "
                            + "Pomona.SystemTests.Linq.ExtendedResourceClientTests+IExtendedResourceWithNonNullableInteger "
                            + "to underlying dictionary property Map of Critters.Client.IStringToObjectDictionaryContainer. "
                            + "Only nullable value types can be mapped to a dictionary."));
        }


        [Test]
        public void PostExtendedResource()
        {
            var response = (IExtendedResource3)Client.Post<IExtendedResource3>(x =>
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
        public void PostExtendedResourceHavingReferenceToAnotherExtendedResource_IsSuccessful()
        {
            var extendedFarm =
                (IDecoratedMusicalFarm)Client.Post<IDecoratedMusicalFarm>(x => x.Name = "The music farm");
            var musicalCritter =
                (IDecoratedMusicalCritter)Client.Post<IDecoratedMusicalCritter>(x => x.Farm = extendedFarm);
            Assert.That(musicalCritter.Farm.Id, Is.EqualTo(extendedFarm.Id));
        }


        [Test]
        public void
            PostExtendedResourceWhenDifferentTypeIsReturnedFromPost_AndResponseIsExtendedType_ReturnsCorrectResponse()
        {
            var response = Client.Orders.Post<ICustomOrder, ICustomOrderResponse>(co =>
            {
                co.Description = "Custom order";
                co.Items.Add(new OrderItemForm());
            });
            Assert.That(response, Is.AssignableTo<ICustomOrderResponse>());
            Assert.That(response.Order, Is.AssignableTo<ICustomOrder>());
        }


        [Test]
        public void
            PostExtendedResourceWhenDifferentTypeIsReturnedFromPost_AndResponseTypeIsNotSpecified_ReturnsCorrectResponse
            ()
        {
            var response = Client.Orders.Post<ICustomOrder>(co =>
            {
                co.Description = "Custom order";
                co.Items.Add(new OrderItemForm());
            });
            Assert.That(response, Is.AssignableTo<IOrderResponse>());
        }


        [Test]
        public void PostNonExtendedResourceHavingReferenceToAnotherExtendedResource_IsSuccessful()
        {
            var extendedFarm =
                (IDecoratedMusicalFarm)Client.Post<IDecoratedMusicalFarm>(x => x.Name = "The music farm");
            var musicalCritter =
                (IMusicalCritter)Client.Post<IMusicalCritter>(x => x.Farm = extendedFarm);
            Assert.That(musicalCritter.Farm.Id, Is.EqualTo(extendedFarm.Id));
        }


        [Test]
        public void QueryExtendedResource2_WhereDictIsOnBaseInterface_ReturnsExtendedResource2()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(IExtendedResource), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));
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

            var results = Client.Query<IExtendedResource2>()
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
        public void QueryExtendedResource3_ToQueryResult_ReturnsQueryResultOfExtendedResource()
        {
            var timeValue = new DateTime(2042, 2, 4, 6, 3, 2);
            var dictContainer = Repository.Save(new StringToObjectDictionaryContainer
            {
                Map = { { "Text", "foobar" }, { "Number", 32 }, { "Time", timeValue } }
            });

            var results = Client.Query<IExtendedResource3>()
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
        public void QueryExtendedResource3_WhereDictIsStringToObject_ReturnsExtendedResource3()
        {
            var timeValue = new DateTime(2042, 2, 4, 6, 3, 2);
            var dictContainer = Repository.Save(new StringToObjectDictionaryContainer
            {
                Map = { { "Text", "foobar" }, { "Number", 32 }, { "Time", timeValue } }
            });

            var results = Client.Query<IExtendedResource3>()
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
        public void QueryExtendedResourceWithBoolean_ReturnsExtendedResource()
        {
            var dictContainer =
                Repository.Save(new StringToObjectDictionaryContainer { Map = { { "TheBool", true } } });

            var results = Client.Query<IExtendedResourceWithBoolean>()
                .Where(x => x.TheBool == true && x.TheBool.HasValue && x.TheBool.Value)
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];
            Assert.That(result.TheBool, Is.True);
        }


        [Test]
        public void QueryExtendedResource_ReturnsExtendedResource()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(IExtendedResource), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = PostResourceWithAttributes();

            var results = Client.Query<IExtendedResource>()
                .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }


        [Test]
        public void QueryExtendedResource_SelectExtendedResourceToAnonymousType()
        {
            Repository.Save(new StringToObjectDictionaryContainer() { Map = { { "Text", "Lolo" } } });
            var results = Client.Query<IExtendedResource3>().Select(x => new { x, x.Text }).ToList();
            Assert.That(results, Has.Count.EqualTo(1));
            var result = results.First();
            Assert.That(result.Text, Is.EqualTo("Lolo"));
        }


        [Test]
        public void QueryExtendedResource_UsingFirstOrDefault_ReturnsExtendedResource()
        {
            //var visitor = new TransformAdditionalPropertiesToAttributesVisitor(typeof(IExtendedResource), typeof(IDictionaryContainer), (PropertyInfo)ReflectionHelper.GetInstanceMemberInfo<IDictionaryContainer>(x => x.Map));

            var dictionaryContainer = Client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });

            var result =
                Client.Query<IExtendedResource>()
                    .FirstOrDefault(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob");

            Assert.That(result.Id, Is.EqualTo(dictionaryContainer.Id));
            Assert.That(result.CustomString, Is.EqualTo(dictionaryContainer.Map["CustomString"]));
        }


        [Test]
        public void QueryExtendedResource_UsingGroupBy_ReturnsExtendedResource()
        {
            Client.DictionaryContainers.Post<IDictionaryContainer>(
                x =>
                {
                    x.Map.Add("CustomString", "Lalalala");
                    x.Map.Add("OtherCustom", "Blob rob");
                });

            var result =
                Client.Query<IExtendedResource>()
                    .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                    .GroupBy(x => x.CustomString)
                    .Select(x => new { x.Key })
                    .ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo("Lalalala"));
        }


        [Category("TODO")]
        [Test(
            Description =
                "client.SomeRepo.OfType<T>() does not work when T is an extended client resource. Should ideally work like client.SomeRepo.Query<T>."
            )]
        public void QueryExtendedResourcesUsingOfTypeDirectlyOnRepository_ReturnsExtendedResource()
        {
            var dictionaryContainer = PostResourceWithAttributes();

            var results = Client.StringToObjectDictionaryContainers.OfType<IExtendedResource>()
                .Where(x => x.CustomString == "Lalalala" && x.OtherCustom == "Blob rob")
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
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
            Assert.That(resource.OtherContainers.Count, Is.EqualTo(1));
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


        [Test]
        public void Query_ExtendedResource_UsingValueFromClosure()
        {
            var response =
                Query_ExtendedResource_UsingValueFromClosure_GenericMethod<IExtendedResource>("NO RESULTS WILL BE FOUND");
            Assert.That(response.Count, Is.EqualTo(0));
        }


        [Test(Description = "Regression test for problem with TransformAdditionalPropertiesToAttributesVisitor.")]
        public void Query_ExtendedResource_UsingValueFromStaticField()
        {
            Assert.DoesNotThrow(() => Client.DictionaryContainers.Query<IExtendedResource>().Where(
                x => DateTime.UtcNow > DateTime.MinValue && x.Id == 33).ToList());
        }


        [Test]
        public void UnwrapResource_IsSuccessful()
        {
            var resource = PostResourceWithAttributes();
            var unwrapped =
                Client.DictionaryContainers.Query<IExtendedResource>().First(x => x.Id == resource.Id)
                    .Unwrap<IDictionaryContainer>();
            Assert.That(unwrapped, Is.Not.AssignableTo<IExtendedResource>());
        }


        [Test]
        public void WrapResource_IsSuccessful()
        {
            var resource = PostResourceWithAttributes();
            var wrapped = resource.Wrap<IDictionaryContainer, IExtendedResource>();
            Assert.That(wrapped.CustomString, Is.EqualTo("Lalalala"));
            Assert.That(wrapped.OtherCustom, Is.EqualTo("Blob rob"));
        }


        public interface IExtendedResourceWithNonNullableInteger : IStringToObjectDictionaryContainer
        {
            int NonNullableNumber { get; set; }
        }

        public interface IExtendedResource : IDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }

        public interface IExtendedResource2 : ISubtypedDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
        }

        public interface IExtendedResource3 : IStringToObjectDictionaryContainer
        {
            int? Number { get; set; }
            string Text { get; set; }
            DateTime? Time { get; set; }
        }

        public interface IExtendedResourceWithBoolean : IStringToObjectDictionaryContainer
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

        public interface ICustomOrder : IOrder
        {
        }

        public interface ICustomOrderResponse : IOrderResponse
        {
            new ICustomOrder Order { get; set; }
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
    }
}