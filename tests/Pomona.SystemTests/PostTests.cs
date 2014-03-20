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
using System.Text;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Common.Web;
using Pomona.Example.Models;

using CustomEnum = Critters.Client.CustomEnum;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class PostTests : ClientTestsBase
    {
        [Test]
        public void PostAbstractClass_ThrowsExceptionOnClient()
        {
            try
            {
                var critter = (IAbstractAnimal)Client.Post<IAbstractAnimal>(x => { });
                throw new Exception("Pomona didn't throw an exception despite posting an abstract class!");
            }
            catch (MissingMethodException e)
            {
                StringAssert.Contains("Cannot create an abstract class.",
                    e.Message,
                    "Pomona should warn about posting an abstract class");
            }
        }


        [Test]
        //  Override the class so that it can be posted on the client, but is abstract on the server.
        public void PostAbstractClass_ThrowsExceptionOnServer()
        {
            try
            {
                var critter = (IAbstractOnServerAnimal)Client.Post<IAbstractOnServerAnimal>(x => { });
                throw new Exception("Pomona didn't throw an exception despite receiving an abstract class!");
            }
            catch (Exception e)
            {
                StringAssert.Contains("Pomona was unable to instantiate type ",
                    e.Message,
                    "Pomona should warn about posting an abstract class");
                StringAssert.Contains(", as it's an abstract type.",
                    e.Message,
                    "Pomona should warn about posting an abstract class");
            }
        }


        [Test]
        public void PostBlob_HavingByteArray()
        {
            var dataBytes = Encoding.ASCII.GetBytes("Lalalala");
            var response = Client.Blobs.Post(new BlobForm() { DataBytes = dataBytes });
            Assert.That(response.DataBytes, Is.EquivalentTo(dataBytes));
        }


        [Test]
        public void PostCritterWithExistingHat()
        {
            const string hatType = "Old";

            var hat = PostAHat(hatType);

            const string critterName = "Super critter";

            var critter = (ICritter)Client.Post<ICritter>(
                x =>
                {
                    x.Hat = hat;
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostCritterWithExistingHat_UsingFirstLazyQuery()
        {
            const string hatType = "Special hat";

            var hat = PostAHat(hatType);

            const string critterName = "Super critter";

            var critter = (ICritter)Client.Post<ICritter>(
                x =>
                {
                    x.Hat = Client.Hats.Query().Where(y => y.HatType.StartsWith("Special")).FirstLazy();
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostCritterWithHatForm()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = (ICritter)Client.Post<ICritter>(
                x =>
                {
                    x.Hat = new HatForm { HatType = hatType };
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }


        [Test]
        public void PostCritterWithNameTooLong_ThrowsExceptionWithErrorStatus()
        {
            var form = new CritterForm { Name = string.Join(" ", Enumerable.Repeat("John", 50)) };
            var exception = Assert.Throws<BadRequestException<IErrorStatus>>(() => Client.Critters.Post(form));
            Assert.That(exception.Body, Is.Not.Null);
            Assert.That(exception.Body.Message, Is.EqualTo("Critter can't have name longer than 50 characters."));
            Assert.That(exception.Body.ErrorCode, Is.EqualTo(1337));
        }


        [Test]
        public void PostCritterWithNonExistingHat_UsingFirstLazyQuery_ThrowsBadRequestExceptionHavingHelpfulMessage()
        {
            const string critterName = "Super critter";

            var lazyNonExistingHat = Client.Hats.Query().Where(y => y.Id == int.MaxValue).FirstLazy();
            var nonExistingHatUrl = ((IHasResourceUri)lazyNonExistingHat).Uri;
            var ex = Assert.Throws<BadRequestException>(() => Client.Post<ICritter>(
                x =>
                {
                    x.Hat = lazyNonExistingHat;
                    x.Name = critterName;
                }));
            Assert.That(ex.Message, Is.StringContaining(nonExistingHatUrl));
        }


        [Test]
        public void PostCritterWithSubscription_RunsOnDeserializationHook()
        {
            const string critterName = "Postal critter";

            var critter = (ICritter)Client.Post<ICritter>(
                x =>
                {
                    x.Subscriptions.Add(new SubscriptionForm
                    {
                        Model = new WeaponModelForm { Name = "blah" },
                        Sku = "haha",
                        StartsOn = DateTime.UtcNow
                    });
                    x.Name = critterName;
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Subscriptions.Count, Is.EqualTo(1));

            var critterEntity = CritterEntities.First(x => x.Id == critter.Id);
            Assert.That(critterEntity.Subscriptions[0].Critter, Is.EqualTo(critterEntity));
        }


        [Test]
        public void PostCritter_WithPatchOptionExpandWeapons_ExpandsWeapons()
        {
            var critter = Client.Critters.Post<IMusicalCritter>(x =>
            {
                x.Name = "klukluk";
                x.Weapons.Add(new WeaponForm()
                {
                    Model = new WeaponModelForm() { Name = "halalaksldk" },
                    Price = 23,
                    Strength = 34
                });
            },
                o => o.Expand(x => x.Weapons));
            Assert.That(critter.Weapons, Is.InstanceOf<List<IWeapon>>());
        }


        [Test]
        public void PostDictionaryContainer_WithItemSetInDictionary()
        {
            var response = (IDictionaryContainer)Client.Post<IDictionaryContainer>(x => { x.Map["cow"] = "moo"; });
            Assert.That(response.Map.ContainsKey("cow"));
            Assert.That(response.Map["cow"] == "moo");
        }


        [Test]
        public void PostEntityWithReadOnlyPropertySetThroughConstructor()
        {
            var o =
                Client.HasConstructorInitializedReadOnlyProperties.Post(
                    x =>
                    {
                        x.Crazy.Info = "bam!";
                        x.Crazy.Sickness = "adhd";
                    });
            Assert.That(o.Crazy.Info, Is.EqualTo("bam!"));
            Assert.That(o.Crazy.Sickness, Is.EqualTo("adhd"));
        }


        [Test]
        public void PostExposedInterface_ReturnsExposedInterfaceResource()
        {
            var entity = Client.ExposedInterfaces.Post(f => f.FooBar = "lalalSaa");
            Assert.That(entity, Is.TypeOf<ExposedInterfaceResource>());
        }


        [Test]
        public void PostHasCustomEnum()
        {
            var response = Client.HasCustomEnums.Post(
                x => { x.TheEnumValue = CustomEnum.Tock; });

            Assert.That(response.TheEnumValue, Is.EqualTo(CustomEnum.Tock));
        }


        [Test]
        public void PostHasNullableCustomEnum_WithNonNullValue()
        {
            var response = Client.HasCustomNullableEnums.Post(
                x => { x.TheEnumValue = CustomEnum.Tock; });
            Assert.That(response.TheEnumValue.HasValue, Is.True);
            Assert.That(response.TheEnumValue.Value, Is.EqualTo(CustomEnum.Tock));
        }


        [Test]
        public void PostHasNullableCustomEnum_WithNull()
        {
            var response = Client.HasCustomNullableEnums.Post(
                x => { x.TheEnumValue = null; });
            Assert.That(response.TheEnumValue.HasValue, Is.False);
        }


        [Test]
        public void PostJunkWithRenamedProperty()
        {
            var propval = "Jalla jalla";
            var junk =
                (IJunkWithRenamedProperty)
                    Client.Post<IJunkWithRenamedProperty>(x => { x.BeautifulAndExposed = propval; });

            Assert.That(junk.BeautifulAndExposed, Is.EqualTo(propval));
        }


        [Test]
        public void PostLonerWithOptionalNullablePropertySet_SetsProperty()
        {
            // Model is required, so an exception should be thrown.
            var date = DateTime.Now.AddDays(-2);
            var resource = Client.Loners.Post(new LonerForm { Name = "blah", Strength = 123, OptionalDate = date });
            Assert.That(resource.OptionalDate, Is.EqualTo(date));
        }


        [Test]
        public void PostLonerWithOptionalPropertyNotSet_DoesNotThrowException()
        {
            // Model is required, so an exception should be thrown.
            Assert.That(() => Client.Loners.Post(new LonerForm { Name = "blah", Strength = 123 }), Throws.Nothing);
        }


        [Test]
        public void PostMusicalCritter()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = (IMusicalCritter)Client.Post<IMusicalCritter>(
                x =>
                {
                    x.Hat = new HatForm { HatType = hatType };
                    x.Name = critterName;
                    x.BandName = "banana";
                    x.Instrument = new InstrumentForm { Type = "helo" };
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
            Assert.That(critter.BandName, Is.EqualTo("banana"));

            Assert.That(Repository.List<Critter>().Any(x => x.Id == critter.Id && x is MusicalCritter));
        }


        [Test]
        public void PostMusicalCritterUsingMethodTakingFormAndReturningMusicalCritter()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter =
                Client.Critters.Post(new MusicalCritterForm
                {
                    Hat = new HatForm { HatType = hatType },
                    BandName = "banana",
                    Name = critterName,
                    Instrument = new InstrumentForm { Type = "blablabla" }
                });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
            Assert.That(critter.BandName, Is.EqualTo("banana"));

            Assert.That(Repository.List<Critter>().Any(x => x.Id == critter.Id && x is MusicalCritter));
        }


        [Test]
        public void PostOrderWithItems()
        {
            var orderResponse =
                Client.Orders.Post(new PurchaseOrderForm { Items = { new OrderItemForm { Name = "blah" } } });
            Assert.That(orderResponse.Order.Items, Has.Count.EqualTo(1));
            Assert.That(orderResponse.Order.Items[0].Name, Is.EqualTo("blah"));
        }


        [Test]
        public void PostPurhcaseOrder_ReturnsOrderResponse()
        {
            var response = Client.Orders.Post<IPurchaseOrder>(x =>
            {
                x.Description = "Blob";
                x.Items.Add(new OrderItemForm { Name = "Lola" });
            });
            Assert.That(response, Is.InstanceOf<IOrderResponse>());
            Assert.That(response.Order, Is.Not.Null);
            Assert.That(response.Order, Is.TypeOf<PurchaseOrderResource>());
            Assert.That(response.Order.Description, Is.EqualTo("Blob"));
        }


        [Test]
        public void PostResourceWithEnum()
        {
            var hasCustomEnum = Client.HasCustomEnums.Post(x => x.TheEnumValue = CustomEnum.Tock);
            Assert.That(hasCustomEnum, Is.EqualTo(hasCustomEnum));
        }


        [Test]
        public void PostStringToObjectDictionaryContainer_WithItemSetInDictionary()
        {
            var form = new StringToObjectDictionaryContainerForm
            {
                Map = { { "TheString", "hello" }, { "TheInt", 1337 } }
            };

            var resource = Client.StringToObjectDictionaryContainers.Post(form);

            Assert.That(resource.Map["TheString"], Is.EqualTo("hello"));
            Assert.That(resource.Map["TheInt"], Is.EqualTo(1337));
        }


        [Test]
        public void PostThingWithNullableDateTime_WithNonNullValue()
        {
            DateTime? maybeDateTime = new DateTime(2011, 10, 22, 1, 33, 22);
            var response = Client.ThingWithNullableDateTimes.Post(
                x => { x.MaybeDateTime = maybeDateTime; });

            Assert.That(response.MaybeDateTime.HasValue, Is.True);
            Assert.That(response.MaybeDateTime, Is.EqualTo(maybeDateTime));
        }


        [Test]
        public void PostThingWithNullableDateTime_WithNullValue()
        {
            var response = Client.ThingWithNullableDateTimes.Post(
                x => { x.MaybeDateTime = null; });

            Assert.That(response.MaybeDateTime.HasValue, Is.False);
        }


        [Test]
        public void PostThingWithPropertyNamedUri()
        {
            var uri = new Uri("http://bahaha");
            var response = Client.ThingWithPropertyNamedUris.Post(
                x => { x.Uri = uri; });

            Assert.That(response.Uri, Is.EqualTo(uri));
        }


        [Test]
        public void PostThing_IdentifiedByGuid_IsSuccessful()
        {
            var guidThing = Client.GuidThings.Post(new GuidThingForm());
            var guid = guidThing.Id;
            var reloadedThing = Client.Reload(guidThing);
            Assert.That(reloadedThing.Id, Is.EqualTo(guid));
        }


        [Test]
        public void PostToReadOnlyAttributesProperty()
        {
            var o =
                Client.HasReadOnlyDictionaryProperties.Post(new HasReadOnlyDictionaryPropertyForm
                {
                    Map = { { "blah", "hah" } }
                });

            Assert.That(o.Map["blah"], Is.EqualTo("hah"));
        }


        [Test]
        public void PostUnpostableThingOnServer_ThatIsOnlyUnpostableServerSide_ThrowsInvalidOperationException()
        {
            // UnpostableThingOnServer has been modified in GenerateClientDllApp to appear postable in client dll,
            // but should still not be postable on server. This is done to test server validation of posting rules.

            var ex = Assert.Throws<WebClientException>(() => Client.UnpostableThingsOnServer.Post(x => x.FooBar = "moo"));
            Assert.That(ex.Message, Is.EqualTo("MethodNotAllowed: Method POST not allowed!"));
            Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
        }


        [Test]
        public void PostUnpostableThing_ThrowsInvalidOperationException()
        {
            var ex =
                Assert.Throws<InvalidOperationException>(
                    () =>
                        ((IPostableRepository<IUnpostableThing, IUnpostableThing>)Client.UnpostableThings).Post(
                            x => x.FooBar = "moo"));
            Assert.That(ex.Message, Is.EqualTo("Method POST is not allowed for uri."));
        }


        [Test]
        public void PostUsingOverloadTakingFormObject()
        {
            const string critterName = "Lonely critter boy";
            var critterForm = new CritterForm
            {
                Name = critterName
            };

            var critterResource = Client.Critters.Post(critterForm);
            Assert.That(critterResource.Name, Is.EqualTo(critterName));
        }


        [Test]
        public void PostWeaponWithRequiredPropertyNotSet_ThrowsBadRequestException()
        {
            // Model is required, so an exception should be thrown.
            var ex =
                Assert.Throws<BadRequestException<IErrorStatus>>(
                    () => Client.Weapons.Post(new WeaponForm { Price = 12345 }));
            Assert.That(ex.Body, Is.Not.Null);
            Assert.That(ex.Body.Member, Is.EqualTo("Model"));
        }
    }
}