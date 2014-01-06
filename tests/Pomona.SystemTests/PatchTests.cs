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
using System.Collections.Generic;
using System.Linq;
using Critters.Client;
using NUnit.Framework;
using Pomona.Common;
using Pomona.Common.Linq;
using Pomona.Common.Web;
using Pomona.Example.Models;
using Pomona.Common.Internals;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class PatchTests : ClientTestsBase
    {
        public void PatchProtectedProperty_ThrowsBadRequestException_AndDoesNotAllowChangeOfProtectedProperty()
        {
            var critter = Save(new Critter());
            var protectedValue = critter.Protected;
            var c = Client.Critters.Query(x => x.Id == critter.Id).First();
            var ex =
                Assert.Throws<BadRequestException<IErrorStatus>>(
                    () => Client.Critters.Patch(c, p => p.Protected = "HALLA MALLA NALLA"));

            Assert.That(ex.Body, Is.Not.Null);
            Assert.That(ex.Body.Member, Is.EqualTo("Protected"));
            Assert.That(critter.Protected, Is.EqualTo(protectedValue));
        }


        [Test]
        public void PatchCritter_AddNewFormToList()
        {
            var critter = new Critter();
            critter.Weapons.Add(new Gun(new WeaponModel {Name = "ExistingWeaponModel"}));
            Save(critter);

            var resource = Client.Query<ICritter>().First(x => x.Id == critter.Id);
            Client.Patch(resource,
                         x => x.Weapons.Add(new WeaponForm
                             {
                                 Price = 3.4m,
                                 Model = new WeaponModelForm {Name = "balala"},
                                 Strength = 3.5
                             }));

            Assert.That(critter.Weapons, Has.Count.EqualTo(2));
        }

        [Test]
        public void PatchStringToStringDictionary_SetValueForKey()
        {
            var result = Client.Patch(CreateDictionaryResource(), d => d.Map.Add("foo", "bar"));
            Assert.That(result.Map, Contains.Item(new KeyValuePair<string, string>("foo", "bar")));
        }

        [Test]
        public void PatchStringToStringDictionary_RemoveKey()
        {
            var result = Client.Patch(CreateDictionaryResource(new [] {new KeyValuePair<string, string>("foo", "bar"), }), d => d.Map.Remove("foo"));
            Assert.That(result.Map, Is.Not.Contains(new KeyValuePair<string, string>("foo", "bar")));
        }

        private IDictionaryContainer CreateDictionaryResource(IEnumerable<KeyValuePair<string, string>> items = null)
        {
            var dictContainer = new DictionaryContainer();
            foreach (var kvp in items.EmptyIfNull())
            {
                dictContainer.Map.Add(kvp);
            }
            Save(dictContainer);
            var dictResource = Client.DictionaryContainers.Get(dictContainer.Id);
            return dictResource;
        }

        [Test]
        public void PatchCritter_ModifyWeapon()
        {
            var critter = Client.Critters.Get(Repository.CreateRandomCritter().Id);
            var TMPweapons = critter.Weapons.ToList(); // REMOVE THIS
            var response = Client.Patch(critter, x => x.Weapons.First().Strength = 1337, o => o.Expand(x => x.Weapons));
            Assert.That(response.Weapons.First().Strength, Is.EqualTo(1337.0));
        }

        [Test]
        public void PatchCritter_RemoveWeapon()
        {
            var critter = Client.Critters.Get(Repository.CreateRandomCritter().Id);
            Assert.That(critter.Weapons.Count, Is.GreaterThan(0));
            var response = Client.Patch(critter, x => x.Weapons.Clear(), o => o.Expand(x => x.Weapons));
            Assert.That(response.Weapons.Count, Is.EqualTo(0));
        }

        [Test]
        public void PatchCritter_SetWriteOnlyProperty()
        {
            var critter = Repository.CreateRandomCritter();
            var resource = Client.Critters.Query(x => x.Id == critter.Id).First();

            Client.Patch(resource, x => x.Password = "NewPassword");

            Assert.That(critter.Password, Is.EqualTo("NewPassword"));
        }

        [Test]
        public void PatchCritter_UpdatePropertyOfValueObject()
        {
            var critter = Save(new Critter());
            var resource = Client.Query<ICritter>().First(x => x.Id == critter.Id);
            Client.Patch(resource,
                         x =>
                         x.CrazyValue = new CrazyValueObjectForm {Sickness = "Just crazy thats all"});

            Assert.That(critter.CrazyValue.Sickness, Is.EqualTo("Just crazy thats all"));
        }

        [Test]
        public void PatchCritter_UpdateReferenceProperty_UsingValueFromFirstLazyMethod()
        {
            var hat = Save(new Hat {Style = "Gangnam Style 1234"});
            var critter = Save(new Critter());
            var resource = Client.Query<ICritter>().First(x => x.Id == critter.Id);
            Client.Patch(resource,
                         x =>
                         x.Hat = Client.Query<IHat>().Where(y => y.Style == "Gangnam Style 1234").FirstLazy());

            Assert.That(critter.Hat, Is.EqualTo(hat));
        }

        [Test]
        public void PatchCritter_UpdateStringProperty()
        {
            var critter = Save(new Critter());
            var resource = Client.Query<ICritter>().First(x => x.Id == critter.Id);
            Client.Patch(resource,
                         x =>
                         x.Name = "NewName");

            Assert.That(critter.Name, Is.EqualTo("NewName"));
        }

        [Test]
        public void PatchCritter_WithPatchOptionExpandWeapons_ExpandsWeapons()
        {
            var critter = new Critter();
            critter.Weapons.Add(new Gun(new WeaponModel {Name = "ExistingWeaponModel"}));
            Save(critter);

            var resource = Client.Query<ICritter>().First(x => x.Id == critter.Id);
            var patchResponse = Client.Patch(resource,
                                             x => x.Weapons.Add(new WeaponForm
                                                 {
                                                     Price = 3.4m,
                                                     Model = new WeaponModelForm {Name = "balala"},
                                                     Strength = 3.5
                                                 }), o => o.Expand(x => x.Weapons));


            Assert.That(patchResponse.Weapons.IsLoaded());
        }

        [Test]
        public void PatchMusicalInheritedCritter_UpdateProperty()
        {
            var critter = Save(new MusicalCritter("lalala"));
            var resource = Client.Query<IMusicalCritter>().First(x => x.Id == critter.Id);
            Client.Patch(resource,
                         x =>
                         x.BandName = "The Patched Sheeps");

            Assert.That(critter.BandName, Is.EqualTo("The Patched Sheeps"));
        }

        [Test]
        public void PatchUnpatchableThing_ThrowsInvalidOperationException()
        {
            var resource = Client.UnpatchableThings.Post(x => x.FooBar = "haha");
            var ex =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    ((IPatchableRepository<IUnpatchableThing>) Client.UnpatchableThings).Patch(resource,
                                                                                               x => x.FooBar = "moo"));
            Assert.That(ex.Message, Is.EqualTo("Method PATCH is not allowed for uri."));
        }

        [Category("TODO")]
        [Test]
        public void Patch_RemoveItemFromCollectionWhereKeyTypeIsString_IsSuccessful()
        {
            Assert.Fail("Known to not be working yet, putting here as a reminder.");
        }

        [Test]
        public void Patch_EtaggedEntity_WithCorrectEtag_UpdatesEntity()
        {
            var etaggedEntity = Save(new EtaggedEntity {Info = "Ancient"});
            var originalResource = Client.EtaggedEntities.Query<IEtaggedEntity>().First(x => x.Id == etaggedEntity.Id);
            var updatedResource = Client.EtaggedEntities.Patch(originalResource, x => x.Info = "Fresh");
            Assert.That(updatedResource.Info, Is.EqualTo("Fresh"));
            Assert.That(updatedResource.ETag, Is.Not.EqualTo(originalResource.ETag));
        }

        [Test]
        public void Patch_EtaggedEntity_WithIncorrectEtag_ThrowsException()
        {
            var etaggedEntity = Save(new EtaggedEntity {Info = "Ancient"});
            var originalResource = Client.EtaggedEntities.Query<IEtaggedEntity>().First(x => x.Id == etaggedEntity.Id);

            // Change etag on entity, which should give an exception
            etaggedEntity.SetEtag("MODIFIED!");

            Assert.That(() => Client.EtaggedEntities.Patch(originalResource, x => x.Info = "Fresh"),
                        Throws.TypeOf<PreconditionFailedException>());
            Assert.That(etaggedEntity.Info, Is.EqualTo("Ancient"));
        }
    }
}