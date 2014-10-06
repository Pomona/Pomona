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

using Extra.Client;
using Nancy.Testing;
using NUnit.Framework;
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
            var nancyTestingWebClient = new NancyTestingWebClient(new Browser(critterBootstrapper));
            return new ExtraClient(baseUri + "Extra/", nancyTestingWebClient);
        }

        public override void SetupRequestCompletedHandler()
        {
        }


        public override void TeardownRequestCompletedHandler()
        {
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
            var post = Client.SimpleExtraDatas.Post(new SimpleExtraDataForm() {TheString = "NootNoot"});
            var result = Client.SimpleExtraDatas.Get(post.Id);
            Assert.AreEqual(post.Id, result.Id);
            Assert.AreEqual("NootNoot", result.TheString);
        }
    }
}