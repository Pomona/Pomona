#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using NSubstitute;

using NUnit.Framework;

using Pomona.Example.Models;
using Pomona.Security.Authentication;

namespace Pomona.UnitTests.Security.Authentication
{
    [TestFixture]
    public class DefaultPreAuthenticatedUriResolverTests
    {
        [Test]
        public void GetPreAuthenticatedUriFor_ReturnsPreAuthenticatedUriForResource()
        {
            // Very much mocked, just check that GetPreAuthenticatedUriFor actually calls IUriResolver and IPreAuthenticatedUriProvider
            var uriResolverMock = Substitute.For<IUriResolver>();
            var critter = new Critter();
            uriResolverMock.GetUriFor(critter).Returns("http://api/entities/0");
            var preAuthenticatedUriProviderMock = Substitute.For<IPreAuthenticatedUriProvider>();
            preAuthenticatedUriProviderMock.CreatePreAuthenticatedUrl("http://api/entities/0", null).Returns(
                "http://api/entities/0?$token=XYZ");
            var resolver = new DefaultPreAuthenticatedUriResolver(uriResolverMock, preAuthenticatedUriProviderMock);
            Assert.That(resolver.GetPreAuthenticatedUriFor(critter, null),
                        Is.EqualTo("http://api/entities/0?$token=XYZ"));
        }
    }
}

