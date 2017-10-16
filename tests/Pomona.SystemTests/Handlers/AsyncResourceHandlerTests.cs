#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.Example.Rules;

namespace Pomona.SystemTests.Handlers
{
    [TestFixture]
    public class AsyncResourceHandlerTests : ClientTestsBase<AsyncResourceHandlerTests.BootstrapperWithAsyncHandlers>
    {
        [Test]
        public void DeleteHandledThing_ById_CallsHandleDeleteMethod()
        {
            var entity = Save(new HandledThing() { Foo = "to be exterminated." });
            Client.HandledThings.Delete(entity.Id);
            Assert.That(Repository.List<HandledThing>(), Is.Not.Contains(entity));
        }


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


        [Test]
        public void PatchHandledThing_CallsHandlerPatchMethod()
        {
            var thingEntity = Save(new HandledThing() { Foo = "blabla" });
            Assert.That(thingEntity.PatchCounter, Is.EqualTo(0));
            var thingResource = Client.HandledThings.Patch(Client.HandledThings.GetLazy(thingEntity.Id),
                                                           p => p.Marker = "dudida");
            Assert.That(thingResource.PatchCounter, Is.EqualTo(1));
            Assert.That(thingEntity.PatchCounter, Is.EqualTo(1));
            Assert.That(thingResource.Marker, Is.EqualTo("dudida"));
        }


        [Test]
        public void PatchSingleChildOfHandledThing_CallsPatchHandlerForChild()
        {
            var thingEntity = Save(new HandledThing() { Foo = "blabla" });
            var thingResource = Client.HandledThings.Get(thingEntity.Id);
            var patchedChild = Client.Patch(thingResource.SingleChild, c => c.Name = "Renamed");
            Assert.That(patchedChild.Name, Is.EqualTo("Renamed"));
            Assert.That(patchedChild.PatchHandlerCalled, Is.True);
        }


        [Test]
        public void PostChildToHandledThing_CallsPostHandlerMethodWithParent()
        {
            var thingEntity = Save(new HandledThing() { Foo = "blabla" });
            var thingResource = Client.HandledThings.Get(thingEntity.Id);
            var childResource = thingResource.Children.Post(new HandledChildForm() { Toy = "rattle snake" });
            Assert.That(childResource.HandlerWasCalled, Is.True, "It doesn't seem like handler has been called.");
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


        [Test]
        public void QueryHandledThing_CallsHandlerQueryMethod()
        {
            var thingEntity = Save(new HandledThing() { Foo = "blabla" });
            Assert.That(thingEntity.FetchedCounter, Is.EqualTo(0));
            var thingResource = Client.HandledThings.Query().First(x => x.Foo == "blabla");
            Assert.That(thingResource.Id, Is.EqualTo(thingEntity.Id));
            Assert.That(thingResource.QueryCounter, Is.EqualTo(1));
        }


        public class BootstrapperWithAsyncHandlers : CritterBootstrapper
        {
            public BootstrapperWithAsyncHandlers()
                : base(configuration : new ConfigurationWithAsyncHandlers())
            {
            }
        }

        class ConfigurationWithAsyncHandlers : CritterPomonaConfiguration
        {
            public override IEnumerable<object> FluentRuleObjects
            {
                get
                {
                    var fluentRuleObjects =
                        base.FluentRuleObjects.Where(x => !(x is HandledThingRules)).Append(new AsyncHandledThingRules()).ToList();
                    return fluentRuleObjects;
                }
            }
        }
    }
}