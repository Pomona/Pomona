#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Security.Authentication
{
    public interface IPreAuthenticatedUriProvider
    {
        string CreatePreAuthenticatedUrl(string urlString, DateTime? expiration = null);
        bool VerifyPreAuthenticatedUrl(string urlString, DateTime verificationTime);
    }
}