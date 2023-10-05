#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Nancy;

namespace Pomona
{
    [Serializable]
    public class UnknownTypeException : PomonaServerException
    {
        public UnknownTypeException(string message,
                                    Exception innerException = null,
                                    HttpStatusCode statusCode = HttpStatusCode.BadRequest,
                                    IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
            : base(message, innerException, statusCode, responseHeaders)
        {
        }


        protected UnknownTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

