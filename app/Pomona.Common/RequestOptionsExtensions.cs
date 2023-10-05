#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Globalization;

namespace Pomona.Common
{
    public static class RequestOptionsExtensions
    {
        public static TRequestOptions AppendQueryParameter<TRequestOptions>(this TRequestOptions requestOptions,
                                                                            string key,
                                                                            string value)
            where TRequestOptions : IRequestOptions
        {
            if ((object)requestOptions == null)
                throw new ArgumentNullException(nameof(requestOptions));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return requestOptions.RewriteUrl(u => string.Format(CultureInfo.InvariantCulture,
                                                                "{0}{1}{2}={3}",
                                                                u,
                                                                u.Contains("?") ? "&" : "?",
                                                                HttpUtility.UrlEncode(key),
                                                                HttpUtility.UrlEncode(value)));
        }


        public static TRequestOptions RewriteUrl<TRequestOptions>(this TRequestOptions requestOptions,
                                                                  Func<string, string> urlRewriter)
            where TRequestOptions : IRequestOptions
        {
            if ((object)requestOptions == null)
                throw new ArgumentNullException(nameof(requestOptions));
            if (urlRewriter == null)
                throw new ArgumentNullException(nameof(urlRewriter));

            return (TRequestOptions)requestOptions.ModifyRequest(x => x.RequestUri = new Uri(urlRewriter(x.RequestUri.ToString())));
        }
    }
}

