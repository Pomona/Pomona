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

using Critters.Client;

using NUnit.Framework;

using Pomona.Example.Models;

namespace Pomona.SystemTests.Handlers
{
    [TestFixture]
    public class ResourceHandlerTests : ClientTestsBase
    {
        [Test]
        public void DeleteHandledThing_CallsHandleDeleteMethod()
        {
            var entity = Save(new HandledThing() { Foo = "to be exterminated." });
            Client.HandledThings.Delete(Client.HandledThings.GetLazy(entity.Id));
            Assert.That(Repository.List<HandledThing>(), Is.Not.Contains(entity));
        }


        [Test]
        public void GetHandledThing_CallsHandlerGetMethod()
        {
            var thingEntity = Save(new HandledThing() { Foo = "blabla" });
            Assert.That(thingEntity.FetchedCounter, Is.EqualTo(0));
            var thingResource = Client.HandledThings.Get(thingEntity.Id);
            Assert.That(thingResource.Id, Is.EqualTo(thingEntity.Id));
            Assert.That(thingResource.FetchedCounter, Is.EqualTo(1));
            Assert.That(thingEntity.FetchedCounter, Is.EqualTo(1));
        }


        [Category("TODO")]
        [Test]
        public void PatchHandledThing_CallsHandlerPatchMethod()
        {
            Assert.Fail("NOT YET IMPLEMENTED");
        }


        [Category("TODO")]
        [Test]
        public void PostFormToHandledThing_CallsHandlerPostToResourceMethod()
        {
            Assert.Fail("NOT YET IMPLEMENTED");
        }


        [Test]
        public void PostHandledThing_CallsHandlerPostMethod()
        {
            var resource = Client.HandledThings.Post(new HandledThingForm() { Foo = "lalala" });
            Assert.That(resource.Marker, Is.EqualTo("HANDLER WAS HERE!"));
        }


        [Test]
        public void PostUnhandledThing_HasNoMatchingHandlerAndIsPassedToDataSourceHandlerByDefault()
        {
            var resource = Client.UnhandledThings.Post(new UnhandledThingForm() { Bah = "klsjklj" });
            Assert.That(resource, Is.Not.Null);
            Assert.That(resource.Id, Is.GreaterThan(0));
        }
    }
}