#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Extra.Client;

using NUnit.Framework;

using Pomona.Common.Web;
using Pomona.Example;
using Pomona.TestHelpers;
using Pomona.UnitTests.Client;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class ExtraDataTests : CritterServiceTestsBase<ExtraClient>
    {
        public override ExtraClient CreateHttpTestingClient(string baseUri)
        {
            return new ExtraClient(baseUri + "/Extra/");
        }


        public override ExtraClient CreateInMemoryTestingClient(string baseUri, CritterBootstrapper critterBootstrapper)
        {
            return new ExtraClient(baseUri + "Extra/", new HttpWebClient(new NancyTestingWebClient(critterBootstrapper)));
        }


        [Test]
        public void GetExtraData()
        {
            var result = Client.SimpleExtraDatas.Get(0);
            Assert.AreEqual(0, result.Id);
            Assert.AreEqual("What", result.TheString);
        }


        [Test]
        public void PatchExtraData()
        {
            var resource = Client.SimpleExtraDatas.GetLazy(0);
            var patch = Client.SimpleExtraDatas.Patch(resource, (x) => x.TheString = "NootNoot");
            var result = Client.SimpleExtraDatas.Get(0);
            Assert.AreEqual(0, patch.Id);
            Assert.AreEqual("NootNoot", result.TheString);
        }


        [Test]
        public void PostExtraData()
        {
            var post = Client.SimpleExtraDatas.Post(new SimpleExtraDataForm() { TheString = "NootNoot" });
            var result = Client.SimpleExtraDatas.Get(post.Id);
            Assert.AreEqual(post.Id, result.Id);
            Assert.AreEqual("NootNoot", result.TheString);
        }


        public override void SetupRequestCompletedHandler()
        {
        }


        public override void TeardownRequestCompletedHandler()
        {
        }
    }
}
