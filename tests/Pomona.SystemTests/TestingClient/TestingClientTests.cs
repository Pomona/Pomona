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

using System.Linq;

using Critters;
using Critters.Client;

using NUnit.Framework;

using Pomona.Common.Linq;
using Pomona.TestingClient;

namespace Pomona.SystemTests.TestingClient
{
    [TestFixture]
    public class TestingClientTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.testClient = TestableClientGenerator.CreateClient<ICritterClient>();
            this.mockControl = (TestableClientProxyBase)this.testClient;
        }

        #endregion

        private ICritterClient testClient;
        private TestableClientProxyBase mockControl;

        public interface ICustomCritter : ICritter
        {
        }

        public interface ICustomGalaxy : IGalaxy
        {
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
        public void Get_Critter_ById()
        {
            var critterResource = new CritterResource() { Name = "donald" };
            this.mockControl.Save(critterResource);
            var critter = this.testClient.Critters.Get(critterResource.Id);
            Assert.That(critter, Is.Not.Null);
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
    }
}