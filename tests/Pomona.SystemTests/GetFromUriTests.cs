#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Critters.Client;

using NUnit.Framework;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class GetFromUriTests : ClientTestsBase
    {
        [Test]
        public void GetFromUriWithSpacesInIdentifier_ReturnsResource()
        {
            Client.Galaxies.Post(new GalaxyForm() { Name = "test_I will not buy this record it is scratched" });
            var galaxy = Client.Galaxies.Get("test_I will not buy this record it is scratched");
            Assert.That(galaxy, Is.Not.EqualTo(null));
            Assert.That(galaxy.Name, Is.EqualTo("test_I will not buy this record it is scratched"));
        }
    }
}