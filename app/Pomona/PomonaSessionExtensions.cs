#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Routing;

namespace Pomona
{
    public static class PomonaSessionExtensions
    {
        public static Task<PomonaResponse> Get(this IPomonaSession session, string url)
        {
            // TODO: Move this to some other class.

            string urlWithoutQueryPart = url;
            IDictionary<string, string> query = null;
            var queryStart = url.IndexOf('?');
            if (queryStart != -1)
            {
                urlWithoutQueryPart = url.Substring(0, queryStart);
                query = url.Substring(queryStart + 1).AsQueryDictionary();
            }

            var relativePath = session.UriResolver.ToRelativePath(urlWithoutQueryPart);
            var req = new PomonaRequest(url, relativePath, query : query);
            return session.Dispatch(req);
        }


        internal static async Task<object> Get(this IPomonaSession session,
                                   UrlSegment urlSegment)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (urlSegment == null)
                throw new ArgumentNullException(nameof(urlSegment));

            var request = new PomonaContext(urlSegment, executeQueryable : true, handleException : false);
            var pomonaResponse = await session.Dispatch(request);
            return pomonaResponse.Entity;
        }


        internal static async Task<IQueryable> Query(this IPomonaSession session,
                                         UrlSegment urlSegment)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (urlSegment == null)
                throw new ArgumentNullException(nameof(urlSegment));

            var request = new PomonaContext(urlSegment, acceptType : typeof(IQueryable), handleException : false);
            var response = await session.Dispatch(request);
            return (IQueryable)response.Entity;
        }


        private static IDictionary<string, string> AsQueryDictionary(this string queryString)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(queryString);
            IDictionary<string, string> queryDictionary = new Dictionary<string, string>();
            foreach (string index in nameValueCollection.AllKeys.Where(key => key != null))
                queryDictionary[index] = (string)nameValueCollection[index];
            return queryDictionary;
        }
    }
}