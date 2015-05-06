#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.Common
{
    public class ChildResourceRepository<TResource, TPostResponseResource, TId>
        : ClientRepository<TResource, TPostResponseResource, TId>
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
            var requestOptions = new RequestOptions();
            AddEtagOptions(requestOptions);
            return (TPostResponseResource)Client.Post(Uri, (TResource)((object)form), requestOptions);
        }


        public override TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction)
        {
            return base.Post(postAction, AddEtagOptions);
        }


        public override TPostResponseResource Post(Action<TResource> postAction)
        {
            return base.Post(postAction, AddEtagOptions);
        }


        public override TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                                      Action<IRequestOptions<TSubResponseResource>> options)
        {
            return base.Post<TSubResource, TSubResponseResource>(postAction,
                                                                 x =>
                                                                 {
                                                                     if (options != null)
                                                                         options(x);
                                                                     AddEtagOptions(x);
                                                                 });
        }


        public override TPostResponseResource Post<TSubResource>(Action<TSubResource> postAction,
                                                                 Action<IRequestOptions<TPostResponseResource>> options)
        {
            return base.Post(postAction,
                             x =>
                             {
                                 if (options != null)
                                     options(x);
                                 AddEtagOptions(x);
                             });
        }


        private void AddEtagOptions(IRequestOptions options)
        {
            var parentResourceInfo = Client.GetMostInheritedResourceInterfaceInfo(this.parent.GetType());
            if (parentResourceInfo.HasEtagProperty)
            {
                var etag = parentResourceInfo.EtagProperty.GetValue(this.parent, null);
                options.ModifyRequest(r => r.Headers.Add("If-Match", string.Format("\"{0}\"", etag)));
            }
        }
    }
}