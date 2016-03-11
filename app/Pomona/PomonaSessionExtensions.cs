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
using System.Linq;

using Nancy;
using Nancy.Extensions;

using Pomona.Routing;

namespace Pomona
{
    public static class PomonaSessionExtensions
    {
        public static PomonaResponse Get(this IPomonaSession session, string url)
        {
            // TODO: Move this to some other class.

            string urlWithoutQueryPart = url;
            DynamicDictionary query = null;
            var queryStart = url.IndexOf('?');
            if (queryStart != -1)
            {
                urlWithoutQueryPart = url.Substring(0, queryStart);
                query = url.Substring(queryStart + 1).AsQueryDictionary();
            }

            var relativePath = session.GetInstance<IUriResolver>().ToRelativePath(urlWithoutQueryPart);
            var req = new PomonaRequest(url, relativePath, query : query);
            return session.Dispatch(req);
        }


        internal static object Get(this IPomonaSession session,
                                   UrlSegment urlSegment)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (urlSegment == null)
                throw new ArgumentNullException(nameof(urlSegment));

            var request = new PomonaContext(urlSegment, executeQueryable : true, handleException : false);
            return session.Dispatch(request).Entity;
        }


        internal static IQueryable Query(this IPomonaSession session,
                                         UrlSegment urlSegment)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (urlSegment == null)
                throw new ArgumentNullException(nameof(urlSegment));

            var request = new PomonaContext(urlSegment, acceptType : typeof(IQueryable), handleException : false);
            return (IQueryable)session.Dispatch(request).Entity;
        }
    }
}