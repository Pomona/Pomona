#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaResponse
    {
        static PomonaResponse()
        {
            NoBodyEntity = new object();
        }


        public PomonaResponse(PomonaContext context,
                              object entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders = null)
            : this(entity, statusCode, GetExpandedPaths(context, expandedPaths), resultType, responseHeaders)
        {
        }


        public PomonaResponse(PomonaQuery query, object entity)
            : this(query, entity, HttpStatusCode.OK)
        {
        }


        public PomonaResponse(object entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders = null)
        {
            Entity = entity;
            StatusCode = statusCode;
            ExpandedPaths = expandedPaths;
            ResultType = resultType;
            Headers = responseHeaders.EmptyIfNull().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }


        public PomonaResponse(PomonaQuery query, object entity, HttpStatusCode statusCode)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            Entity = entity;
            StatusCode = statusCode;
            ExpandedPaths = query.ExpandedPaths;
            ResultType = query.ResultType;
        }


        public object Entity { get; }

        public string ExpandedPaths { get; }

        public IDictionary<string, IEnumerable<string>> Headers { get; }
        public static object NoBodyEntity { get; }

        public TypeSpec ResultType { get; }

        public HttpStatusCode StatusCode { get; }


        private static string GetExpandedPaths(PomonaContext context, string expandedPaths)
        {
            return String.IsNullOrEmpty(expandedPaths) && context != null
                ? context.ExpandedPaths
                : expandedPaths;
        }
    }
}