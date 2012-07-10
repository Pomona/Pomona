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
                                                                         typeof (List<>), typeof (IList<>),
                                                                         typeof (ICollection<>)
                                                                     };

        private static Dictionary<Type, Type> interfaceToProxyDictionary = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> interfaceToPocoDictionary = new Dictionary<Type, Type>();

        private readonly WebClient webClient = new WebClient();

        public object GetUri(string uri, Type type)
        {
            return Deserialize(type, GetUri(uri));
        }

        public T GetUri<T>(string uri)
        {
            return (T) Deserialize(typeof (T), GetUri(uri));
        }


        public IList<T> List<T>(string expand = null)
        {
            // TODO: Implement baseuri property or something.
            var uri = "http://localhost:2211/" + typeof (T).Name.ToLower();

            if (expand != null)
                uri = uri + "?expand=" + expand;

            return GetUri<IList<T>>(uri);
        }


        private static object CreateListOfType(Type elementType, IEnumerable elements)
        {
            var createListOfTypeGeneric =
                typeof (ClientHelper).GetMethod("CreateListOfTypeGeneric", BindingFlags.NonPublic | BindingFlags.Static)
                    .
                    MakeGenericMethod(elementType);

            return createListOfTypeGeneric.Invoke(null, new object[] {elements});
        }


        private static object CreateListOfTypeGeneric<TElementType>(IEnumerable elements)
        {
            return new List<TElementType>(elements.Cast<TElementType>());
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

            return Convert.ChangeType(((JValue) jToken).Value, expectedType);
        }


        private object DeserializeObject(Type expectedType, JObject jObject)
        {
            Type receivedSubclassInterface = expectedType;

            JToken typePropertyToken;
            if (jObject.TryGetValue("_type", out typePropertyToken))
            {
                var typeString = (string)((JValue)typePropertyToken).Value;
                receivedSubclassInterface =
                    expectedType.Assembly.GetTypes().Where(x => x.Name == "I" + typeString).First(
                        x => expectedType.IsAssignableFrom(x));
            }

            JToken uriToken;
            if (jObject.TryGetValue("_uri", out uriToken))
            {
                var uriValue = (JValue) uriToken;
                return CreateProxyFor((string) uriValue.Value, receivedSubclassInterface);
            }

            var createdType = receivedSubclassInterface;

            // Find matching type for interface, we simply do this by removing the "I" from the beginning
            if (receivedSubclassInterface.Name.StartsWith("I") && expectedType.IsInterface)
            {
                // TODO: Cache this mapping in static dictionary
                createdType = GetPocoForInterface(receivedSubclassInterface);
            }


            // TODO: Support subclassing, maybe in special "_type" property in json
            var target = Activator.CreateInstance(createdType);

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

        private object CreateProxyFor(string uri, Type expectedType)
        {
            var proxyType = GetProxyForInterface(expectedType);
            var proxy = (ProxyBase) Activator.CreateInstance(proxyType);
            proxy.ProxyInterceptor = new LazyProxyInterceptor(uri, GetPocoForInterface(expectedType), this);
            return proxy;
        }

        private static Type GetProxyForInterface(Type expectedType)
        {
            lock (interfaceToProxyDictionary)
            {
                Type createdType;
                if (!interfaceToProxyDictionary.TryGetValue(expectedType, out createdType))
                {
                    if (!expectedType.Name.StartsWith("I") || expectedType.Name.Length < 2 || !expectedType.IsInterface)
                    {
                        throw new InvalidOperationException(expectedType.FullName + " not recognized as interface.");
                    }
                    var proxyName = expectedType.Name.Substring(1) + "Proxy";
                    createdType =
                        expectedType.Assembly.GetTypes().First(
                            x => x.FullName == expectedType.Namespace + "." + proxyName);
                    interfaceToProxyDictionary[expectedType] = createdType;
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
                    {
                        throw new InvalidOperationException(expectedType.FullName + " not recognized as interface.");
                    }
                    var pocoName = expectedType.Name.Substring(1);
                    createdType =
                        expectedType.Assembly.GetTypes().First(
                            x => x.FullName == expectedType.Namespace + "." + pocoName);
                    interfaceToPocoDictionary[expectedType] = createdType;
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

        private JToken PutUri(string uri, JToken jsonData)
        {
            var requestBytes = Encoding.UTF8.GetBytes(jsonData.ToString());
            var responseBytes = webClient.UploadData(uri, "PUT", requestBytes);
            return JToken.Parse(Encoding.UTF8.GetString(responseBytes));
        }

        private JToken GetUri(string uri)
        {
            var jsonString = Encoding.UTF8.GetString(webClient.DownloadData(uri));
            Console.WriteLine("Incoming data from " + uri + ":\r\n" + jsonString);
            return JToken.Parse(jsonString);
        }
    }
}