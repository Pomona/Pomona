#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

using Pomona.Common;

namespace Pomona
{
    [Serializable]
    public class PomonaServerException : PomonaException
    {
        public PomonaServerException()
        {
        }


        public PomonaServerException(string message, object entity = null)
            : base(message)
        {
            Entity = entity;
            StatusCode = HttpStatusCode.InternalServerError;
        }


        public PomonaServerException(string message, Exception innerException, object entity = null)
            : base(message, innerException)
        {
            Entity = entity;
            StatusCode = HttpStatusCode.InternalServerError;
        }


        public PomonaServerException(string message,
                                     Exception innerException,
                                     HttpStatusCode statusCode,
                                     IEnumerable<KeyValuePair<string, string>> responseHeaders = null,
                                     object entity = null)
            : base(message, innerException)
        {
            Entity = entity;
            if (responseHeaders != null)
                ResponseHeaders = responseHeaders.ToList();

            StatusCode = statusCode;
        }


        protected PomonaServerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public object Entity { get; }

        public List<KeyValuePair<string, string>> ResponseHeaders { get; }

        public HttpStatusCode StatusCode { get; }
    }
}