#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Pomona.Common.Internals;
using Pomona.Common.Linq;
using Pomona.Common.Loading;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization.Json;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public class PomonaClient : IPomonaClient
    {
        private readonly IRequestDispatcher dispatcher;
        private readonly ClientTypeMapper typeMapper;


        public PomonaClient(ClientTypeMapper typeMapper)
            : this(typeMapper, (IRequestDispatcher)null)
        {
        }


        public PomonaClient(ClientTypeMapper typeMapper, HttpClient httpClient)
            : this(typeMapper, CreateDefaultRequestDispatcher(typeMapper, new HttpWebClient(httpClient)))
        {
        }


        public PomonaClient(ClientTypeMapper typeMapper, IWebClient webClient)
            : this(typeMapper, CreateDefaultRequestDispatcher(typeMapper, webClient))
        {
        }


        public PomonaClient(ClientTypeMapper typeMapper, IRequestDispatcher dispatcher)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            dispatcher = dispatcher ?? CreateDefaultRequestDispatcher(typeMapper);

            this.typeMapper = typeMapper;
            this.dispatcher = dispatcher;

            Settings = new ClientSettings();

            dispatcher.RequestCompleted += (s, e) => RaiseRequestCompleted(e.Request, e.Response, e.ThrownException);
        }


        public IEnumerable<Type> ResourceTypes => this.typeMapper.ResourceTypes;


        public string GetRelativeUriForType(Type type)
        {
            var resourceInfo = this.GetResourceInfoForType(type);
            return resourceInfo.UrlRelativePath;
        }


        protected void RaiseRequestCompleted(HttpRequestMessage request,
                                             HttpResponseMessage response,
                                             Exception thrownException = null)
        {
            RequestCompleted?.Invoke(this, new ClientRequestLogEventArgs(request, response, thrownException));
        }


        private void AddIfMatchToPatch(object postForm, RequestOptions options)
        {
            string etagValue = null;
            var resourceInfo = TypeMapper.GetMostInheritedResourceInterfaceInfo(postForm.GetType());
            if (resourceInfo.HasEtagProperty)
                etagValue = (string)resourceInfo.EtagProperty.GetValue(postForm, null);

            if (etagValue != null)
            {
                options.ModifyRequest(
                    request => request.Headers.Add("If-Match", $"\"{etagValue}\""));
            }
        }


        private static IRequestDispatcher CreateDefaultRequestDispatcher(ClientTypeMapper typeMapper, IWebClient webClient = null)
        {
            var client = webClient ?? new HttpWebClient();
            var serializerFactory = new PomonaJsonSerializerFactory();
            return new RequestDispatcher(typeMapper, client, serializerFactory);
        }


        private IResourceLoader GetResourceLoader(RequestOptions requestOptions)
        {
            if (requestOptions?.ResourceLoader != null)
                return requestOptions.ResourceLoader;

            if (Settings.LazyMode == LazyMode.Disabled)
                return new DisabledResourceLoader();

            return new DefaultResourceLoader(this);
        }


        private ClientSerializationContextProvider GetSerializationContextProvider(RequestOptions requestOptions)
        {
            var resourceLoader = GetResourceLoader(requestOptions);
            return new ClientSerializationContextProvider(this.typeMapper, this, resourceLoader);
        }


        private static string GetUriOfForm(object form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));
            var delta = form as IDelta;
            if (delta != null)
                return ((IHasResourceUri)delta.Original).Uri;
            var extendedProxy = form as IExtendedResourceProxy;
            if (extendedProxy != null)
                return GetUriOfForm(extendedProxy.WrappedResource);
            throw new InvalidOperationException("Unable to retrieve uri from resource of type "
                                                + form.GetType().FullName);
        }


        public virtual void Delete(object resource, RequestOptions options)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            var uri = ((IHasResourceUri)resource).Uri;
            this.dispatcher.SendRequest(uri, "DELETE", null, GetSerializationContextProvider(options), options);
        }


        public async Task DeleteAsync(object resource, RequestOptions options)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            var uri = ((IHasResourceUri)resource).Uri;
            await this.dispatcher.SendRequestAsync(uri, "DELETE", null, GetSerializationContextProvider(options), options);
        }


        public virtual object Get(string uri, Type type, RequestOptions requestOptions)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (requestOptions == null)
                requestOptions = new RequestOptions(type);
            else if (type != null && requestOptions.ExpectedResponseType == null)
                requestOptions.ExpectedResponseType = type;

            return this.dispatcher.SendRequest(uri, "GET", null, GetSerializationContextProvider(requestOptions), requestOptions);
        }


        public virtual Task<object> GetAsync(string uri, Type type, RequestOptions requestOptions)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (requestOptions == null)
                requestOptions = new RequestOptions(type);
            else if (type != null && requestOptions.ExpectedResponseType == null)
                requestOptions.ExpectedResponseType = type;

            return this.dispatcher.SendRequestAsync(uri, "GET", null, GetSerializationContextProvider(requestOptions), requestOptions);
        }


        public virtual object Patch(object form, RequestOptions options)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var uri = GetUriOfForm(form);

            AddIfMatchToPatch(form, options);
            return this.dispatcher.SendRequest(uri, "PATCH", form, GetSerializationContextProvider(options), options);
        }


        public virtual Task<object> PatchAsync(object form, RequestOptions options)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var uri = GetUriOfForm(form);

            AddIfMatchToPatch(form, options);
            return this.dispatcher.SendRequestAsync(uri, "PATCH", form, GetSerializationContextProvider(options), options);
        }


        public virtual object Post(string uri, object form, RequestOptions options)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return this.dispatcher.SendRequest(uri, "POST", form, GetSerializationContextProvider(options), options);
        }


        public virtual Task<object> PostAsync(string uri, IPostForm form, RequestOptions options)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return this.dispatcher.SendRequestAsync(uri, "POST", form, GetSerializationContextProvider(options), options);
        }


        public virtual IQueryable<T> Query<T>(string uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return this.typeMapper.WrapExtendedQuery<T>(st => new RestQueryProvider(this).CreateQuery(uri, st));
        }


        public virtual T Reload<T>(T resource)
        {
            var resourceWithUri = resource as IHasResourceUri;
            if (resourceWithUri == null)
            {
                throw new ArgumentException("Could not find resource URI, resouce not of type IHasResourceUri.",
                                            nameof(resource));
            }

            if (resourceWithUri.Uri == null)
                throw new ArgumentException("Uri on resource was null.", nameof(resource));

            if (!typeof(T).IsInterface)
                throw new ArgumentException("Type should be an interface inherited from a known resource type.");

            var resourceType = this.GetMostInheritedResourceInterface(typeof(T));
            return (T)Get(resourceWithUri.Uri, resourceType, new RequestOptions(typeof(T)));
        }


        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
        public ClientSettings Settings { get; private set; }


        public virtual bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return this.typeMapper.TryGetResourceInfoForType(type, out resourceInfo);
        }


        public virtual ClientTypeMapper TypeMapper => this.typeMapper;

        public virtual IWebClient WebClient => this.dispatcher.WebClient;
    }
}