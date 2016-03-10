#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
                throw new ArgumentNullException("requestOptions");
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");

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
                throw new ArgumentNullException("requestOptions");
            if (urlRewriter == null)
                throw new ArgumentNullException("urlRewriter");

            return (TRequestOptions)requestOptions.ModifyRequest(x => x.RequestUri = new Uri(urlRewriter(x.RequestUri.ToString())));
        }
    }
}