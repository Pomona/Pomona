#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
        private UrlToken deserializedObject;
        private UrlToken serializedObject;
        private PreAuthenticatedUriProvider uriProvider;


        [TestCase("http://bahahaha", "http://bahahaha/?$token=XYZ")]
        [TestCase("http://bahahaha?existingParam=something",
            "http://bahahaha/?existingParam=something&$token=XYZ")]
        [TestCase("http://bahahaha?$token=something-else", "http://bahahaha/?$token=XYZ")]
        [TestCase("/relative", "/relative/?$token=XYZ", Ignore = "TODO", Category = "TODO")]
        public void CreatePreAuthenticatedUrl_AppendsSerializedUrlTokenToUrl(string url, string expectedResult)
        {
            var preAuthUrl = this.uriProvider.CreatePreAuthenticatedUrl(url);
            Assert.That(preAuthUrl, Is.EqualTo(expectedResult));
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.deserializedObject = new UrlToken() { Path = "blah" };
            this.uriProvider = new PreAuthenticatedUriProvider(new MockedCryptoSerializer(this));
        }

        #endregion

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
    }
}

