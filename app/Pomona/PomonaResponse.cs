#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Nancy;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaResponse<T> : PomonaResponse
    {
        public PomonaResponse(T entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
            : base(entity, statusCode, expandedPaths, resultType, responseHeaders)
        {
        }


        public PomonaResponse(PomonaQuery query, T entity)
            : base(query, entity)
        {
        }


        public PomonaResponse(PomonaQuery query, T entity, HttpStatusCode statusCode)
            : base(query, entity, statusCode)
        {
        }


        public new T Entity => (T)base.Entity;
    }

    public class PomonaResponse
    {
        internal static readonly object NoBodyEntity;


        static PomonaResponse()
        {
            NoBodyEntity = new object();
        }


        public PomonaResponse(PomonaContext context,
                              object entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
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
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
        {
            Entity = entity;
            StatusCode = statusCode;
            ExpandedPaths = expandedPaths;
            ResultType = resultType;

            if (responseHeaders != null)
                ResponseHeaders = responseHeaders.ToList();
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

        public List<KeyValuePair<string, string>> ResponseHeaders { get; }

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

