#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

using Nancy;
using Nancy.Extensions;

using Pomona.Queries;
using Pomona.Routing;

namespace Pomona
{
    /// <summary>
    /// Extension methods for <see cref="IPomonaSession"/>
    /// </summary>
    public static class PomonaSessionExtensions
    {
        /// <summary>
        /// Gets a <see cref="PomonaResponse"/> for the given <paramref name="url"/>.
        /// </summary>
        /// <param name="session">The <see cref="IPomonaSession"/> instance.</param>
        /// <param name="url">The URL to create a <see cref="PomonaResponse"/> for.</param>
        /// <returns>
        /// A <see cref="PomonaResponse"/> for the given <paramref name="url"/>.
        /// </returns>
        public static PomonaResponse Get(this IPomonaSession session, string url)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

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


        /// <summary>
        /// Gets a <see cref="PomonaHttpQueryTransformer"/> from the <paramref name="session"/>.
        /// </summary>
        /// <param name="session">The <see cref="IPomonaSession"/> from which to get a <see cref="PomonaHttpQueryTransformer"/>.</param>
        /// <returns>
        /// A <see cref="PomonaHttpQueryTransformer"/> from the <paramref name="session"/>.
        /// </returns>
        public static PomonaHttpQueryTransformer GetQueryTransformer(this IPomonaSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            var typeMapper = session.Factory.TypeMapper;
            var queryPropertyResolver = new QueryTypeResolver(typeMapper);
            var queryExpressionParser = new QueryExpressionParser(queryPropertyResolver);
            return new PomonaHttpQueryTransformer(typeMapper, queryExpressionParser);
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