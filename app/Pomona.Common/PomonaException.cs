#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Runtime.Serialization;

namespace Pomona.Common
{
    [Serializable]
    public class PomonaException : ApplicationException
    {
        public PomonaException()
        {
        }


        public PomonaException(string message)
            : base(message)
        {
        }


        public PomonaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected PomonaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
