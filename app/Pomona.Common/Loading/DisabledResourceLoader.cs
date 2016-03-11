#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Common.Loading
{
    public class DisabledResourceLoader : IResourceLoader
    {
        public object Get(string uri, Type type, RequestOptions requestOptions)
        {
            throw new LoadException(uri, type);
        }


        public Task<object> GetAsync(string uri, Type type, RequestOptions requestOptions)
        {
            throw new LoadException(uri, type);
        }
    }
}