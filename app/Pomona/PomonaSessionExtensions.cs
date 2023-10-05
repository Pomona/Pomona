#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

