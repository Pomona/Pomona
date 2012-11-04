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
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json.Linq;

using Pomona.Internals;

namespace Pomona.Client
{
    public abstract class ClientBase
    {
        internal ClientBase()
        {
        }


        public abstract T GetUri<T>(string uri);
        public abstract object GetUri(string uri, Type type);


        public abstract IList<T> List<T>(string expand = null)
            where T : IClientResource;


        public abstract object Post<T>(Action<T> postAction)
            where T : IClientResource;


        public abstract T Put<T>(T target, Action<T> updateAction)
            where T : IClientResource;


        public abstract IList<T> Query<T>(
            Expression<Func<T, bool>> predicate, string expand = null, int? top = null, int? skip = null);
    }

    public abstract class ClientBase<TClient> : ClientBase
    {
        private static readonly GenericMethodCaller<ClientBase<TClient>, IEnumerable, object> createListOfTypeMethod;

        private static readonly ReadOnlyDictionary<Type, ResourceInfoAttribute> interfaceToResourceInfoDict;

        private static readonly Type[] knownGenericCollectionTypes =
            {
                typeof(List<>), typeof(IList<>),
                typeof(ICollection<>)
            };

        private static readonly ReadOnlyDictionary<string, ResourceInfoAttribute> typeNameToResourceInfoDict;
        private readonly WebClient webClient = new WebClient();


