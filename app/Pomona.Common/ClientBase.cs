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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
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
        public abstract IWebClient WebClient { get; set; }

        public abstract T Get<T>(string uri);
        public abstract string GetUriOfType(Type type);

        public abstract bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo);
        public abstract object DownloadString(string uri, Type type);


        public abstract IList<T> List<T>(string expand = null)
            where T : IClientResource;


        public abstract object Post<T>(Action<T> postAction)
            where T : class, IClientResource;


        public abstract T Patch<T>(T target, Action<T> updateAction)
            where T : class, IClientResource;

        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;


        protected void RaiseRequestCompleted(string httpMethod, string uri, string requestString, string responseString)
        {
            var eh = RequestCompleted;
            if (eh != null)
                eh(this, new ClientRequestLogEventArgs(httpMethod, uri, requestString, responseString));
        }


        internal abstract object Post<T>(string uri, Action<T> postAction)
            where T : class, IClientResource;

        internal abstract object Post<T>(string uri, T postForm)
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

        private static readonly MethodInfo postOrPatchMethod =
            ReflectionHelper.GetGenericMethodDefinition<ClientBase<TClient>>(
                x => x.PostOrPatch("", "", null, "POST", null));

        private readonly string baseUri;
        private readonly JsonSerializer jsonSerializer;
        private readonly ISerializer serializer;
        private readonly ISerializerFactory serializerFactory;
        private readonly ClientTypeMapper typeMapper;
        private IWebClient webClient;


        static ClientBase()
        {
            createListOfTypeMethod =
                new GenericMethodCaller<ClientBase<TClient>, IEnumerable, object>(
                    ReflectionHelper.GetGenericMethodDefinition<ClientBase<TClient>>(
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


        protected ClientBase(string baseUri)
        {
            jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new StringEnumConverter());

            webClient = new WrappedWebClient();

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
            set { webClient = value; }
        }

        public static IEnumerable<Type> ResourceTypes
        {
            get { return interfaceToResourceInfoDict.Keys; }
        }

        public override string BaseUri
        {
            get { return baseUri; }
        }


        public override object DownloadString(string uri, Type type)
        {
            return Deserialize(DownloadString(uri), type);
        }


        public override T Get<T>(string uri)
        {
            Log("Fetching uri {0}", uri);
            return (T) Deserialize(DownloadString(uri), typeof (T));
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


            return Post(uri, postAction);
        }


        public override T Patch<T>(T target, Action<T> updateAction)
        {
            return (T) PostOrPatch(((IHasResourceUri) target).Uri, null, updateAction, "PATCH", x => x.PutFormType);
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


        internal override object Post<T>(string uri, Action<T> postAction)
        {
            return PostOrPatch(uri, null, postAction, "POST", x => x.PostFormType);
        }

        private object PostOrPatch<T>(string uri, T form, Action<T> postAction, string httpMethod,
                                      Func<ResourceInfoAttribute, Type> formTypeGetter)
            where T : class
        {
            var type = typeof (T);
            // TODO: T needs to be an interface, not sure how we fix this, maybe generate one Update method for every entity
            if (!type.IsInterface)
                throw new InvalidOperationException("postAction needs to operate on the interface of the entity");

            var serverType = type;

            CustomUserTypeInfo customUserTypeInfo;

            if (CustomUserTypeInfo.TryGetCustomUserTypeInfo(typeof (T), this, out customUserTypeInfo))
            {
                if (form != null)
                    throw new NotImplementedException("Only supports form set to null (created by action) for now.");

                var proxy =
                    (ClientSideFormProxyBase) ((object) RuntimeProxyFactory<ClientSideFormProxyBase, T>.Create());
                proxy.AttributesProperty = customUserTypeInfo.DictProperty;

                var resourceInfo = this.GetResourceInfoForType(customUserTypeInfo.ServerType);

                var wrappedForm = Activator.CreateInstance(formTypeGetter(resourceInfo));

                proxy.ProxyTarget = wrappedForm;

                if (postAction != null)
                {
                    postAction((T) ((object) proxy));
                }
                var innerResponse = postOrPatchMethod.MakeGenericMethod(customUserTypeInfo.ServerType)
                                                     .Invoke(this,
                                                             new[] {uri, wrappedForm, null, httpMethod, formTypeGetter});

                var responseProxy =
                    (ClientSideResourceProxyBase)
                    ((object) RuntimeProxyFactory<ClientSideResourceProxyBase, T>.Create());
                responseProxy.AttributesProperty = customUserTypeInfo.DictProperty;
                responseProxy.ProxyTarget = innerResponse;
                return responseProxy;
            }
            else
            {
                var resourceInfo = this.GetResourceInfoForType(type);

                var newType = formTypeGetter(resourceInfo);

                if (form == null)
                    form = (T) Activator.CreateInstance(newType);
            }

            if (postAction != null)
            {
                postAction(form);
            }

            // Post the json!
            var response = UploadToUri(uri, form, type, httpMethod);

            return Deserialize(response, null);
        }

        internal override object Post<T>(string uri, T postForm)
        {
            return PostOrPatch(uri, postForm, null, "POST", x => x.PostFormType);
        }


        private object CreateListOfTypeGeneric<TElementType>(IEnumerable elements)
        {
            return new List<TElementType>(elements.Cast<TElementType>());
        }


        private object Deserialize(string jsonString, Type expectedType)
        {
            // TODO: Clean up this mess, we need to get a uniform container type for all results! [KNS]
            var jToken = JToken.Parse(jsonString);
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


        private string DownloadString(string uri)
        {
            // TODO: Check that response code is correct and content-type matches JSON. [KNS]
            webClient.Headers["Accept"] = "application/json";
            var downloadData = webClient.DownloadData(uri);
            var responseString = Encoding.UTF8.GetString(downloadData);

            RaiseRequestCompleted("GET", uri, null, responseString);

            return responseString;
        }


        private void InstantiateClientRepositories()
        {
            foreach (
                var prop in
                    GetType().GetProperties().Where(
                        x =>
                        x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof (ClientRepository<,>)))
            {
                var repositoryType = prop.PropertyType;
                var tResource = repositoryType.GetGenericArguments()[0];
                var uri = GetUriOfType(tResource);
                prop.SetValue(this, Activator.CreateInstance(repositoryType, this, uri), null);
            }
        }


        private void Log(string format, params object[] args)
        {
            // TODO: Provide optional integration with CommonLogging
            Console.WriteLine(format, args);
        }


        private string Serialize(object obj, Type expectedBaseType)
        {
            var stringWriter = new StringWriter();
            var writer = serializer.CreateWriter(stringWriter);
            var context = new ClientSerializationContext(typeMapper);
            var node = new ItemValueSerializerNode(obj, typeMapper.GetClassMapping(expectedBaseType), "", context);
            serializer.SerializeNode(node, writer);
            return stringWriter.ToString();
        }

        private string UploadToUri(string uri, object obj, Type expectedBaseType, string httpMethod)
        {
            var requestString = Serialize(obj, expectedBaseType);

            var requestBytes = Encoding.UTF8.GetBytes(requestString);
            webClient.Headers["Accept"] = "application/json";
            var responseBytes = webClient.UploadData(uri, httpMethod, requestBytes);
            var responseString = Encoding.UTF8.GetString(responseBytes);

            RaiseRequestCompleted(httpMethod, uri, requestString, responseString);

            return responseString;
        }
    }
}