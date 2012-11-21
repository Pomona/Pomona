#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona.Client
{
    public class ClientRepository<TResource, TPostResponseResource>
        where TResource : IClientResource
        where TPostResponseResource : IClientResource
    {
        private readonly ClientBase client;
        private readonly string uri;


        public ClientRepository(ClientBase client, string uri)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            this.client = client;
            this.uri = uri;
        }


        public string Uri
        {
            get { return this.uri; }
        }


        public TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction)
            where TSubResource : TResource
        {
            return this.client.Put(resource, patchAction);
        }


        public TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
            where TSubResource : TResource
        {
            return (TPostResponseResource)this.client.Post(Uri, postAction);
        }


        public TPostResponseResource Post(Action<TResource> postAction)
        {
            return (TPostResponseResource)this.client.Post(Uri, postAction);
        }


        public IList<TResource> Query(
            Expression<Func<TResource, bool>> predicate,
            Expression<Func<TResource, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null)
        {
            return this.client.Query(Uri, predicate, orderBy, sortOrder, top, skip, expand);
        }
    }
}