        static ClientBase()
        {
            createListOfTypeMethod =
                new GenericMethodCaller<ClientBase<TClient>, IEnumerable, object>(
                    ReflectionHelper.GetGenericMethodDefinition<ClientBase<TClient>>(
                        x => x.CreateListOfTypeGeneric<object>(null)));

            // Preload resource info attributes..
            var resourceTypes =
                typeof(TClient).Assembly.GetTypes().Where(x => typeof(IClientResource).IsAssignableFrom(x));

            var interfaceDict = new Dictionary<Type, ResourceInfoAttribute>();
            var typeNameDict = new Dictionary<string, ResourceInfoAttribute>();
            foreach (
                var resourceInfo in
                    resourceTypes.SelectMany(
                        x => x.GetCustomAttributes(typeof(ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>())
                )
            {
                interfaceDict[resourceInfo.InterfaceType] = resourceInfo;
                typeNameDict[resourceInfo.JsonTypeName] = resourceInfo;
            }

            interfaceToResourceInfoDict = new ReadOnlyDictionary<Type, ResourceInfoAttribute>(interfaceDict);
            typeNameToResourceInfoDict = new ReadOnlyDictionary<string, ResourceInfoAttribute>(typeNameDict);
        }


        protected ClientBase()
        {
            BaseUri = "http://localhost:2211/";

            InstantiateClientRepositories();
        }


        public string BaseUri { get; set; }


        public static string GetRelativeUriForType(Type type)
        {
            var resourceInfo = GetResourceInfoForType(type);
            return resourceInfo.UrlRelativePath;
        }


        public static ResourceInfoAttribute GetResourceInfoForType(Type type)
        {
            ResourceInfoAttribute resourceInfoAttribute;
            if (!interfaceToResourceInfoDict.TryGetValue(type, out resourceInfoAttribute))
            {
                throw new InvalidOperationException(
                    "Unable to find ResourceInfoAttribute for type " + type.FullName);
            }
            return resourceInfoAttribute;
        }


        public override object GetUri(string uri, Type type)
        {
            return Deserialize(type, GetUri(uri));
        }


        public override T GetUri<T>(string uri)
        {
            return (T)Deserialize(typeof(T), GetUri(uri));
        }


        public override IList<T> List<T>(string expand = null)
        {
            // TODO: Implement baseuri property or something.
            var type = typeof(T);

            if (type.IsInterface && type.Name.StartsWith("I"))
            {
                var uri = GetUriOfType(type);

                if (expand != null)
                    uri = uri + "?expand=" + expand;

                return GetUri<IList<T>>(uri);
            }
            else
                throw new NotImplementedException("We expect an interface as Type parameter!");
        }


        public override object Post<T>(Action<T> postAction)
        {
            var type = typeof(T);
            // TODO: T needs to be an interface, not sure how we fix this, maybe generate one Update method for every entity
            if (!type.IsInterface)
                throw new InvalidOperationException("postAction needs to operate on the interface of the entity");

            var resourceInfo = GetResourceInfoForType(type);

            var newType = resourceInfo.PostFormType;
            var newProxy = Activator.CreateInstance(newType);

            postAction((T)newProxy);

            // TODO: Implement baseuri property or something.
            var uri = GetUri(type);

            // Post the json!
            var requestJson = ((PutResourceBase)newProxy).ToJson();
            requestJson["_type"] = resourceInfo.JsonTypeName;
            var response = UploadToUri(uri, requestJson, "POST");

            return
                (T)Deserialize(type, response);
        }


        public override T Put<T>(T target, Action<T> updateAction)
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


        public override IList<T> Query<T>(
            Expression<Func<T, bool>> predicate, string expand = null, int? top = null, int? skip = null)
        {
            var resourceInfo = GetResourceInfoForType(typeof(T));
            if (!resourceInfo.IsUriBaseType)
            {
                // If we get an expression operating on a subclass of the URI base type, we need to modify it (casting)
                // TODO: Optimize this by caching MethodInfo or make this method non-generic.
                var transformedExpression = ChangeExpressionArgumentToType(predicate, resourceInfo.UriBaseType);
                var genericMethod = typeof(ClientBase<TClient>).GetMethod("Query");
                var results = (IEnumerable)genericMethod.MakeGenericMethod(resourceInfo.UriBaseType).Invoke(
                    this, new object[] { transformedExpression, expand, top, skip });
                return new List<T>(results.OfType<T>());
            }

            var filterString = new QueryPredicateBuilder<T>(predicate).ToString();
            var uri = BaseUri + GetRelativeUriForType(typeof(T)) + "?filter=" + filterString;

            if (expand != null)
                uri = uri + "&expand=" + expand;

            if (top.HasValue)
                uri = uri + "&top=" + top.Value;

            if (skip.HasValue)
                uri = uri + "&skip=" + skip.Value;

            return GetUri<IList<T>>(uri);
        }


        public string GetUri(Type type)
        {
            return BaseUri + GetRelativeUriForType(type);
        }


        private static Expression ChangeExpressionArgumentToType(LambdaExpression lambdaExpr, Type newArgType)
        {
            if (lambdaExpr == null)
                throw new ArgumentNullException("lambdaExpr");
            if (newArgType == null)
                throw new ArgumentNullException("newArgType");
            if (lambdaExpr.Parameters.Count != 1)
                throw new InvalidOperationException("Only works using expressions with property count equal to one.");

            var origParam = lambdaExpr.Parameters[0];
            var body = lambdaExpr.Body;
            var newParam = Expression.Parameter(newArgType, origParam.Name);
            var visitor = new ChangeExpressionArgumentVisitor(origParam, newParam);
            var newBody = visitor.Visit(body);
            return
                Expression.Lambda(Expression.AndAlso(Expression.TypeIs(newParam, origParam.Type), newBody), newParam);
        }


        private static Type GetNewProxyForInterface(Type expectedType)
        {
            return GetResourceInfoForType(expectedType).PostFormType;
        }


        private static Type GetPocoForInterface(Type expectedType)
        {
            return GetResourceInfoForType(expectedType).PocoType;
        }


        private static Type GetUpdateProxyForInterface(Type expectedType)
        {
            return GetResourceInfoForType(expectedType).PutFormType;
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


        private object CreateListOfType(Type elementType, IEnumerable elements)
        {
            return createListOfTypeMethod.Call(elementType, this, elements);
        }


        private object CreateListOfTypeGeneric<TElementType>(IEnumerable elements)
        {
            return new List<TElementType>(elements.Cast<TElementType>());
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
                var resourceInfo = GetResourceInfoForType(expectedType);
                var proxy = (LazyProxyBase)Activator.CreateInstance(resourceInfo.LazyProxyType);
                proxy.Uri = uri;
                proxy.TargetType = resourceInfo.PocoType;
                proxy.Client = this;
                return proxy;
            }
        }


        private object Deserialize(Type expectedType, JToken jToken)
        {
            // TODO: Clean up this mess, we need to get a uniform container type for all results! [KNS]
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
                        return Deserialize(expectedType, itemsToken);
                    }
                }
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

            return Convert.ChangeType(((JValue)jToken).Value, expectedType);
        }


