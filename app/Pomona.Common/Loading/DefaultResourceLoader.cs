#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Common.Loading
{
    public class DefaultResourceLoader : IResourceLoader
    {
        private readonly IResourceLoader loader;


        public DefaultResourceLoader(IResourceLoader loader)
        {
            if (loader == null)
                throw new ArgumentNullException("wrappedLoader");

            this.loader = loader;
        }


        public object Get(string uri, Type type, RequestOptions requestOptions)
        {
            if (requestOptions != null)
                requestOptions.ResourceLoader = this;

            return this.loader.Get(uri, type, requestOptions);
        }


        public Task<object> GetAsync(string uri, Type type, RequestOptions requestOptions)
        {
            if (requestOptions != null)
                requestOptions.ResourceLoader = this;

            return this.loader.GetAsync(uri, type, requestOptions);
        }
    }
}