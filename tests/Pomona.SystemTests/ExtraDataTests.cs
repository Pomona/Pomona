using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extra.Client;
using Nancy.Testing;
using NUnit.Framework;
using Pomona.Common;
using Pomona.Common.Web;
using Pomona.TestHelpers;
using Pomona.UnitTests.Client;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class ExtraDataTests : CritterServiceTestsBase<Extra.Client.Client>
    {
        [Test]
        public void GetExtraData()
        {
            var result = Client.SimpleExtraDatas.Get(0);
            Assert.AreEqual(0,result.Id);
            Assert.AreEqual("What",result.TheString);
        }
        [Test]
        public void PostExtraData()
        {
            var post = Client.SimpleExtraDatas.Post(new SimpleExtraDataForm() {TheString = "NootNoot"});
            var result = Client.SimpleExtraDatas.Get(post.Id);
            Assert.AreEqual(post.Id, result.Id);
            Assert.AreEqual("NootNoot", result.TheString);
        }
        [Test]
        public void PatchExtraData()
        {
            var resource = Client.SimpleExtraDatas.GetLazy(0);
            var patch = Client.SimpleExtraDatas.Patch(resource,(x)=>x.TheString="NootNoot");
            var result = Client.SimpleExtraDatas.Get(0);
            Assert.AreEqual(0,patch.Id);
            Assert.AreEqual("NootNoot", result.TheString);
        }

        public override bool UseSelfHostedHttpServer
        {
            get { return true; }
        }

        public override Extra.Client.Client CreateHttpTestingClient(string baseUri)
        {
            return new Client(baseUri+"/Extra/");
        }

        public override Extra.Client.Client CreateInMemoryTestingClient(string baseUri, Example.CritterBootstrapper critterBootstrapper)
        {
            var nancyTestingWebClient = new NancyTestingWebClient(new Browser(critterBootstrapper));
            return new Client(baseUri + "/Extra/", nancyTestingWebClient);
        }

        public override void SetupRequestCompletedHandler()
        {
        }
    }
}
