#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common.Linq;
using Pomona.TestingClient;

namespace Pomona.SystemTests.TestingClient
{
    [TestFixture]
    public class TestingClientTests
    {
        private TestableClientProxyBase mockControl;
        private ICritterClient testClient;


        [Test]
        public void Get_Critter_ById()
        {
            var critterResource = new CritterResource() { Name = "donald" };
            this.mockControl.Save(critterResource);
            var critter = this.testClient.Critters.Get(critterResource.Id);
            Assert.That(critter, Is.Not.Null);
        }


        [Test]
        public void GetLazy_ForACritter_ReturnsTheCritter()
        {
            var donald = new CritterResource() { Name = "donald" };
            this.mockControl.Save(donald);
            // For testable client we don't actually get things lazy.
            // This will make behaviour different than real client for resources that don't exist.
            var donaldIsLazy = this.testClient.Critters.GetLazy(donald.Id);
            Assert.That(donaldIsLazy, Is.EqualTo(donald));
        }


        [Test]
        public void Query_AllCritters_NoCrittersInStore_ReturnsEmptyList()
        {
            var emptyCritterResults = this.testClient.Critters.Query().ToList();
            Assert.That(emptyCritterResults, Is.Empty);
        }


        [Test]
        public void Query_AllCritters_ReturnsSomeCritters()
        {
            this.mockControl.Save(new CritterResource() { Name = "donald" });
            var emptyCritterResults = this.testClient.Critters.Query().ToList();
            Assert.That(emptyCritterResults, Has.Count.EqualTo(1));
        }


        [Test]
        public void Query_AllCritters_WithExpand_ReturnsSomeCritters()
        {
            this.mockControl.Save(new CritterResource() { Name = "donald" });
            var emptyCritterResults =
                this.testClient.Critters.Query().Where(x => x.Name == "donald").Expand(x => x.Farm).ToList();
            Assert.That(emptyCritterResults, Has.Count.EqualTo(1));
        }


        [Test]
        public void Query_CustomClientSideCritter_NoCrittersInStore_ReturnsEmptyList()
        {
            var emptyCritterResults = this.testClient.Critters.Query<ICustomCritter>().ToList();
            Assert.That(emptyCritterResults, Is.Empty);
        }


        [Test]
        public void Query_CustomClientSideEntityWithChildRepository_ReturnsSomeCritters()
        {
            this.mockControl.Save(new GalaxyResource() { Name = "donald" });
            var queryable = this.testClient.Galaxies.Query<ICustomGalaxy>();
            var emptyCritterResults = queryable.ToList();
            Assert.That(emptyCritterResults, Has.Count.EqualTo(1));
        }


        [Test]
        public void Query_EntityWithChildRepository_ReturnsSomeCritters()
        {
            this.mockControl.Save(new GalaxyResource() { Name = "donald" });
            var emptyCritterResults = this.testClient.Galaxies.Query().ToList();
            Assert.That(emptyCritterResults, Has.Count.EqualTo(1));
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.testClient = TestableClientGenerator.CreateClient<ICritterClient>();
            this.mockControl = (TestableClientProxyBase)this.testClient;
        }

        #endregion

        public interface ICustomCritter : ICritter
        {
        }

        public interface ICustomGalaxy : IGalaxy
        {
        }
    }
}

