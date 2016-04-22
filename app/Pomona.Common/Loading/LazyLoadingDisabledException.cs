#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Runtime.Serialization;

namespace Pomona.Common.Loading
{
    [Serializable]
    public class LazyLoadingDisabledException : LoadException
    {
        public LazyLoadingDisabledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        public LazyLoadingDisabledException(string uri, Type type)
            : base(uri, type)
        {
        }


        protected LazyLoadingDisabledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        internal LazyLoadingDisabledException(string resourcePath, LoadException innerException)
            : base($"Unable to fetch {resourcePath}. Lazy loading is disabled.", innerException)
        {
        }
    }
}