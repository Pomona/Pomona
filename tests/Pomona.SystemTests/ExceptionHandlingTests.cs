#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Net;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class ExceptionHandlingTests : ClientTestsBase
    {
        [Test]
        public void Get_CritterAtNonExistingUrl_ThrowsWebClientException()
        {
            Assert.Throws<Common.Web.ResourceNotFoundException>(
                () => Client.Get<ICritter>(Client.BaseUri + "critters/38473833"));
        }


        [Test]
        public void Post_FailingThing_ThrowsWebClientException()
        {
            var ex =
                Assert.Throws<WebClientException<IErrorStatus>>(() => Client.FailingThings.Post(new FailingThingForm()));
            Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}