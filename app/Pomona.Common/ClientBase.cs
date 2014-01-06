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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
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

        public abstract T Get<T>(string uri);
        public abstract string GetUriOfType(Type type);
        public abstract IQueryable<T> Query<T>();

        public abstract bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo);
        public abstract object DownloadFromUri(string uri, Type type);


        public abstract IList<T> List<T>(string expand = null)
            where T : IClientResource;


        public abstract object Post<T>(Action<T> postAction)
            where T : class, IClientResource;


        public abstract T Patch<T>(T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
            where T : class, IClientResource;

        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;


        protected void RaiseRequestCompleted(WebClientRequestMessage request, WebClientResponseMessage response,
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

        public abstract T GetLazy<T>(string uri)
            where T : class, IClientResource;
    }

    public abstract class ClientBase<TClient> : ClientBase
    {
        private static readonly GenericMethodCaller<ClientBase<TClient>, IEnumerable, object> createListOfTypeMethod;

        private static readonly ReadOnlyDictionary<Type, ResourceInfoAttribute> interfaceToResourceInfoDict;

        private static readonly Type[] knownGenericCollectionTypes =
            {
                typeof (List<>), typeof (IList<>),
                typeof (ICollection<>)
            };

        private static readonly ReadOnlyDictionary<string, ResourceInfoAttribute> typeNameToResourceInfoDict;

        private static readonly MethodInfo postServerTypeMethod =
            ReflectionHelper.GetMethodDefinition<ClientBase<TClient>>(x => x.PostServerType<object>(null, null, null));

        private static readonly MethodInfo patchServerTypeMethod =
            ReflectionHelper.GetMethodDefinition<ClientBase<TClient>>(x => x.PatchServerType<object>(null, null));

        private readonly string baseUri;
        private readonly ISerializer serializer;
        private readonly ISerializerFactory serializerFactory;
        private readonly ClientTypeMapper typeMapper;
        private readonly IWebClient webClient;


        static ClientBase()
        {
            createListOfTypeMethod =
                new GenericMethodCaller<ClientBase<TClient>, IEnumerable, object>(
                    ReflectionHelper.GetMethodDefinition<ClientBase<TClient>>(
                        x => x.CreateListOfTypeGeneric<object>(null)));

            // Preload resource info attributes..
            var resourceTypes =
                typeof (TClient).Assembly.GetTypes().Where(x => typeof (IClientResource).IsAssignableFrom(x));

            var interfaceDict = new Dictionary<Type, ResourceInfoAttribute>();
            var typeNameDict = new Dictionary<string, ResourceInfoAttribute>();
            foreach (
                var resourceInfo in
                    resourceTypes.SelectMany(
                        x =>
                        x.GetCustomAttributes(typeof (ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>())
                )
            {
                interfaceDict[resourceInfo.InterfaceType] = resourceInfo;
                typeNameDict[resourceInfo.JsonTypeName] = resourceInfo;
            }

            interfaceToResourceInfoDict = new ReadOnlyDictionary<Type, ResourceInfoAttribute>(interfaceDict);
            typeNameToResourceInfoDict = new ReadOnlyDictionary<string, ResourceInfoAttribute>(typeNameDict);
        }


        protected ClientBase(string baseUri, IWebClient webClient)
        {
            this.webClient = webClient ?? new HttpWebRequestClient();

            this.baseUri = baseUri;
            // BaseUri = "http://localhost:2211/";

            typeMapper = new ClientTypeMapper(ResourceTypes);
            serializerFactory = new PomonaJsonSerializerFactory();
            serializer = serializerFactory.GetSerialier();
            InstantiateClientRepositories();
        }

        public override IWebClient WebClient
        {
            get { return webClient; }
        }

        public static IEnumerable<Type> ResourceTypes
        {
            get { return interfaceToResourceInfoDict.Keys; }
        }

        public override string BaseUri
        {
            get { return baseUri; }
        }


        public override object DownloadFromUri(string uri, Type type)
        {
            return Deserialize(DownloadFromUri(uri), type);
        }


        public override T Get<T>(string uri)
        {
            return (T) Deserialize(DownloadFromUri(uri), typeof (T));
        }

        public override T GetLazy<T>(string uri)
        {
            var typeInfo = this.GetResourceInfoForType(typeof (T));
            var proxy = (LazyProxyBase) Activator.CreateInstance(typeInfo.LazyProxyType);
            proxy.Uri = uri;
            proxy.Client = this;
            proxy.ProxyTargetType = typeInfo.PocoType;
            return (T) ((object) proxy);
        }


        public override string GetUriOfType(Type type)
        {
            return BaseUri + this.GetResourceInfoForType(type).UrlRelativePath;
        }


        public override IList<T> List<T>(string expand = null)
        {
            // TODO: Implement baseuri property or something.
            var type = typeof (T);

            if (type.IsInterface && type.Name.StartsWith("I"))
            {
                var uri = GetUriOfType(type);

                if (expand != null)
                    uri = uri + "?expand=" + expand;

                return Get<IList<T>>(uri);
            }
            else
                throw new NotImplementedException("We expect an interface as Type parameter!");
        }


        public override object Post<T>(Action<T> postAction)
        {
            CustomUserTypeInfo info;
            string uri;
            if (CustomUserTypeInfo.TryGetCustomUserTypeInfo(typeof (T), this, out info))
            {
                uri = GetUriOfType(info.ServerType);
            }
            else
            {
                uri = GetUriOfType(typeof (T));
            }


            return Post(uri, postAction, null);
        }


        public override T Patch<T>(T target, Action<T> updateAction, Action<IRequestOptions<T>> options = null)
        {
            var patchForm = (T) CreatePatchForm(typeof (T), target);
            updateAction(patchForm);

            var requestOptions = new RequestOptions<T>();
            if (options != null)
                options(requestOptions);

            return Patch(patchForm, requestOptions);
        }


        public override bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return interfaceToResourceInfoDict.TryGetValue(type, out resourceInfo);
        }


        public string GetRelativeUriForType(Type type)
        {
            var resourceInfo = this.GetResourceInfoForType(type);
            return resourceInfo.UrlRelativePath;
        }

        public override IQueryable<T> Query<T>()
        {
            return new RestQuery<T>(new RestQueryProvider(this, typeof (T)));
        }

        private object CreatePostForm(Type resourceType)
        {
            CustomUserTypeInfo userTypeInfo;
            var isClientResource = TryGetUserTypeInfo(resourceType, out userTypeInfo);
            var resourceInfo = this.GetResourceInfoForType(isClientResource ? userTypeInfo.ServerType : resourceType);
            if (resourceInfo.PostFormType == null)
            {
                throw new InvalidOperationException("Method POST is not allowed for uri.");
            }
            var serverPostForm = Activator.CreateInstance(resourceInfo.PostFormType);

            if (isClientResource)
            {
                var userPostForm =
                    (ClientSideFormProxyBase) RuntimeProxyFactory.Create(typeof (ClientSideFormProxyBase), resourceType);
                userPostForm.Initialize(this, userTypeInfo, serverPostForm);
                return userPostForm;
            }
            return serverPostForm;
        }

        private object CreatePatchForm(Type resourceType, object original)
        {
            var originalClientSideProxy = original as ClientSideResourceProxyBase;
            var isClientResource = originalClientSideProxy != null;
            var userTypeInfo = isClientResource ? originalClientSideProxy.UserTypeInfo : null;
            var resourceInfo = this.GetResourceInfoForType(isClientResource ? userTypeInfo.ServerType : resourceType);
            if (resourceInfo.PatchFormType == null)
            {
                throw new InvalidOperationException("Method PATCH is not allowed for uri.");
            }

            if (isClientResource)
            {
                // We want to patch the wrapped proxy!
                original = originalClientSideProxy.ProxyTarget;
            }

            var serverPatchForm = ObjectDeltaProxyBase.CreateDeltaProxy(original,
                                                                        typeMapper.GetClassMapping(
                                                                            resourceInfo.InterfaceType),
                                                                        typeMapper, null);

            if (isClientResource)
            {
                var userPostForm =
                    (ClientSideFormProxyBase) RuntimeProxyFactory.Create(typeof (ClientSideFormProxyBase), resourceType);
                userPostForm.Initialize(this, userTypeInfo, serverPatchForm);
                return userPostForm;
            }
            return serverPatchForm;
        }

        internal override object Post<T>(string uri, Action<T> postAction, RequestOptions options)
        {
            var postForm = (T) CreatePostForm(typeof (T));
            postAction(postForm);
            return Post(uri, postForm, options);
        }


#if false

    // TODO JUST REMOVE THIS METHOD!!! JUST KEEPING IT TEMPORARILY FOR COPYING FROM ;)
        private object PostOrPatchOldRemoveMe<T>(string uri, T form, Action<T> postAction, string httpMethod,
                                      Func<ResourceInfoAttribute, Type> formTypeGetter,
                                      Action<WebClientRequestMessage> modifyRequestHandler)
            where T : class
        {
            var type = typeof (T);
            // TODO: T needs to be an interface, not sure how we fix this, maybe generate one Update method for every entity
            if (!type.IsInterface)
                throw new InvalidOperationException("postAction needs to operate on the interface of the entity");


            Type expectedBaseType;

            CustomUserTypeInfo customUserTypeInfo;

            if (CustomUserTypeInfo.TryGetCustomUserTypeInfo(typeof (T), this, out customUserTypeInfo))
            {
                if (form != null)
                    throw new NotImplementedException("Only supports form set to null (created by action) for now.");

                var resourceInfo = this.GetResourceInfoForType(customUserTypeInfo.ServerType);

                var formType = formTypeGetter(resourceInfo);
                var wrappedForm = Activator.CreateInstance(formType);
                var proxy =
                    (ClientSideFormProxyBase)((object)RuntimeProxyFactory<ClientSideFormProxyBase, T>.Create());
                proxy.Initialize(this, customUserTypeInfo, wrappedForm);

                if (postAction != null)
                {
                    postAction((T)((object)proxy));
                }
                var innerResponse = postOrPatchMethod.MakeGenericMethod(customUserTypeInfo.ServerType)
                                                     .Invoke(this,
                                                             new[]
                                                                 {
                                                                     uri, wrappedForm, null, httpMethod, formTypeGetter,
                                                                     modifyRequestHandler
                                                                 });

                var responseProxy =
                    (ClientSideResourceProxyBase)
                    ((object)RuntimeProxyFactory<ClientSideResourceProxyBase, T>.Create());

                responseProxy.Initialize(this, customUserTypeInfo, innerResponse);
                return responseProxy;
            }
            else
            {
                var resourceInfo = this.GetResourceInfoForType(type);

                var formType = formTypeGetter(resourceInfo);

                // When form type of ResourceInfo is null, it means that method is not allowed.
                if (formType == null)
                    throw new InvalidOperationException("Method " + httpMethod + " is not allowed for uri.");

                if (form == null)
                {
                    form = (T)Activator.CreateInstance(formType);
                }
            }

            if (postAction != null)
            {
                postAction(form);
            }

            // Post the json!
            var response = SendHttpRequest(uri, httpMethod, form, null /*typeMapper.GetClassMapping(expectedBaseType)*/,
                                           modifyRequestHandler);

            return Deserialize(response, null);
        }

#endif

        private object PostOrPatch<T>(string uri, T form, string httpMethod, RequestOptions options)
            where T : class
        {
            if (form == null) throw new ArgumentNullException("form");


            var response = SendHttpRequest(uri, httpMethod, form, null, options);

            return Deserialize(response, null);
        }


        internal override object Post<T>(string uri, T form, RequestOptions options)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (form == null) throw new ArgumentNullException("form");

            var type = typeof (T);
            CustomUserTypeInfo userTypeInfo;
            if (TryGetUserTypeInfo(type, out userTypeInfo))
            {
                return PostUserType(uri, (ClientSideFormProxyBase) ((object) form), options);
            }

            return PostServerType(uri, form, options);
        }

        private T Patch<T>(T form, RequestOptions requestOptions)
            where T : class
        {
            if (form == null) throw new ArgumentNullException("form");
            if (form is ClientSideFormProxyBase)
            {
                return (T) PatchUserType((ClientSideFormProxyBase) ((object) form), requestOptions);
            }

            return PatchServerType(form, requestOptions);
        }

        private object PostUserType(string uri, ClientSideFormProxyBase postForm, RequestOptions options)
        {
            var userTypeInfo = postForm.UserTypeInfo;
            var serverTypeResult = postServerTypeMethod.MakeGenericMethod(userTypeInfo.ServerType)
                                                       .Invoke(this, new[] {uri, postForm.ProxyTarget, options});
            var resultProxy =
                (ClientSideResourceProxyBase)
                RuntimeProxyFactory.Create(typeof (ClientSideResourceProxyBase), userTypeInfo.ClientType);
            resultProxy.Initialize(this, userTypeInfo, serverTypeResult);
            return resultProxy;
        }

        private object PostServerType<T>(string uri, T postForm, RequestOptions options)
            where T : class
        {
            return PostOrPatch(uri, postForm, "POST", options);
        }

        private object PatchUserType(ClientSideFormProxyBase patchForm, RequestOptions requestOptions)
        {
            var userTypeInfo = patchForm.UserTypeInfo;
            var serverTypeResult = patchServerTypeMethod.MakeGenericMethod(userTypeInfo.ServerType)
                                                        .Invoke(this, new[] {patchForm.ProxyTarget, requestOptions});
            var resultProxy =
                (ClientSideResourceProxyBase)
                RuntimeProxyFactory.Create(typeof (ClientSideResourceProxyBase), userTypeInfo.ClientType);
            resultProxy.Initialize(this, userTypeInfo, serverTypeResult);
            return resultProxy;
        }

        private T PatchServerType<T>(T postForm, RequestOptions requestOptions)
            where T : class
        {
            var uri = ((IHasResourceUri) ((IDelta) postForm).Original).Uri;
            AddIfMatchToPatch(postForm, requestOptions);

            return (T) PostOrPatch(uri, postForm, "PATCH", requestOptions);
        }

        private void AddIfMatchToPatch<T>(T postForm, RequestOptions requestOptions) where T : class
        {
            string etagValue = null;
            ResourceInfoAttribute resourceInfo;
            if (TryGetResourceInfoForType(typeof (T), out resourceInfo) && resourceInfo.HasEtagProperty)
            {
                etagValue = (string) resourceInfo.EtagProperty.GetValue(postForm, null);
            }

            if (etagValue != null)
                requestOptions.ModifyRequest(
                    request => request.Headers.Add("If-Match", string.Format("\"{0}\"", etagValue)));
        }

        private bool TryGetUserTypeInfo(Type type, out CustomUserTypeInfo userTypeInfo)
        {
            return CustomUserTypeInfo.TryGetCustomUserTypeInfo(type, this, out userTypeInfo);
        }


        private object CreateListOfTypeGeneric<TElementType>(IEnumerable elements)
        {
            return new List<TElementType>(elements.Cast<TElementType>());
        }


        private object Deserialize(string jsonString, Type expectedType)
        {
            // TODO: Clean up this mess, we need to get a uniform container type for all results! [KNS]
            var jToken = JToken.Parse(jsonString);

            if (expectedType == typeof (JToken))
                return jToken;

            var jObject = jToken as JObject;
            if (jObject != null)
            {
                JToken typeValue;
                if (jObject.TryGetValue("_type", out typeValue))
                {
                    if (typeValue.Type == JTokenType.String && (string) ((JValue) typeValue).Value == "__result__")
                    {
                        JToken itemsToken;
                        if (!jObject.TryGetValue("items", out itemsToken))
                            throw new InvalidOperationException("Got result object, but lacking items");

                        var totalCount = (int) jObject.GetValue("totalCount");

                        var deserializedItems = Deserialize(itemsToken.ToString(), expectedType);
                        return QueryResult.Create((IEnumerable) deserializedItems, /* TODO */ 0, totalCount,
                                                  "http://todo");
                    }
                }
            }

            var deserializer = serializerFactory.GetDeserializer();
            var context = new ClientDeserializationContext(typeMapper, this);
            var deserialized = deserializer.Deserialize(
                new StringReader(jsonString),
                expectedType != null
                    ? typeMapper.GetClassMapping(expectedType)
                    : null,
                context);
            return deserialized;
        }


        private ResourceInfoAttribute GetLeafResourceInfo(Type sourceType)
        {
            var allResourceInfos = sourceType.GetInterfaces().Select(
                x =>
                    {
                        ResourceInfoAttribute resourceInfo;
                        if (!TryGetResourceInfoForType(x, out resourceInfo))
                            resourceInfo = null;
                        return resourceInfo;
                    }).Where(x => x != null).ToList();

            var mostSubtyped = allResourceInfos
                .FirstOrDefault(
                    x =>
                    !allResourceInfos.Any(
                        y => x.InterfaceType != y.InterfaceType && x.InterfaceType.IsAssignableFrom(y.InterfaceType)));

            return mostSubtyped;
        }


        private string DownloadFromUri(string uri)
        {
            return SendHttpRequest(uri, "GET");
        }


        private void InstantiateClientRepositories()
        {
            var generatedAssembly = GetType().Assembly;
            var repoTypes = generatedAssembly.GetTypes()
                .Where(x => typeof (IClientRepository).IsAssignableFrom(x) && !x.IsInterface && !x.IsGenericType).ToList();
            Console.WriteLine(string.Join("\r\n", repoTypes));
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
                    GetType().GetProperties().Where(x => typeof (IClientRepository).IsAssignableFrom(x.PropertyType)))
            {
                var repositoryInterface = prop.PropertyType;
                var repositoryImplementation = repositoryImplementations[repositoryInterface];

                Type[] typeArgs;
                if (!repositoryInterface.TryExtractTypeArguments(typeof (IQueryableRepository<>), out typeArgs))
                    throw new InvalidOperationException("Expected IQueryableRepository to inherit IClientRepository..");

                var tResource = typeArgs[0];
                var uri = GetUriOfType(tResource);
                prop.SetValue(this, Activator.CreateInstance(repositoryImplementation, this, uri, null, null), null);
            }
        }


        private string Serialize(object obj, TypeSpec expectedBaseType)
        {
            var stringWriter = new StringWriter();
            var writer = serializer.CreateWriter(stringWriter);
            var context = new ClientSerializationContext(typeMapper);
            var node = new ItemValueSerializerNode(obj, expectedBaseType, "", context, null);
            serializer.SerializeNode(node, writer);
            return stringWriter.ToString();
        }

        private string SendHttpRequest(string uri, string httpMethod, object requestBodyEntity = null,
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

            webClient.Headers.Add("Accept", "application/json");

            string responseString = null;
            Exception thrownException = null;
            try
            {
                if (options != null)
                    options.ApplyRequestModifications(request);

                response = webClient.Send(request);
                responseString = (response.Data != null && response.Data.Length > 0)
                                     ? Encoding.UTF8.GetString(response.Data)
                                     : null;

                if ((int) response.StatusCode >= 400)
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