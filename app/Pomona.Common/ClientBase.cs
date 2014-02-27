#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Newtonsoft.Json.Linq;

using Pomona.Common.ExtendedResources;
using Pomona.Common.Internals;
using Pomona.Common.Linq;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public abstract class ClientBase : IPomonaClient
    {
        internal ClientBase()
        {
        }


        public abstract string BaseUri { get; }
        public abstract IWebClient WebClient { get; }
        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
        public abstract object Get(string uri, Type type);

        public abstract T Get<T>(string uri);


        public abstract T GetLazy<T>(string uri)
            where T : class, IClientResource;


        public abstract string GetUriOfType(Type type);
        public abstract T Reload<T>(T resource);

        public abstract void Delete<T>(T resource)
            where T : class, IClientResource;

        public abstract T Patch<T>(T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
            where T : class, IClientResource;


        public abstract object Post<T>(Action<T> postAction)
            where T : class, IClientResource;


        public abstract IQueryable<T> Query<T>();
        public abstract IQueryable<T> Query<T>(string uri);
        public abstract bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo);


        protected void RaiseRequestCompleted(WebClientRequestMessage request,
            WebClientResponseMessage response,
            Exception thrownException = null)
        {
            var eh = RequestCompleted;
            if (eh != null)
                eh(this, new ClientRequestLogEventArgs(request, response, thrownException));
        }


        internal abstract object Post<T>(string uri, Action<T> postAction, RequestOptions options)
            where T : class, IClientResource;


        internal abstract object Post<T>(string uri, T form, RequestOptions options)
            where T : class, IClientResource;
    }

    public abstract class ClientBase<TClient> : ClientBase
    {
        private readonly string baseUri;
        private readonly ITextSerializer serializer;
        private readonly ITextSerializerFactory serializerFactory;
        private static readonly ClientTypeMapper typeMapper;

        internal static ClientTypeMapper ClientTypeMapper
        {
            get { return typeMapper; }
        }

        private readonly IWebClient webClient;


        static ClientBase()
        {
            // Preload resource info attributes..
            typeMapper = new ClientTypeMapper(typeof(TClient).Assembly);
        }


        public override T Reload<T>(T resource)
        {
            var resourceWithUri = resource as IHasResourceUri;
            if (resourceWithUri == null)
                throw new ArgumentException("Could not find resource URI, resouce not of type IHasResourceUri.", "resource");

            if (resourceWithUri.Uri == null)
                throw new ArgumentException("Uri on resource was null.", "resource");

            if (!typeof(T).IsInterface)
                throw new ArgumentException("Type should be an interface inherited from a known resource type.");

            var resourceType = this.GetMostInheritedResourceInterface(typeof(T));
            return (T)Get(resourceWithUri.Uri, resourceType);
        }


        protected ClientBase(string baseUri, IWebClient webClient)
        {
            this.webClient = webClient ?? new HttpWebRequestClient();

            this.baseUri = baseUri;
            // BaseUri = "http://localhost:2211/";

            this.serializerFactory = new PomonaJsonSerializerFactory(new ClientSerializationContextProvider(typeMapper, this));
            this.serializer = this.serializerFactory.GetSerializer();

            InstantiateClientRepositories();
        }


        public IEnumerable<Type> ResourceTypes
        {
            get { return typeMapper.ResourceTypes; }
        }

        public override string BaseUri
        {
            get { return this.baseUri; }
        }

        public override IWebClient WebClient
        {
            get { return this.webClient; }
        }

        public override void Delete<T>(T resource)
        {
            var uri = ((IHasResourceUri)resource).Uri;
            SendHttpRequest(uri, "DELETE");
        }


        public override object Get(string uri, Type type)
        {
            return SendRequestAndDeserialize(uri, null, "GET", null, type != null ? typeMapper.GetClassMapping(type) : null);
        }


        public override T Get<T>(string uri)
        {
            return (T)Get(uri, typeof(T));
        }


        public override T GetLazy<T>(string uri)
        {
            var typeInfo = this.GetResourceInfoForType(typeof(T));
            var proxy = (LazyProxyBase)Activator.CreateInstance(typeInfo.LazyProxyType);
            proxy.Uri = uri;
            proxy.Client = this;
            proxy.ProxyTargetType = typeInfo.PocoType;
            return (T)((object)proxy);
        }


        public override string GetUriOfType(Type type)
        {
            return BaseUri + this.GetResourceInfoForType(type).UrlRelativePath;
        }


        public override T Patch<T>(T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
        {
            var patchForm = (T)typeMapper.CreatePatchForm(typeof(T), target);
            updateAction(patchForm);

            var requestOptions = new RequestOptions<T>();
            if (options != null)
                options(requestOptions);

            return Patch(patchForm, requestOptions);
        }


        public override object Post<T>(Action<T> postAction)
        {
            ExtendedResourceInfo info;
            string uri;
            if (ExtendedResourceInfo.TryGetExtendedResourceInfo(typeof(T), this, out info))
                uri = GetUriOfType(info.ServerType);
            else
                uri = GetUriOfType(typeof(T));

            return Post(uri, postAction, null);
        }


        public override IQueryable<T> Query<T>()
        {
            return Query<T>(null);
        }

        public override IQueryable<T> Query<T>(string uri)
        {
            return
                typeMapper.WrapExtendedQuery<T>(st => new RestQueryProvider(this).CreateQuery(uri ?? GetUriOfType(st), st));
        }


        public override bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return typeMapper.TryGetResourceInfoForType(type, out resourceInfo);
        }


        public string GetRelativeUriForType(Type type)
        {
            var resourceInfo = this.GetResourceInfoForType(type);
            return resourceInfo.UrlRelativePath;
        }


        internal override object Post<T>(string uri, Action<T> postAction, RequestOptions options)
        {
            var postForm = (T)typeMapper.CreatePostForm(typeof(T));
            postAction(postForm);
            return Post(uri, postForm, options);
        }


        internal override object Post<T>(string uri, T form, RequestOptions options)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (form == null)
                throw new ArgumentNullException("form");

            var type = typeof(T);
            ExtendedResourceInfo userTypeInfo;
            if (typeMapper.TryGetExtendedTypeInfo(type, out userTypeInfo))
                return PostExtendedType(uri, (ExtendedFormBase)((object)form), options);

            return PostServerType(uri, form, options);
        }


        private void AddIfMatchToPatch(object postForm, RequestOptions requestOptions) 
        {
            string etagValue = null;
            ResourceInfoAttribute resourceInfo = typeMapper.GetMostInheritedResourceInterfaceInfo(postForm.GetType());
            if (resourceInfo.HasEtagProperty)
                etagValue = (string)resourceInfo.EtagProperty.GetValue(postForm, null);

            if (etagValue != null)
            {
                requestOptions.ModifyRequest(
                    request => request.Headers.Add("If-Match", string.Format("\"{0}\"", etagValue)));
            }
        }


        private object Deserialize(string jsonString, Type expectedType)
        {
            // TODO: Clean up this mess, we need to get a uniform container type for all results! [KNS]
            var jToken = JToken.Parse(jsonString);

            if (expectedType == typeof(JToken))
                return jToken;

            var jObject = jToken as JObject;
            if (jObject != null)
            {
                JToken typeValue;
                if (jObject.TryGetValue("_type", out typeValue))
                {
                    if (typeValue.Type == JTokenType.String && (string)((JValue)typeValue).Value == "__result__")
                    {
                        JToken itemsToken;
                        if (!jObject.TryGetValue("items", out itemsToken))
                            throw new InvalidOperationException("Got result object, but lacking items");

                        var totalCount = (int)jObject.GetValue("totalCount");

                        var deserializedItems = Deserialize(itemsToken.ToString(), expectedType);
                        return QueryResult.Create((IEnumerable)deserializedItems,
                            /* TODO */ 0,
                            totalCount,
                            "http://todo");
                    }
                }
            }

            return serializerFactory.GetDeserializer().DeserializeString(jsonString,
                new DeserializeOptions() { ExpectedBaseType = expectedType });
        }

        private void InstantiateClientRepositories()
        {
            var generatedAssembly = GetType().Assembly;
            var repoTypes = generatedAssembly.GetTypes()
                .Where(x => typeof(IClientRepository).IsAssignableFrom(x) && !x.IsInterface && !x.IsGenericType).ToList();
            var repositoryImplementations =
                repoTypes
                    .Select(
                        x =>
                            new
                            {
                                Interface =
                                    x.GetInterfaces()
                                        .First(y => y.Assembly == generatedAssembly && y.Name == "I" + x.Name),
                                Implementation = x
                            })
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


        private T Patch<T>(T form, RequestOptions requestOptions)
            where T : class
        {
            if (form == null)
                throw new ArgumentNullException("form");
            if (form is ExtendedFormBase)
                return (T)PatchExtendedType((ExtendedFormBase)((object)form), requestOptions);

            return (T)PatchServerType(form, requestOptions);
        }


        private object PatchExtendedType(ExtendedFormBase patchForm, RequestOptions requestOptions)
        {
            var extendedResourceInfo = patchForm.UserTypeInfo;
            var serverTypeResult = PatchServerType(patchForm.WrappedResource, requestOptions);

            return typeMapper.WrapResource(serverTypeResult,
                extendedResourceInfo.ServerType,
                extendedResourceInfo.ExtendedType);
        }


        private object PatchServerType(object postForm, RequestOptions requestOptions)
        {
            var uri = ((IHasResourceUri)((IDelta)postForm).Original).Uri;
            AddIfMatchToPatch(postForm, requestOptions);

            return SendRequestAndDeserialize(uri, postForm, "PATCH", requestOptions);
        }


        private object PostExtendedType(string uri, ExtendedFormBase postForm, RequestOptions options)
        {
            var extendedResourceInfo = postForm.UserTypeInfo;
            
            var serverTypeResult = PostServerType(uri, postForm.WrappedResource, options);

            return typeMapper.WrapResource(serverTypeResult,
                extendedResourceInfo.ServerType,
                extendedResourceInfo.ExtendedType);
        }


        private object SendRequestAndDeserialize(string uri, object form, string httpMethod, RequestOptions options, TypeSpec responseBaseType = null)
        {
            var response = SendHttpRequest(uri, httpMethod, form, null, options);
            return Deserialize(response, responseBaseType);
        }


        private object PostServerType(string uri, object postForm, RequestOptions options)
        {
            return SendRequestAndDeserialize(uri, postForm, "POST", options);
        }


        private string SendHttpRequest(string uri,
            string httpMethod,
            object requestBodyEntity = null,
            TypeSpec requestBodyBaseType = null,
            RequestOptions options = null)
        {
            byte[] requestBytes = null;
            WebClientResponseMessage response = null;
            if (requestBodyEntity != null)
            {
                requestBytes = serializer.SerializeToBytes(requestBodyEntity,
                    new SerializeOptions() { ExpectedBaseType = requestBodyBaseType });
            }
            var request = new WebClientRequestMessage(uri, requestBytes, httpMethod);

            string responseString = null;
            Exception thrownException = null;
            try
            {
                if (options != null)
                    options.ApplyRequestModifications(request);

                request.Headers.Add("Accept", "application/json");
                response = this.webClient.Send(request);
                responseString = (response.Data != null && response.Data.Length > 0)
                    ? Encoding.UTF8.GetString(response.Data)
                    : null;

                if ((int)response.StatusCode >= 400)
                {
                    var gotJsonResponseBody = responseString != null &&
                                              response.Headers.GetValues("Content-Type")
                                                  .Any(x => x.StartsWith("application/json"));

                    var responseObject = gotJsonResponseBody
                        ? Deserialize(responseString, null)
                        : null;

                    throw WebClientException.Create(this, request, response, responseObject, null);
                }
            }
            catch (Exception ex)
            {
                thrownException = ex;
                throw;
            }
            finally
            {
                RaiseRequestCompleted(request, response, thrownException);
            }

            return responseString;
        }
    }
}