        private object DeserializeObject(Type expectedType, JObject jObject)
        {
            var receivedSubclassInterface = expectedType;

            JToken typePropertyToken;
            if (jObject.TryGetValue("_type", out typePropertyToken))
            {
                var typeString = (string)((JValue)typePropertyToken).Value;
                receivedSubclassInterface = typeNameToResourceInfoDict[typeString].InterfaceType;
            }

            JToken refUriToken;
            if (jObject.TryGetValue("_ref", out refUriToken))
            {
                var uriValue = (JValue)refUriToken;
                return CreateProxyFor((string)uriValue.Value, receivedSubclassInterface);
            }

            var createdType = receivedSubclassInterface;

            // Find matching type for interface, we simply do this by removing the "I" from the beginning
            if (createdType.IsInterface && typeof(IClientResource).IsAssignableFrom(createdType))
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
            // TODO: Check that response code is correct and content-type matches JSON. [KNS]
            var jsonString = Encoding.UTF8.GetString(this.webClient.DownloadData(uri));
            Console.WriteLine("Incoming data from " + uri + ":\r\n" + jsonString);
            return JToken.Parse(jsonString);
        }


        private string GetUriOfType(Type type)
        {
            return BaseUri + GetResourceInfoForType(type).UrlRelativePath;
        }


        private void InstantiateClientRepositories()
        {
            foreach (
                var prop in
                    GetType().GetProperties().Where(
                        x =>
                        x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof(ClientRepository<,>)))
            {
                var repositoryType = prop.PropertyType;
                prop.SetValue(this, Activator.CreateInstance(repositoryType, this), null);
            }
        }


        private JToken PutUri(string uri, JToken jsonData)
        {
            return UploadToUri(uri, jsonData, "PUT");
        }


        private JToken UploadToUri(string uri, JToken jsonData, string httpMethod)
        {
            var requestString = jsonData.ToString();

            Console.WriteLine(httpMethod + "ting data to " + uri + ":\r\n" + requestString);

            var requestBytes = Encoding.UTF8.GetBytes(requestString);
            var responseBytes = this.webClient.UploadData(uri, httpMethod, requestBytes);
            var responseString = Encoding.UTF8.GetString(responseBytes);

            Console.WriteLine("Received response from " + httpMethod + ":\t\n" + responseString);

            return JToken.Parse(responseString);
        }

        #region Nested type: ChangeExpressionArgumentVisitor

        /// <summary>
        /// This visitor will convert an expression like:
        ///    MusicalCritter x => x.BandName == "Hi"
        /// to
        ///    Critter x => ((MusicalCritter)x).BandName == "Hi"
        /// </summary>
        private class ChangeExpressionArgumentVisitor : ExpressionVisitor
        {
            private readonly Expression baseclassedArgument;
            private readonly Expression subclassedArgument;


            public ChangeExpressionArgumentVisitor(Expression subclassedArgument, Expression baseclassedArgument)
            {
                this.subclassedArgument = subclassedArgument;
                this.baseclassedArgument = baseclassedArgument;
            }


            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression == this.subclassedArgument)
                {
                    if (node.Member.DeclaringType == this.baseclassedArgument.Type)
                    {
                        // Don't need to do cast for accessing properties on base type..
                        return Expression.MakeMemberAccess(this.baseclassedArgument, node.Member);
                    }
                    return
                        Expression.MakeMemberAccess(
                            Expression.Convert(this.baseclassedArgument, this.subclassedArgument.Type), node.Member);
                }
                return base.VisitMember(node);
            }
        }

        #endregion
    }
}