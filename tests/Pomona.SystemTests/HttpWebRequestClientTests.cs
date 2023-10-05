#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;
using Pomona.Example.Models;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class HttpWebRequestClientTests : ClientTestsBase
    {
        public override bool UseSelfHostedHttpServer => true;


        [Test]
        public void Get_CritterAtNonExistingUrl_ThrowsWebClientException()
        {
            Assert.Throws<Common.Web.ResourceNotFoundException>(
                () => Client.Get<ICritter>(Client.BaseUri + "critters/38473833"));
        }


        [Test]
        public void Get_UsingQuery_ReturnsEntities()
        {
            // Here we just expect to get something returned, Query itself is tested in other fixture,
            var oddCritters = Client.Critters.Query().Where(x => x.Id % 2 == 1).ToList();
            Assert.That(oddCritters, Has.Count.GreaterThan(0));
        }


        [Test]
        public void Patch_EtaggedEntity_WithCorrectEtag_UpdatesEntity()
        {
            var etaggedEntity = Save(new EtaggedEntity { Info = "Ancient" });
            var originalResource = Client.EtaggedEntities.Query<IEtaggedEntity>().First(x => x.Id == etaggedEntity.Id);
            var updatedResource = Client.EtaggedEntities.Patch(originalResource, x => x.Info = "Fresh");
            Assert.That(updatedResource.Info, Is.EqualTo("Fresh"));
            Assert.That(updatedResource.ETag, Is.Not.EqualTo(originalResource.ETag));
        }


        [Test]
        public void Patch_EtaggedEntity_WithIncorrectEtag_ThrowsException()
        {
            var etaggedEntity = Save(new EtaggedEntity { Info = "Ancient" });
            var originalResource = Client.EtaggedEntities.Query<IEtaggedEntity>().First(x => x.Id == etaggedEntity.Id);

            // Change etag on entity, which should give an exception
            etaggedEntity.SetEtag("MODIFIED!");

            Assert.That(() => Client.EtaggedEntities.Patch(originalResource, x => x.Info = "Fresh"),
                        Throws.TypeOf<PreconditionFailedException>());
            Assert.That(etaggedEntity.Info, Is.EqualTo("Ancient"));
        }


        [Test]
        public void Post_FailingThing_ThrowsWebClientException()
        {
            var ex =
                Assert.Throws<WebClientException<IErrorStatus>>(() => Client.FailingThings.Post(new FailingThingForm()));
            Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }


        [Test]
        public void Post_SavesEntityAndReturnsResource()
        {
            var resource = Client.Critters.Post(new CritterForm { Name = "Hiihaa" });
            Assert.That(resource.Id, Is.GreaterThan(0));
            Assert.That(resource.Name, Is.EqualTo("Hiihaa"));
        }


        [Category("TODO")]
        [Test(
            Description =
                "Encoding of some identifiers (?&/) is not working properly, this is due to behaviour in Nancy hosts. Maybe we need custom encoding?"
            )]
        public void QueryGalaxyHavingQuestionMarkInName_ReturnsCorrectResource()
        {
            var galaxy = Client.Galaxies.Post(new GalaxyForm() { Name = "The Joker?" });
            Assert.That(galaxy.Name, Is.EqualTo("The Joker?"));
            galaxy = Client.Reload(galaxy);
            Assert.That(galaxy.Name, Is.EqualTo("The Joker?"));
        }


        [Test]
        public void Synchronous_client_call_from_async_context_using_exclusive_scheduler_is_not_deadlocked()
        {
            // Note that this will probably leave a hanging worker thread if failing :/
            // Not sure how to clean that up properly, might have to implement a custom TaskScheduler to do that
            var ccsp = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, 1);
            var tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskContinuationOptions.None,
                                     ccsp.ExclusiveScheduler);
            // Should have good time to complete in 10seconds, on any computer
            var hasCompleted = tf.StartNew(async () =>
            {
                // Just do some random sync client call
                Client.Critters.Post(new CritterForm() { Name = "hi" });
                await Task.Yield();
                Client.Critters.Post(new CritterForm() { Name = "again" });
            }).Wait(10000);
            Assert.That(hasCompleted, Is.True, "Task never completed, which means it's probably deadlocked.");
        }
    }
}
