#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Runtime.Serialization;

namespace Pomona.Common.Loading
{
    [Serializable]
    public class LoadException : PomonaException
    {
        public LoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        public LoadException(string uri, Type type)
            : base("Could not load " + type)
        {
            Uri = uri;
        }


        protected LoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public string Uri { get; }
    }
}