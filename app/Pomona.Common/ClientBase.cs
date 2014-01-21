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
using Pomona.Internals;

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
        public abstract object DownloadFromUri(string uri, Type type);

        public abstract T Get<T>(string uri);


        public abstract T GetLazy<T>(string uri)
            where T : class, IClientResource;


        public abstract string GetUriOfType(Type type);

        public abstract T Patch<T>(T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
            where T : class, IClientResource;


        public abstract object Post<T>(Action<T> postAction)
            where T : class, IClientResource;


        public abstract IQueryable<T> Query<T>();
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
        private static readonly MethodInfo patchServerTypeMethod =
            ReflectionHelper.GetMethodDefinition<ClientBase<TClient>>(x => x.PatchServerType<object>(null, null));

        private static readonly MethodInfo postServerTypeMethod =
            ReflectionHelper.GetMethodDefinition<ClientBase<TClient>>(x => x.PostServerType<object>(null, null, null));

        private readonly string baseUri;
        private readonly ISerializer serializer;
        private readonly ISerializerFactory serializerFactory;
        private static readonly ClientTypeMapper typeMapper;
        private readonly IWebClient webClient;


        static ClientBase()
        {
            // Preload resource info attributes..
            typeMapper = new ClientTypeMapper(typeof(TClient).Assembly);
        }


        protected ClientBase(string baseUri, IWebClient webClient)
        {
            this.webClient = webClient ?? new HttpWebRequestClient();

            this.baseUri = baseUri;
            // BaseUri = "http://localhost:2211/";

            this.serializerFactory = new PomonaJsonSerializerFactory();
            this.serializer = this.serializerFactory.GetSerialier();

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


        public override object DownloadFromUri(string uri, Type type)
        {
            return Deserialize(DownloadFromUri(uri), type);
        }


        public override T Get<T>(string uri)
        {
            return (T)Deserialize(DownloadFromUri(uri), typeof(T));
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
            return
                typeMapper.WrapExtendedQuery<T>(st => new RestQueryProvider(this).CreateQuery(GetUriOfType(st), st));
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


        private void AddIfMatchToPatch<T>(T postForm, RequestOptions requestOptions) where T : class
        {
            string etagValue = null;
            ResourceInfoAttribute resourceInfo;
            if (TryGetResourceInfoForType(typeof(T), out resourceInfo) && resourceInfo.HasEtagProperty)
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

            var deserializer = this.serializerFactory.GetDeserializer();
            var context = new ClientDeserializationContext(typeMapper, this);
            var deserialized = deserializer.Deserialize(
                new StringReader(jsonString),
                expectedType != null
                    ? typeMapper.GetClassMapping(expectedType)
                    : null,
                context);
            return deserialized;
        }


        private string DownloadFromUri(string uri)
        {
            return SendHttpRequest(uri, "GET");
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

            return PatchServerType(form, requestOptions);
        }


        private object PatchExtendedType(ExtendedFormBase patchForm, RequestOptions requestOptions)
        {
            var extendedResourceInfo = patchForm.UserTypeInfo;
            var serverTypeResult = patchServerTypeMethod.MakeGenericMethod(extendedResourceInfo.ServerType)
                .Invoke(this, new[] { patchForm.ProxyTarget, requestOptions });

            return typeMapper.WrapResource(serverTypeResult,
                extendedResourceInfo.ServerType,
                extendedResourceInfo.ExtendedType);
        }


        private T PatchServerType<T>(T postForm, RequestOptions requestOptions)
            where T : class
        {
            var uri = ((IHasResourceUri)((IDelta)postForm).Original).Uri;
            AddIfMatchToPatch(postForm, requestOptions);

            return (T)PostOrPatch(uri, postForm, "PATCH", requestOptions);
        }


        private object PostExtendedType(string uri, ExtendedFormBase postForm, RequestOptions options)
        {
            var extendedResourceInfo = postForm.UserTypeInfo;
            var serverTypeResult = postServerTypeMethod.MakeGenericMethod(extendedResourceInfo.ServerType)
                .Invoke(this, new[] { uri, postForm.ProxyTarget, options });

            return typeMapper.WrapResource(serverTypeResult,
                extendedResourceInfo.ServerType,
                extendedResourceInfo.ExtendedType);
        }


        private object PostOrPatch<T>(string uri, T form, string httpMethod, RequestOptions options)
            where T : class
        {
            if (form == null)
                throw new ArgumentNullException("form");

            var response = SendHttpRequest(uri, httpMethod, form, null, options);

            return Deserialize(response, null);
        }


        private object PostServerType<T>(string uri, T postForm, RequestOptions options)
            where T : class
        {
            return PostOrPatch(uri, postForm, "POST", options);
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
                var requestString = Serialize(requestBodyEntity, requestBodyBaseType);
                requestBytes = Encoding.UTF8.GetBytes(requestString);
            }
            var request = new WebClientRequestMessage(uri, requestBytes, httpMethod);

            this.webClient.Headers.Add("Accept", "application/json");

            string responseString = null;
            Exception thrownException = null;
            try
            {
                if (options != null)
                    options.ApplyRequestModifications(request);

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


        private string Serialize(object obj, TypeSpec expectedBaseType)
        {
            var stringWriter = new StringWriter();
            var writer = this.serializer.CreateWriter(stringWriter);
            var context = new ClientSerializationContext(typeMapper);
            var node = new ItemValueSerializerNode(obj, expectedBaseType, "", context, null);
            this.serializer.SerializeNode(node, writer);
            return stringWriter.ToString();
        }
    }
}