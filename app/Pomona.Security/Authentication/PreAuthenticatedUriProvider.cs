#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Common;
using Pomona.Security.Crypto;

namespace Pomona.Security.Authentication
{
    public class PreAuthenticatedUriProvider : IPreAuthenticatedUriProvider
    {
        private readonly ICryptoSerializer cryptoSerializer;


        public PreAuthenticatedUriProvider(ICryptoSerializer cryptoSerializer)
        {
            if (cryptoSerializer == null)
                throw new ArgumentNullException(nameof(cryptoSerializer));
            this.cryptoSerializer = cryptoSerializer;
        }


        private static string AddQueryParameterString(string url, string key, string value)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // ..and adds the new one
            newQueryString.Add(key, value);

            // this gets the page path from root without QueryString
            var pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }


        private static string RemoveQueryStringByKey(string url, string key)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // this gets the page path from root without QueryString
            var pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }


        public string CreatePreAuthenticatedUrl(string urlString, DateTime? expiration = null)
        {
            // Path and query is part of verified url
            var url = new Uri(urlString);
            var verifiedUrlPart = url.PathAndQuery;
            var urlToken = new UrlToken() { Path = verifiedUrlPart, Expiration = expiration };
            var tokenParameter = this.cryptoSerializer.Serialize(urlToken);
            return AddQueryParameterString(urlString, "$token", tokenParameter);
        }


        public bool VerifyPreAuthenticatedUrl(string urlString, DateTime verificationTime)
        {
            var query = HttpUtility.ParseQueryString(new Uri(urlString).Query);
            var tokenParameter = query.Get("$token");
            UrlToken urlToken;
            try
            {
                urlToken = this.cryptoSerializer.Deserialize<UrlToken>(tokenParameter);
            }
            catch
            {
                // TODO: Find out what exceptions to see as yeah you know..
                return false;
            }
            var urlWithoutToken = new Uri(RemoveQueryStringByKey(urlString, "$token"));
            if (urlToken.Expiration.HasValue && urlToken.Expiration < verificationTime)
                return false;
            return urlToken.Path == urlWithoutToken.PathAndQuery;
        }
    }
}