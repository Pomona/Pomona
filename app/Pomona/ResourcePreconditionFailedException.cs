#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Runtime.Serialization;

using Nancy;

namespace Pomona
{
    [Serializable]
    public class ResourcePreconditionFailedException : PomonaServerException
    {
        public ResourcePreconditionFailedException()
            : base("Precondition failed", null, HttpStatusCode.PreconditionFailed)
        {
        }


        public ResourcePreconditionFailedException(string message)
            : base(message, null, HttpStatusCode.PreconditionFailed)
        {
        }


        public ResourcePreconditionFailedException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.PreconditionFailed)
        {
        }


        protected ResourcePreconditionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}