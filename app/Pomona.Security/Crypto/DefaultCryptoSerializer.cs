#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Security.Cryptography;

using Pomona.Security.Authentication;

namespace Pomona.Security.Crypto
{
    public class DefaultCryptoSerializer : CryptoSerializerBase
    {
        public DefaultCryptoSerializer(ISiteKeyProvider siteKeyProvider)
            : base(siteKeyProvider, new RNGCryptoServiceProvider())
        {
        }
    }
}