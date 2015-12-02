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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Pomona.Common.Loading;
using Pomona.Common.Proxies;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public interface IPomonaClient : IClientTypeResolver, IResourceLoader
    {
        ClientSettings Settings { get; }
        ClientTypeMapper TypeMapper { get; }
        HttpClient WebClient { get; }
        void Delete(object resource, RequestOptions options);
        object Patch(object form, RequestOptions options);
        object Post(string uri, object form, RequestOptions options);
        IQueryable<T> Query<T>(string uri);
        T Reload<T>(T resource);
        event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
        Task<object> PostAsync(string uri, IPostForm form, RequestOptions options);
        Task<object> PatchAsync(object form, RequestOptions options);
        Task DeleteAsync(object resource, RequestOptions options);
    }
}