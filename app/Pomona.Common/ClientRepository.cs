#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;
using Pomona.Common.Linq;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;

namespace Pomona.Common
{
    public class ChildResourceRepository<TResource, TPostResponseResource> : ClientRepository<TResource, TPostResponseResource>
        where TResource : class, IClientResource
        where TPostResponseResource : IClientResource
    {
        private readonly IClientResource parent;

        public ChildResourceRepository(ClientBase client, string uri, IEnumerable results, IClientResource parent)
            : base(client, uri, results, parent)
        {
            this.parent = parent;
        }


        public override TPostResponseResource Post(IPostForm form)
        {
            return (TPostResponseResource)Client.Post(Uri, (TResource)((object)form),GetEtagOptions());
        }


        private RequestOptions GetEtagOptions()
        {
            var parentResourceInfo = Client.GetMostInheritedResourceInterfaceInfo(this.parent.GetType());
            RequestOptions options = null;
            if (parentResourceInfo.HasEtagProperty)
            {
                var etag = parentResourceInfo.EtagProperty.GetValue(this.parent, null);
                options = new RequestOptions();
                options.ModifyRequest(r => r.Headers.Add("If-Match", string.Format("\"{0}\"", etag)));
            }
            return options;
        }


        public override TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
        {
            throw new NotImplementedException();
        }
    }

    public class ClientRepository<TResource, TPostResponseResource> :
        IClientRepository<TResource, TPostResponseResource>, IQueryable<TResource>
        where TResource : class, IClientResource
        where TPostResponseResource : IClientResource
    {
        private readonly ClientBase client;

        private readonly string uri;
        private readonly IEnumerable<TResource> results;


        public ClientRepository(ClientBase client, string uri, IEnumerable results, IClientResource parent)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            this.client = client;
            this.uri = uri;
            this.results = results as IEnumerable<TResource> ?? (results != null ? results.Cast<TResource>() : null);
        }


        internal ClientBase Client
        {
            get { return client; }
        }

        public string Uri
        {
            get { return uri; }
        }


        public TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction, Action<IRequestOptions<TSubResource>> options) where TSubResource : class, TResource
        {
            return client.Patch(resource, patchAction, options);
        }

        public TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction)
            where TSubResource : class, TResource
        {
            return Patch(resource, patchAction, null);
        }

        public virtual TPostResponseResource Post(IPostForm form)
        {
            return (TPostResponseResource)client.Post(Uri, (TResource)((object)form), null);
        }


        public virtual TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource
        {
            return (TPostResponseResource)client.Post(Uri, postAction, null);
        }

        public IQueryable<TSubResource> Query<TSubResource>()
            where TSubResource : TResource
        {
            return client.Query<TSubResource>(uri);
        }

        public TPostResponseResource Post(Action<TResource> postActionBlah)
        {
            return (TPostResponseResource)client.Post(Uri, postActionBlah, null);
        }

        public object Post<TPostForm>(TResource resource, TPostForm form)
            where TPostForm : class, IPostForm, IClientResource
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (form == null) throw new ArgumentNullException("form");

            return client.Post(((IHasResourceUri)resource).Uri, form, null);
        }

        public TResource Get(object id)
        {
            return client.Get<TResource>(GetResourceUri(id));
        }

        public IQueryable<TResource> Query()
        {
            return client.Query<TResource>(uri);
        }

        public IEnumerator<TResource> GetEnumerator()
        {
            return results != null ? results.GetEnumerator() : Query().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return Expression.Constant(Query()); }
        }

        public Type ElementType
        {
            get { return typeof (TResource); }
        }

        public IQueryProvider Provider
        {
            get { return new RestQueryProvider(client); }
        }

        public TResource GetLazy(object id)
        {
            return client.GetLazy<TResource>(GetResourceUri(id));
        }

        private string GetResourceUri(object id)
        {
            return string.Format("{0}/{1}", uri, HttpUtility.UrlPathEncode(Convert.ToString(id, CultureInfo.InvariantCulture)));
        }
    }
}