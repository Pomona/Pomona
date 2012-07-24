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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Pomona.Client
{
    public class ClientHelper
    {
        private static readonly Type[] knownGenericCollectionTypes = {
            typeof(List<>), typeof(IList<>),
            typeof(ICollection<>)
        };

        private static Dictionary<Type, Type> interfaceToNewProxyDictionary = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> interfaceToPocoDictionary = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> interfaceToProxyDictionary = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> interfaceToUpdateProxyDictionary = new Dictionary<Type, Type>();

        private readonly WebClient webClient = new WebClient();


        public ClientHelper()
        {
            BaseUri = "http://localhost:2211/";
        }


        public string BaseUri { get; set; }


        public object GetUri(string uri, Type type)
        {
            return Deserialize(type, GetUri(uri));
        }


        public T GetUri<T>(string uri)
        {
            return (T)Deserialize(typeof(T), GetUri(uri));
        }


        public IList<T> List<T>(string expand = null)
        {
            // TODO: Implement baseuri property or something.
            var type = typeof(T);

            if (type.IsInterface && type.Name.StartsWith("I"))
            {
                var uri = BaseUri + type.Name.Substring(1).ToLower();

                if (expand != null)
                    uri = uri + "?expand=" + expand;

                return GetUri<IList<T>>(uri);
            }
            else
                throw new NotImplementedException("We expect an interface as Type parameter!");
        }


        public T Post<T>(Action<T> postAction)
        {
            var type = typeof(T);
            // TODO: T needs to be an interface, not sure how we fix this, maybe generate one Update method for every entity
            if (!type.IsInterface)
                throw new InvalidOperationException("postAction needs to operate on the interface of the entity");

            var pocoType = GetPocoForInterface(type);
            var newType = GetNewProxyForInterface(type);
            var newProxy = Activator.CreateInstance(newType);

            postAction((T)newProxy);

            // TODO: Implement baseuri property or something.
            var uri = BaseUri + pocoType.Name.ToLower();

            // Post the json!
            return
                (T)Deserialize(type, UploadToUri(uri, ((PutResourceBase)newProxy).ToJson(), "POST"));
        }


        public T Put<T>(T target, Action<T> updateAction)
        {
            var type = typeof(T);
            // TODO: T needs to be an interface, not sure how we fix this, maybe generate one Update method for every entity
            if (!type.IsInterface)
                throw new InvalidOperationException("updateAction needs to operate on the interface of the entity");

            var updateType = GetUpdateProxyForInterface(type);

            var updateProxy = Activator.CreateInstance(updateType);

            // Run user supplied actions on updateProxy
            updateAction((T)updateProxy);

            // Put the json!
            return
                (T)
                Deserialize(
                    type,
                    UploadToUri(((IHasResourceUri)target).Uri, ((PutResourceBase)updateProxy).ToJson(), "PUT"));
        }


        private static object CreateListOfType(Type elementType, IEnumerable elements)
        {
            var createListOfTypeGeneric =
                typeof(ClientHelper).GetMethod("CreateListOfTypeGeneric", BindingFlags.NonPublic | BindingFlags.Static)
                    .
                    MakeGenericMethod(elementType);

            return createListOfTypeGeneric.Invoke(null, new object[] { elements });
        }


        private static object CreateListOfTypeGeneric<TElementType>(IEnumerable elements)
        {
            return new List<TElementType>(elements.Cast<TElementType>());
        }


        private static Type GetNewProxyForInterface(Type expectedType)
        {
            lock (interfaceToNewProxyDictionary)
            {
                Type createdType;
                if (!interfaceToNewProxyDictionary.TryGetValue(expectedType, out createdType))
                {
                    if (!expectedType.Name.StartsWith("I") || expectedType.Name.Length < 2 || !expectedType.IsInterface)
                        throw new InvalidOperationException(expectedType.FullName + " not recognized as interface.");
                    var pocoName = expectedType.Name.Substring(1);
                    createdType =
                        expectedType.Assembly.GetTypes().First(
                            x => x.FullName == expectedType.Namespace + ".New" + pocoName);
                    interfaceToNewProxyDictionary[expectedType] = createdType;
                }
                return createdType;
            }
        }


        private static Type GetPocoForInterface(Type expectedType)
        {
            lock (interfaceToPocoDictionary)
            {
                Type createdType;
                if (!interfaceToPocoDictionary.TryGetValue(expectedType, out createdType))
                {
                    if (!expectedType.Name.StartsWith("I") || expectedType.Name.Length < 2 || !expectedType.IsInterface)
                        throw new InvalidOperationException(expectedType.FullName + " not recognized as interface.");
                    var pocoName = expectedType.Name.Substring(1);
                    createdType =
                        expectedType.Assembly.GetTypes().First(
                            x => x.FullName == expectedType.Namespace + "." + pocoName);
                    interfaceToPocoDictionary[expectedType] = createdType;
                }
                return createdType;
            }
        }


        private static Type GetProxyForInterface(Type expectedType)
        {
            lock (interfaceToProxyDictionary)
            {
                Type createdType;
                if (!interfaceToProxyDictionary.TryGetValue(expectedType, out createdType))
                {
                    if (!expectedType.Name.StartsWith("I") || expectedType.Name.Length < 2 || !expectedType.IsInterface)
                        throw new InvalidOperationException(expectedType.FullName + " not recognized as interface.");
                    var proxyName = expectedType.Name.Substring(1) + "Proxy";
                    createdType =
                        expectedType.Assembly.GetTypes().First(
                            x => x.FullName == expectedType.Namespace + "." + proxyName);
                    interfaceToProxyDictionary[expectedType] = createdType;
                }
                return createdType;
            }
        }


        private static Type GetUpdateProxyForInterface(Type expectedType)
        {
            lock (interfaceToUpdateProxyDictionary)
            {
                Type createdType;
                if (!interfaceToUpdateProxyDictionary.TryGetValue(expectedType, out createdType))
                {
                    if (!expectedType.Name.StartsWith("I") || expectedType.Name.Length < 2 || !expectedType.IsInterface)
                        throw new InvalidOperationException(expectedType.FullName + " not recognized as interface.");
                    var pocoName = expectedType.Name.Substring(1);
                    createdType =
                        expectedType.Assembly.GetTypes().First(
                            x => x.FullName == expectedType.Namespace + "." + pocoName + "Update");
                    interfaceToUpdateProxyDictionary[expectedType] = createdType;
                }
                return createdType;
            }
        }


        private static bool TryGetCollectionElementType(Type type, out Type elementType, bool searchInterfaces = true)
        {
            elementType = null;

            // First look if we're dealing directly with a known collection type
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (knownGenericCollectionTypes.Contains(genericTypeDefinition))
                    elementType = type.GetGenericArguments()[0];
            }

            if (elementType == null && searchInterfaces)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (TryGetCollectionElementType(interfaceType, out elementType, false))
                        break;
                }
            }

            return elementType != null;
        }


        private object CreateProxyFor(string uri, Type expectedType)
        {
            // Check if this is a proxy for a collection or not
            Type elementType;
            if (TryGetCollectionElementType(expectedType, out elementType))
            {
                var proxy = LazyListProxy.CreateForType(elementType, uri, this);
                return proxy;
            }
            else
            {
                var proxyType = GetProxyForInterface(expectedType);
                var proxy = (LazyProxyBase)Activator.CreateInstance(proxyType);
                proxy.Uri = uri;
                proxy.TargetType = GetPocoForInterface(expectedType);
                proxy.Client = this;
                return proxy;
            }
        }


        private object Deserialize(Type expectedType, JToken jToken)
        {
            var jObject = jToken as JObject;
            if (jObject != null)
                return DeserializeObject(expectedType, jObject);

            var jArray = jToken as JArray;
            if (jArray != null)
            {
                Type listElementType;
                if (!TryGetCollectionElementType(expectedType, out listElementType))
                    throw new SerializationException("Don't know how to serialize JArray to " + expectedType.FullName);

                return CreateListOfType(listElementType, jArray.Children().Select(x => Deserialize(listElementType, x)));
            }

            return Convert.ChangeType(((JValue)jToken).Value, expectedType);
        }


        private object DeserializeObject(Type expectedType, JObject jObject)
        {
            var receivedSubclassInterface = expectedType;

            JToken typePropertyToken;
            if (jObject.TryGetValue("_type", out typePropertyToken))
            {
                var typeString = (string)((JValue)typePropertyToken).Value;
                receivedSubclassInterface =
                    expectedType.Assembly.GetTypes().Where(x => x.Name == "I" + typeString).First(
                        x => expectedType.IsAssignableFrom(x));
            }

            JToken refUriToken;
            if (jObject.TryGetValue("_ref", out refUriToken))
            {
                var uriValue = (JValue)refUriToken;
                return CreateProxyFor((string)uriValue.Value, receivedSubclassInterface);
            }

            var createdType = receivedSubclassInterface;

            // Find matching type for interface, we simply do this by removing the "I" from the beginning
            if (receivedSubclassInterface.Name.StartsWith("I") && expectedType.IsInterface)
            {
                // TODO: Cache this mapping in static dictionary
                createdType = GetPocoForInterface(receivedSubclassInterface);
            }

            var target = (ResourceBase)Activator.CreateInstance(createdType);

            // Set uri, if available in json (for updates etc)
            JToken uriToken;
            if (jObject.TryGetValue("_uri", out uriToken))
                target.Uri = (string)(((JValue)uriToken).Value);

            // TODO: Cache this dictionary
            var propertiesForType = createdType.GetProperties().ToDictionary(x => x.Name.ToLower(), x => x);

            foreach (var jprop in jObject.Properties())
            {
                var name = jprop.Name;
                var nameLowerCase = name.ToLower();

                PropertyInfo propInfo;
                if (propertiesForType.TryGetValue(nameLowerCase, out propInfo))
                    propInfo.SetValue(target, Deserialize(propInfo.PropertyType, jprop.Value), null);
            }

            return target;
        }


        private JToken GetUri(string uri)
        {
            var jsonString = Encoding.UTF8.GetString(this.webClient.DownloadData(uri));
            Console.WriteLine("Incoming data from " + uri + ":\r\n" + jsonString);
            return JToken.Parse(jsonString);
        }


        private JToken PutUri(string uri, JToken jsonData)
        {
            return UploadToUri(uri, jsonData, "PUT");
        }


        private JToken UploadToUri(string uri, JToken jsonData, string httpMethod)
        {
            var requestString = jsonData.ToString();

            Console.WriteLine("PUTting data to " + uri + ":\r\n" + requestString);

            var requestBytes = Encoding.UTF8.GetBytes(requestString);
            var responseBytes = this.webClient.UploadData(uri, httpMethod, requestBytes);
            var responseString = Encoding.UTF8.GetString(responseBytes);

            Console.WriteLine("Received response from PUT:\t\n" + responseString);

            return JToken.Parse(responseString);
        }
    }
}