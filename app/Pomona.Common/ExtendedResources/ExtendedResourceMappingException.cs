#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Runtime.Serialization;

namespace Pomona.Common.ExtendedResources
{
    [Serializable]
    public class ExtendedResourceMappingException : PomonaException
    {
        public ExtendedResourceMappingException()
        {
        }


        public ExtendedResourceMappingException(string message)
            : base(message)
        {
        }


        public ExtendedResourceMappingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected ExtendedResourceMappingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}