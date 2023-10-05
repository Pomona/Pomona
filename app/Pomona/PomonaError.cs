#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;

using Nancy;

namespace Pomona
{
    public class PomonaError
    {
        public PomonaError(HttpStatusCode statusCode)
            : this(statusCode, null)
        {
        }


        public PomonaError(HttpStatusCode statusCode,
                           object entity,
                           IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
        {
            StatusCode = statusCode;
            Entity = entity;
            if (responseHeaders != null)
                ResponseHeaders = responseHeaders.ToList();
        }


        public object Entity { get; }

        public List<KeyValuePair<string, string>> ResponseHeaders { get; }

        public HttpStatusCode StatusCode { get; }
    }
}

