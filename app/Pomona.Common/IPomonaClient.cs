#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
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
        IWebClient WebClient { get; }
        void Delete(object resource, RequestOptions options);
        Task DeleteAsync(object resource, RequestOptions options);
        object Patch(object form, RequestOptions options);
        Task<object> PatchAsync(object form, RequestOptions options);
        object Post(string uri, object form, RequestOptions options);
        Task<object> PostAsync(string uri, IPostForm form, RequestOptions options);
        IQueryable<T> Query<T>(string uri);
        T Reload<T>(T resource);
        event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
    }
}

