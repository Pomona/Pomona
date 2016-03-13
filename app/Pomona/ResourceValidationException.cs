#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net;
using System.Runtime.Serialization;

namespace Pomona
{
    [Serializable]
    public class ResourceValidationException : PomonaServerException
    {
        public ResourceValidationException(string message,
                                           string memberName,
                                           string resourceName,
                                           Exception innerException)
            : base(message, innerException, HttpStatusCode.BadRequest)
        {
            MemberName = memberName;
            ResourceName = resourceName;
        }


        protected ResourceValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public string MemberName { get; }

        public string ResourceName { get; }
    }
}