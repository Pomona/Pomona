#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.Linq
{
    public class RestQueryRoot<T> : RestQuery<T>, IRestQueryRoot
    {
        public RestQueryRoot(RestQueryProvider provider, string uri)
            : base(provider)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            Uri = uri;
        }


        public string Uri { get; }
    }
}