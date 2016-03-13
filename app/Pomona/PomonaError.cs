#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Net;


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
                           IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders = null)
        {
            StatusCode = statusCode;
            Entity = entity;
            if (responseHeaders != null)
                ResponseHeaders = responseHeaders.ToList();
        }


        public object Entity { get; }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders { get; }

        public HttpStatusCode StatusCode { get; }
    }
}