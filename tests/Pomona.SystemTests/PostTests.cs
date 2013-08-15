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
using System.Linq;
using Critters.Client;
using NUnit.Framework;
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
        public void PostCritterWithExistingHat()
        {
            const string hatType = "Old";

            var hat = PostAHat(hatType);

            const string critterName = "Super critter";

            var critter = (ICritter) client.Post<ICritter>(
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

            var critter = (ICritter) client.Post<ICritter>(
                x =>
                    {
                        x.Hat = client.Hats.Query().Where(y => y.HatType.StartsWith("Special")).FirstLazy();
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

            var critter = (ICritter) client.Post<ICritter>(
                x =>
                    {
                        x.Hat = new HatForm {HatType = hatType};
                        x.Name = critterName;
                    });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
        }

        [Test]
        public void PostCritterWithNameTooLong_ThrowsExceptionWithErrorStatus()
        {
            var form = new CritterForm {Name = string.Join(" ", Enumerable.Repeat("John", 50))};
            var exception = Assert.Throws<BadRequestException<IErrorStatus>>(() => client.Critters.Post(form));
            Assert.That(exception.Body, Is.Not.Null);
            Assert.That(exception.Body.Message, Is.EqualTo("Critter can't have name longer than 50 characters."));
            Assert.That(exception.Body.ErrorCode, Is.EqualTo(1337));
        }

        [Test]
        public void PostDictionaryContainer_WithItemSetInDictionary()
        {
            var response = (IDictionaryContainer) client.Post<IDictionaryContainer>(x => { x.Map["cow"] = "moo"; });
            Assert.That(response.Map.ContainsKey("cow"));
            Assert.That(response.Map["cow"] == "moo");
        }

        [Test]
        public void PostEntityWithReadOnlyPropertySetThroughConstructor()
        {
            var o =
                client.HasConstructorInitializedReadOnlyProperties.Post(
                    x => x.Crazy = new CrazyValueObjectForm {Info = "bam!", Sickness = "adhd"});
            Assert.That(o.Crazy.Info, Is.EqualTo("bam!"));
            Assert.That(o.Crazy.Sickness, Is.EqualTo("adhd"));
        }


        [Test]
        public void PostHasCustomEnum()
        {
            var response = client.HasCustomEnums.Post(
                x => { x.TheEnumValue = CustomEnum.Tock; });

            Assert.That(response.TheEnumValue, Is.EqualTo(CustomEnum.Tock));
        }


        [Test]
        public void PostHasNullableCustomEnum_WithNonNullValue()
        {
            var response = client.HasCustomNullableEnums.Post(
                x => { x.TheEnumValue = CustomEnum.Tock; });
            Assert.That(response.TheEnumValue.HasValue, Is.True);
            Assert.That(response.TheEnumValue.Value, Is.EqualTo(CustomEnum.Tock));
        }


        [Test]
        public void PostHasNullableCustomEnum_WithNull()
        {
            var response = client.HasCustomNullableEnums.Post(
                x => { x.TheEnumValue = null; });
            Assert.That(response.TheEnumValue.HasValue, Is.False);
        }


        [Test]
        public void PostJunkWithRenamedProperty()
        {
            var propval = "Jalla jalla";
            var junk =
                (IJunkWithRenamedProperty)
                client.Post<IJunkWithRenamedProperty>(x => { x.BeautifulAndExposed = propval; });

            Assert.That(junk.BeautifulAndExposed, Is.EqualTo(propval));
        }

        [Test]
        public void PostMusicalCritter()
        {
            const string critterName = "Nooob critter";
            const string hatType = "Bolalalala";

            var critter = (IMusicalCritter) client.Post<IMusicalCritter>(
                x =>
                    {
                        x.Hat = new HatForm {HatType = hatType};
                        x.Name = critterName;
                        x.BandName = "banana";
                        x.Instrument = new InstrumentForm {Type = "helo"};
                    });

            Assert.That(critter.Name, Is.EqualTo(critterName));
            Assert.That(critter.Hat.HatType, Is.EqualTo(hatType));
            Assert.That(critter.BandName, Is.EqualTo("banana"));

            Assert.That(DataStore.List<Critter>().Any(x => x.Id == critter.Id && x is MusicalCritter));
        }

        [Test]
        public void PostOrderWithItems()
        {
            var orderResponse = client.Orders.Post(new OrderForm {Items = {new OrderItemForm {Name = "blah"}}});
            Assert.That(orderResponse.Order.Items, Has.Count.EqualTo(1));
            Assert.That(orderResponse.Order.Items[0].Name, Is.EqualTo("blah"));
        }


        [Test]
        public void PostOrder_ReturnsOrderResponse()
        {
            var response = client.Orders.Post(x =>
                {
                    x.Description = "Blob";
                    x.Items.Add(new OrderItemForm {Name = "Lola"});
                });
            Assert.That(response, Is.InstanceOf<IOrderResponse>());
            Assert.That(response.Order, Is.Not.Null);
            Assert.That(response.Order, Is.TypeOf<OrderResource>());
            Assert.That(response.Order.Description, Is.EqualTo("Blob"));
        }

        [Test]
        public void PostResourceWithEnum()
        {
            var hasCustomEnum = client.HasCustomEnums.Post(x => x.TheEnumValue = CustomEnum.Tock);
            Assert.That(hasCustomEnum, Is.EqualTo(hasCustomEnum));
        }

        [Test]
        public void PostStringToObjectDictionaryContainer_WithItemSetInDictionary()
        {
            var form = new StringToObjectDictionaryContainerForm
                {
                    Map = {{"TheString", "hello"}, {"TheInt", 1337}}
                };

            var resource = client.StringToObjectDictionaryContainers.Post(form);

            Assert.That(resource.Map["TheString"], Is.EqualTo("hello"));
            Assert.That(resource.Map["TheInt"], Is.EqualTo(1337));
        }


        [Test]
        public void PostThingWithNullableDateTime_WithNonNullValue()
        {
            DateTime? maybeDateTime = new DateTime(2011, 10, 22, 1, 33, 22);
            var response = client.ThingWithNullableDateTimes.Post(
                x => { x.MaybeDateTime = maybeDateTime; });

            Assert.That(response.MaybeDateTime.HasValue, Is.True);
            Assert.That(response.MaybeDateTime, Is.EqualTo(maybeDateTime));
        }


        [Test]
        public void PostThingWithNullableDateTime_WithNullValue()
        {
            var response = client.ThingWithNullableDateTimes.Post(
                x => { x.MaybeDateTime = null; });

            Assert.That(response.MaybeDateTime.HasValue, Is.False);
        }


        [Test]
        public void PostThingWithPropertyNamedUri()
        {
            var uri = new Uri("http://bahaha");
            var response = client.ThingWithPropertyNamedUris.Post(
                x => { x.Uri = uri; });

            Assert.That(response.Uri, Is.EqualTo(uri));
        }

        [Test]
        public void PostToReadOnlyAttributesProperty()
        {
            var o =
                client.HasReadOnlyDictionaryProperties.Post(new HasReadOnlyDictionaryPropertyForm
                    {
                        Map = {{"blah", "hah"}}
                    });

            Assert.That(o.Map["blah"], Is.EqualTo("hah"));
        }

        [Test]
        public void PostUsingOverloadTakingFormObject()
        {
            const string critterName = "Lonely critter boy";
            var critterForm = new CritterForm
                {
                    Name = critterName
                };

            var critterResource = client.Critters.Post(critterForm);
            Assert.That(critterResource.Name, Is.EqualTo(critterName));
        }

        [Test]
        public void PostWeaponWithOptionalPropertyNotSet_DoesNotThrowException()
        {
            // Model is required, so an exception should be thrown.
            Assert.That(() => client.Loners.Post(new LonerForm {Name = "blah", Strength = 123}), Throws.Nothing);
        }

        [Test]
        public void PostWeaponWithRequiredPropertyNotSet_ThrowsBadRequestException()
        {
            // Model is required, so an exception should be thrown.
            var ex =
                Assert.Throws<BadRequestException<IErrorStatus>>(
                    () => client.Weapons.Post(new WeaponForm {Price = 12345}));
            Assert.That(ex.Body, Is.Not.Null);
            Assert.That(ex.Body.Member, Is.EqualTo("Model"));
        }
    }
}