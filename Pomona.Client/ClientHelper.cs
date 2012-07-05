using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        WebClient webClient = new WebClient();

        public IList<T> List<T>(string expand = null)
        {
            // TODO: Implement baseuri property or something.
            var uri = "http://localhost:2211/" + typeof(T).Name.ToLower();

            if (expand != null)
                uri = uri + "?expand=" + expand;

            return FetchUri<IList<T>>(uri);
        }

        public T FetchUri<T>(string uri)
        {
            return (T)Deserialize(typeof(T), FetchUri(uri));
        }

        private JToken FetchUri(string uri)
        {
            return JToken.Parse(Encoding.UTF8.GetString(webClient.DownloadData(uri)));
        }

        private static object CreateListOfType(Type elementType, IEnumerable elements)
        {
            var createListOfTypeGeneric =
                typeof(ClientHelper).GetMethod("CreateListOfTypeGeneric", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType);

            return createListOfTypeGeneric.Invoke(null, new object[] { elements });
        }

        private static object CreateListOfTypeGeneric<TElementType>(IEnumerable elements)
        {
            return new List<TElementType>(elements.Cast<TElementType>());
        }


        private static Type[] knownGenericCollectionTypes = { typeof(List<>), typeof(IList<>), typeof(ICollection<>) };

        static bool TryGetCollectionElementType(Type type, out Type elementType, bool searchInterfaces = true)
        {
            elementType = null;

            // First look if we're dealing directly with a known collection type
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (knownGenericCollectionTypes.Contains(genericTypeDefinition))
                {
                    elementType = type.GetGenericArguments()[0];
                }
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

        static object Deserialize(Type expectedType, JToken jToken)
        {
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                return DeserializeObject(expectedType, jObject);
            }

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

        static object DeserializeObject(Type expectedType, JObject jObject)
        {
            // TODO: Support fetching proxy objects.
            if (jObject.Properties().Any(x => x.Name == "_uri"))
                return null;

            var createdType = expectedType;

            var typeProperty = jObject.Properties().FirstOrDefault(x => x.Name == "_type");
            JToken typePropertyToken;
            if (jObject.TryGetValue("_type", out typePropertyToken))
            {
                var typeString = (string)((JValue)typePropertyToken).Value;
                createdType =
                    expectedType.Assembly.GetTypes().Where(x => x.Name == typeString).First(x => expectedType.IsAssignableFrom(x));
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
                {
                    propInfo.SetValue(target, Deserialize(propInfo.PropertyType, jprop.Value), null);
                }
            }

            return target;
        }
    }
}