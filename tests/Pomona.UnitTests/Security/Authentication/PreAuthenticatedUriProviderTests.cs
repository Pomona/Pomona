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

using System;
using System.Globalization;

using NUnit.Framework;

using Pomona.Security.Authentication;
using Pomona.Security.Crypto;

namespace Pomona.UnitTests.Security.Authentication
{
    [TestFixture]
    public class PreAuthenticatedUriProviderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.deserializedObject = new UrlToken() { Path = "blah" };
            this.uriProvider = new PreAuthenticatedUriProvider(new MockedCryptoSerializer(this));
        }

        #endregion

        private PreAuthenticatedUriProvider uriProvider;
        private UrlToken deserializedObject;
        private UrlToken serializedObject;

        private class MockedCryptoSerializer : ICryptoSerializer
        {
            private readonly PreAuthenticatedUriProviderTests parent;


            public MockedCryptoSerializer(PreAuthenticatedUriProviderTests parent)
            {
                this.parent = parent;
            }


            public T Deserialize<T>(string hexString)
            {
                if (hexString != "XYZ")
                    throw new NotImplementedException("I'm stupid.");
                return (T)((object)this.parent.deserializedObject);
            }


            public string Serialize(object obj)
            {
                this.parent.serializedObject = (UrlToken)obj;
                return "XYZ";
            }
        }


        [TestCase("http://bahahaha", "http://bahahaha/?$token=XYZ")]
        [TestCase("http://bahahaha?existingParam=something",
            "http://bahahaha/?existingParam=something&$token=XYZ")]
        [TestCase("http://bahahaha?$token=something-else", "http://bahahaha/?$token=XYZ")]
        [TestCase("/relative", "/relative/?$token=XYZ", Ignore = true, Category = "TODO")]
        public void CreatePreAuthenticatedUrl_AppendsSerializedUrlTokenToUrl(string url, string expectedResult)
        {
            var preAuthUrl = this.uriProvider.CreatePreAuthenticatedUrl(url);
            Assert.That(preAuthUrl, Is.EqualTo(expectedResult));
        }


        [TestCase("http://x/filofax?$token=XYZ", "/filofax", null, true)]
        [TestCase("http://y:80/filofax?$token=XYZ", "/filofax", null, /* success?: */ true)]
        [TestCase("http://y:80/filofax?$moo=true&$token=XYZ", "/filofax", null, /* success?: */ false)]
        [TestCase("http://x/filofax?$token=FAILS", "/filofax", null, false)]
        [TestCase("http://x/wrong?$token=XYZ", "/filofax", null, /* success?: */ false)]
        [TestCase("http://x/filofax?$token=XYZ", "/filofax", "2050-02-1 12:18:00Z", /* success?: */
            true)]
        [TestCase("http://x/filofax?$token=XYZ", "/filofax", "2049-02-1 12:18:00Z", /* success?: */
            false)]
        public void VerifyPreAuthenticatedUrl(string url, string urlTokenPath, string expiration, bool expectedResult)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("u"));
            // "http://bahaha/filofax?$token=XYZ"
            this.deserializedObject = new UrlToken()
            {
                Path = urlTokenPath,
                Expiration =
                    expiration != null ? (DateTime?)DateTime.Parse(expiration, CultureInfo.InvariantCulture) : null
            };
            var result = this.uriProvider.VerifyPreAuthenticatedUrl(url,
                new DateTime(2050, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}