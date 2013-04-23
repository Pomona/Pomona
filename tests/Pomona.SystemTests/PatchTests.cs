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

using System.Linq;
using Critters.Client;
using NUnit.Framework;
using Pomona.Common.Linq;
using Pomona.Example.Models;
using Pomona.SystemTests.Linq;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class PatchTests : ClientTestsBase
    {
        [Test]
        public void PatchCritter_AddNewFormToList()
        {
            var critter = new Critter();
            critter.Weapons.Add(new Gun(critter, new WeaponModel {Name = "ExistingWeaponModel"}));
            Save(critter);

            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x => x.Weapons.Add(new WeaponForm
                             {
                                 Price = 3.4m,
                                 Model = new WeaponModelForm {Name = "balala"},
                                 Strength = 3.5
                             }));

            Assert.That(critter.Weapons, Has.Count.EqualTo(2));
        }

        [Test]
        public void PatchCritter_UpdatePropertyOfValueObject()
        {
            var critter = Save(new Critter());
            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.CrazyValue.Sickness = "Just crazy thats all");

            Assert.That(critter.CrazyValue.Sickness, Is.EqualTo("Just crazy thats all"));
        }

        [Test]
        public void PatchCritter_UpdateReferenceProperty_UsingValueFromFirstLazyMethod()
        {
            var hat = Save(new Hat {Style = "Gangnam Style 1234"});
            var critter = Save(new Critter());
            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.Hat = client.Query<IHat>().Where(y => y.Style == "Gangnam Style 1234").FirstLazy());

            Assert.That(critter.Hat, Is.EqualTo(hat));
        }

        [Test]
        public void PatchCritter_UpdateStringProperty()
        {
            var critter = Save(new Critter());
            var resource = client.Query<ICritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.Name = "NewName");

            Assert.That(critter.Name, Is.EqualTo("NewName"));
        }

        [Test]
        public void PatchCustomClientSideResource_SetAttribute_UpdatesAttribute()
        {
            var entity = new StringToObjectDictionaryContainer
                {
                    Map = {{"Text", "testtest"}}
                };
            Save(entity);

            var resource = client.Query<LinqQueryTests.ICustomTestEntity3>().First(x => x.Id == entity.Id);

            var patchedResource =
                client.Patch(resource, x => { x.Text = "UPDATED!"; });


            Assert.That(patchedResource.Text, Is.EqualTo("UPDATED!"));
        }

        [Test]
        public void PatchMusicalInheritedCritter_UpdateProperty()
        {
            var critter = Save(new MusicalCritter());
            var resource = client.Query<IMusicalCritter>().First(x => x.Id == critter.Id);
            client.Patch(resource,
                         x =>
                         x.BandName = "The Patched Sheeps");

            Assert.That(critter.BandName, Is.EqualTo("The Patched Sheeps"));
        }

        [Test]
        public void Patch_EtaggedEntity_WithCorrectEtag_UpdatesEntity()
        {
            var etaggedEntity = Save(new EtaggedEntity {Info = "Ancient"});
            var originalResource = client.EtaggedEntities.Query<IEtaggedEntity>().First(x => x.Id == etaggedEntity.Id);
            var updatedResource = client.EtaggedEntities.Patch(originalResource, x => x.Info = "Fresh");
            Assert.That(updatedResource.Info, Is.EqualTo("Fresh"));
            Assert.That(updatedResource.ETag, Is.Not.EqualTo(originalResource.ETag));
        }

        [Category("TODO")]
        [Test]
        public void Patch_EtaggedEntity_WithIncorrectEtag_ThrowsException()
        {
            var etaggedEntity = Save(new EtaggedEntity {Info = "Ancient"});
            var originalResource = client.EtaggedEntities.Query<IEtaggedEntity>().First(x => x.Id == etaggedEntity.Id);

            // Change etag on entity, which should give an exception
            etaggedEntity.ETag = "MODIFIED!";

            Assert.That(() => client.EtaggedEntities.Patch(originalResource, x => x.Info = "Fresh"), Throws.Exception);
            Assert.That(etaggedEntity.Info, Is.EqualTo("Ancient"));

            Assert.Fail("Missing support for throwing good client side exceptions.");
        }
    }
}