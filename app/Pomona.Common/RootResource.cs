#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;

using Pomona.Common.ExtendedResources;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public abstract class RootResource<TClient> : IPomonaRootResource
    {
        private readonly IPomonaClient client;


        protected RootResource(string baseUri)
            : this(baseUri, new HttpWebClient())
        {
        }


        protected RootResource(string baseUri, IWebClient webClient)
        {
            BaseUri = baseUri;
            this.client = new PomonaClient(ClientTypeMapper, webClient);
            InstantiateClientRepositories();
        }


        protected RootResource(string baseUri, IRequestDispatcher dispatcher)
        {
            BaseUri = baseUri;
            this.client = new PomonaClient(ClientTypeMapper, dispatcher);
            InstantiateClientRepositories();
        }


        public string BaseUri { get; private set; }

        private ClientTypeMapper ClientTypeMapper { get; } = ClientTypeMapper.GetTypeMapper(typeof(TClient));


        public virtual object Post<T>(Action<T> postAction)
        {
            ExtendedResourceInfo info;
            string uri;
            if (ClientTypeMapper.TryGetExtendedTypeInfo(typeof(T), out info))
                uri = GetUriOfType(info.ServerType);
            else
                uri = GetUriOfType(typeof(T));

            return this.client.Post<T>(uri, postAction, null);
        }


        private string GetUriOfType(Type type)
        {
            ExtendedResourceInfo userTypeInfo;
            if (ClientTypeMapper.TryGetExtendedTypeInfo(type, out userTypeInfo))
                type = userTypeInfo.ServerType;
            return BaseUri + this.client.GetResourceInfoForType(type).UrlRelativePath;
        }


        private void InstantiateClientRepositories()
        {
            var generatedAssembly = GetType().Assembly;
            var repoProperties = GetType().GetProperties().Where(x => typeof(IClientRepository).IsAssignableFrom(x.PropertyType)).ToList();

            var repositoryImplementations =
                repoProperties
                    .Select(x => x.PropertyType)
                    .Distinct()
                    .GroupBy(x => x.Assembly)
                    .Select(y =>
                    {
                        var asmTypes = y.Key.GetTypes();
                        return
                            y.Select(
                                x => new { Interface = x, Implementation = asmTypes.First(t => x.IsAssignableFrom(t) && !t.IsInterface) })
                             .ToList();
                    })
                    .SelectMany(x => x)
                    .ToDictionary(x => x.Interface, x => x.Implementation);

            foreach (
                var prop in
                    GetType().GetProperties().Where(x => typeof(IClientRepository).IsAssignableFrom(x.PropertyType)))
            {
                var repositoryInterface = prop.PropertyType;
                var repositoryImplementation = repositoryImplementations[repositoryInterface];

                Type[] typeArgs;
                if (!repositoryInterface.TryExtractTypeArguments(typeof(IQueryableRepository<>), out typeArgs))
                    throw new InvalidOperationException("Expected IQueryableRepository to inherit IClientRepository..");

                var tResource = typeArgs[0];
                var uri = GetUriOfType(tResource);
                prop.SetValue(this, Activator.CreateInstance(repositoryImplementation, this, uri, null, null), null);
            }
        }


        public Task DeleteAsync(object resource, RequestOptions options)
        {
            return this.client.DeleteAsync(resource, options);
        }


        public object Get(string uri, Type type, RequestOptions requestOptions)
        {
            return this.client.Get(uri, type, requestOptions);
        }


        public Task<object> GetAsync(string uri, Type type, RequestOptions requestOptions)
        {
            return this.client.GetAsync(uri, type, requestOptions);
        }


        public Task<object> PatchAsync(object form, RequestOptions options)
        {
            return this.client.PatchAsync(form, options);
        }


        public Task<object> PostAsync(string uri, IPostForm form, RequestOptions options)
        {
            return this.client.PostAsync(uri, form, options);
        }


        public virtual IQueryable<T> Query<T>()
        {
            return this.client.Query<T>(GetUriOfType(typeof(T)));
        }


        public IQueryable<T> Query<T>(string uri)
        {
            return this.client.Query<T>(uri);
        }


        public T Reload<T>(T resource)
        {
            return this.client.Reload(resource);
        }


        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted
        {
            add { this.client.RequestCompleted += value; }
            remove { this.client.RequestCompleted -= value; }
        }

        public ClientSettings Settings => this.client.Settings;


        public bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return this.client.TryGetResourceInfoForType(type, out resourceInfo);
        }


        public ClientTypeMapper TypeMapper => this.client.TypeMapper;

        public IWebClient WebClient => this.client.WebClient;


        void IPomonaClient.Delete(object resource, RequestOptions requestOptions)
        {
            this.client.Delete(resource, requestOptions);
        }


        object IPomonaClient.Patch(object form, RequestOptions requestOptions)
        {
            return this.client.Patch(form, requestOptions);
        }


        object IPomonaClient.Post(string uri, object form, RequestOptions options)
        {
            return this.client.Post(uri, form, options);
        }
    }
}