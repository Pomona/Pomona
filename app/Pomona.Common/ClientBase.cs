#region License

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Internals;

namespace Pomona.Common
{
    public abstract class ClientBase : IPomonaClient
    {
        internal ClientBase()
        {
        }


        public abstract string BaseUri { get; }

        public abstract object DownloadString(string uri, Type type);
        public abstract T Get<T>(string uri);
        public abstract string GetUriOfType(Type type);


        public abstract IList<T> List<T>(string expand = null)
            where T : IClientResource;


        public abstract object Post<T>(Action<T> postAction)
            where T : class, IClientResource;


        public abstract T Put<T>(T target, Action<T> updateAction)
            where T : IClientResource;


        public abstract IList<T> Query<T>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null);


        public abstract IList<T> Query<T>(
            string uri,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null);


        public abstract bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo);


        internal abstract object Post<T>(string uri, Action<T> postAction)
            where T : class, IClientResource;

        internal abstract object Post<T>(string uri, T postForm)
            where T : class, IClientResource;

        internal abstract T Put<T>(string uri, T target, Action<T> updateAction)
            where T : class, IClientResource;
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

        private static readonly MethodInfo queryWithUriMethod;
        private static readonly ReadOnlyDictionary<string, ResourceInfoAttribute> typeNameToResourceInfoDict;
        private readonly string baseUri;
        private readonly JsonSerializer jsonSerializer;
        private readonly ISerializer serializer;
        private readonly ISerializerFactory serializerFactory;
        private readonly ClientTypeMapper typeMapper;
        private readonly WebClient webClient = new WebClient();


        static ClientBase()
        {
            createListOfTypeMethod =
                new GenericMethodCaller<ClientBase<TClient>, IEnumerable, object>(
                    ReflectionHelper.GetGenericMethodDefinition<ClientBase<TClient>>(
                        x => x.CreateListOfTypeGeneric<object>(null)));

            queryWithUriMethod =
                typeof(ClientBase<TClient>).GetMethods().Single(
                    x => x.Name == "Query" && x.GetParameters().Count() == 7);

            // Preload resource info attributes..
            var resourceTypes =
                typeof(TClient).Assembly.GetTypes().Where(x => typeof(IClientResource).IsAssignableFrom(x));

            var interfaceDict = new Dictionary<Type, ResourceInfoAttribute>();
            var typeNameDict = new Dictionary<string, ResourceInfoAttribute>();
            foreach (
                var resourceInfo in
                    resourceTypes.SelectMany(
                        x =>
                        x.GetCustomAttributes(typeof(ResourceInfoAttribute), false).OfType<ResourceInfoAttribute>())
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
            this.jsonSerializer = new JsonSerializer();
            this.jsonSerializer.Converters.Add(new StringEnumConverter());

            this.baseUri = baseUri;
            // BaseUri = "http://localhost:2211/";

            this.typeMapper = new ClientTypeMapper(ResourceTypes);
            this.serializerFactory = new PomonaJsonSerializerFactory();
            this.serializer = this.serializerFactory.GetSerialier();
            InstantiateClientRepositories();
        }


        public static IEnumerable<Type> ResourceTypes
        {
            get { return interfaceToResourceInfoDict.Keys; }
        }

        public override string BaseUri
        {
            get { return this.baseUri; }
        }


        public override object DownloadString(string uri, Type type)
        {
            return Deserialize(GetString(uri), type);
        }


        public override T Get<T>(string uri)
        {
            Log("Fetching uri {0}", uri);
            return (T)Deserialize(GetString(uri), typeof(T));
        }


        public override string GetUriOfType(Type type)
        {
            return BaseUri + this.GetResourceInfoForType(type).UrlRelativePath;
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

                return Get<IList<T>>(uri);
            }
            else
                throw new NotImplementedException("We expect an interface as Type parameter!");
        }


        public override object Post<T>(Action<T> postAction)
        {
            return Post(DownloadString(typeof(T)), postAction);
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
            var responseJson = UploadToUri(
                ((IHasResourceUri)target).Uri, updateProxy, typeof(T), "PUT");
            return (T)Deserialize(responseJson, null);
        }


        public override IList<T> Query<T>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null)
        {
            var uri = BaseUri + GetRelativeUriForType(typeof(T));
            return Query(uri, predicate, orderBy, sortOrder, top, skip, expand);
        }


        public override IList<T> Query<T>(
            string uri,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null)
        {
            ResourceInfoAttribute resourceInfo;

            var type = typeof(T);
            if (!TryGetResourceInfoForType(type, out resourceInfo))
                return QueryInheritedCustomType(uri, predicate, orderBy, sortOrder, top, skip, expand);

            resourceInfo = this.GetResourceInfoForType(type);
            if (!resourceInfo.IsUriBaseType)
            {
                // If we get an expression operating on a subclass of the URI base type, we need to modify it (casting)
                // TODO: Optimize this by caching MethodInfo or make this method non-generic.
                var transformedExpression = ChangeExpressionArgumentToType(predicate, resourceInfo.UriBaseType);
                var results = (IEnumerable)queryWithUriMethod.MakeGenericMethod(resourceInfo.UriBaseType).Invoke(
                    this, new object[] { uri, transformedExpression, orderBy, sortOrder, top, skip, expand });
                return new List<T>(results.OfType<T>());
            }

            var uriQueryBuilder = new UriQueryBuilder();

            uriQueryBuilder.AppendExpressionParameter("$filter", predicate);
            if (orderBy != null)
            {
                uriQueryBuilder.AppendExpressionParameter(
                    "$orderby",
                    RemoveCastOfResult(orderBy),
                    x => sortOrder == SortOrder.Descending ? x + " desc" : x);
            }

            if (expand != null)
                uriQueryBuilder.AppendParameter("$expand", expand);

            if (top.HasValue)
                uriQueryBuilder.AppendParameter("$top", top.Value);

            if (skip.HasValue)
                uriQueryBuilder.AppendParameter("$skip", skip.Value);

            uri = uri + "?" + uriQueryBuilder;

            return Get<IList<T>>(uri);
        }


        public override bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return interfaceToResourceInfoDict.TryGetValue(type, out resourceInfo);
        }


        public string DownloadString(Type type)
        {
            return BaseUri + GetRelativeUriForType(type);
        }


        public string GetRelativeUriForType(Type type)
        {
            var resourceInfo = this.GetResourceInfoForType(type);
            return resourceInfo.UrlRelativePath;
        }


        internal override object Post<T>(string uri, Action<T> postAction)
        {
            return Post(uri, null, postAction);
        }


        private object Post<T>(string uri, T form, Action<T> postAction)
            where T : class
        {
            var type = typeof(T);
            // TODO: T needs to be an interface, not sure how we fix this, maybe generate one Update method for every entity
            if (!type.IsInterface)
                throw new InvalidOperationException("postAction needs to operate on the interface of the entity");

            var resourceInfo = this.GetResourceInfoForType(type);

            var newType = resourceInfo.PostFormType;

            if (form == null)
                form = (T)Activator.CreateInstance(newType);

            if (postAction != null)
            {
                postAction(form);
            }

            // TODO: Implement baseuri property or something.

            // Post the json!
            var response = UploadToUri(uri, form, type, "POST");

            return Deserialize(response, null);
        }

        internal override object Post<T>(string uri, T postForm)
        {
            return Post(uri, postForm, null);
        }


        internal override T Put<T>(string uri, T target, Action<T> updateAction)
        {
            throw new NotImplementedException();
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


        private static LambdaExpression RemoveCastOfResult(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                var convertExpr = (UnaryExpression)lambda.Body;
                lambda = Expression.Lambda(convertExpr.Operand, lambda.Parameters);
            }
            return lambda;
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
                    if (typeValue.Type == JTokenType.String && (string)((JValue)typeValue).Value == "__result__")
                    {
                        JToken itemsToken;
                        if (!jObject.TryGetValue("items", out itemsToken))
                            throw new InvalidOperationException("Got result object, but lacking items");
                        return Deserialize(itemsToken.ToString(), expectedType);
                    }
                }
            }

            var deserializer = this.serializerFactory.GetDeserializer();
            var context = new ClientDeserializationContext(this.typeMapper, this);
            var deserialized = deserializer.Deserialize(
                new StringReader(jsonString),
                expectedType != null
                    ? this.typeMapper.GetClassMapping(expectedType)
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


        private string GetString(string uri)
        {
            // TODO: Check that response code is correct and content-type matches JSON. [KNS]
            var jsonString = Encoding.UTF8.GetString(this.webClient.DownloadData(uri));
            Console.WriteLine("Incoming data from " + uri + ":\r\n" + jsonString);
            return jsonString;
        }


        private Type GetUpdateProxyForInterface(Type expectedType)
        {
            return this.GetResourceInfoForType(expectedType).PutFormType;
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


        private IList<T> QueryInheritedCustomType<T>(
            string uri,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy = null,
            SortOrder sortOrder = SortOrder.Ascending,
            int? top = null,
            int? skip = null,
            string expand = null)
        {
            var customType = typeof(T);

            if (!customType.IsInterface)
            {
                throw new ArgumentException(
                    "Custom type T is required to be an interface which inherits from a known client resource type.");
            }

            var serverKnownResourceInfo = GetLeafResourceInfo(customType);

            if (serverKnownResourceInfo == null)
            {
                throw new ArgumentException(
                    "Custom type T is required to be an interface which inherits from a known client resource type.");
            }

            var serverKnownType = serverKnownResourceInfo.InterfaceType;

            var newParameter = Expression.Parameter(serverKnownType, "x");
            var sourceParameter = predicate.Parameters.First();
            var attributesProperty =
                serverKnownType.GetProperties().First(
                    x => typeof(IDictionary<string, string>).IsAssignableFrom(x.PropertyType));

            var visitor = new InheritedCustomTypePropertyToAttributeAccessVisitor(
                sourceParameter, newParameter, attributesProperty);

            var newBody = visitor.Visit(predicate.Body);

            if (string.IsNullOrEmpty(expand))
                expand = attributesProperty.Name.ToLower();
            else
                expand = expand + "," + attributesProperty.Name.ToLower();

            var transformedExpression = Expression.Lambda(newBody, newParameter);

            var results = (IEnumerable)queryWithUriMethod.MakeGenericMethod(serverKnownType).Invoke(
                this, new object[] { uri, transformedExpression, orderBy, sortOrder, top, skip, expand });
            var resultsWrapper =
                results.Cast<object>().Select(
                    x =>
                    {
                        var proxy =
                            (ClientSideResourceProxyBase)
                            ((object)RuntimeProxyFactory<ClientSideResourceProxyBase, T>.Create());
                        proxy.AttributesProperty = attributesProperty;
                        proxy.ProxyTarget = x;
                        return (T)((object)proxy);
                    }).ToList();

            return resultsWrapper;
        }


        private string Serialize(object obj, Type expectedBaseType)
        {
            var stringWriter = new StringWriter();
            var writer = this.serializer.CreateWriter(stringWriter);
            var context = new ClientSerializationContext(this.typeMapper);
            var node = new ItemValueSerializerNode(obj, this.typeMapper.GetClassMapping(expectedBaseType), "", context);
            this.serializer.SerializeNode(node, writer);
            return stringWriter.ToString();
        }


        private string UploadToUri(string uri, object obj, Type expectedBaseType, string httpMethod)
        {
            var requestString = Serialize(obj, expectedBaseType);

            Console.WriteLine(httpMethod + "ting data to " + uri + ":\r\n" + requestString);

            var requestBytes = Encoding.UTF8.GetBytes(requestString);
            var responseBytes = this.webClient.UploadData(uri, httpMethod, requestBytes);
            var responseString = Encoding.UTF8.GetString(responseBytes);

            Console.WriteLine("Received response from " + httpMethod + ":\t\n" + responseString);

            return responseString;
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

        #region Nested type: InheritedCustomTypePropertyToAttributeAccessVisitor

        private class InheritedCustomTypePropertyToAttributeAccessVisitor : ExpressionVisitor
        {
            private readonly PropertyInfo attributesProperty;
            private readonly HashSet<Type> serverKnownInterfaces;
            private readonly ParameterExpression targetParameter;
            private ParameterExpression sourceParameter;


            public InheritedCustomTypePropertyToAttributeAccessVisitor(
                ParameterExpression sourceParameter,
                ParameterExpression targetParameter,
                PropertyInfo attributesProperty)
            {
                this.sourceParameter = sourceParameter;

                var targetType = targetParameter.Type;
                var clientAssembly = typeof(TClient).Assembly;
                this.serverKnownInterfaces =
                    new HashSet<Type>(
                        targetType.WrapAsEnumerable().Concat(
                            targetType.GetInterfaces().Where(x => x.Assembly == clientAssembly)));

                this.targetParameter = targetParameter;
                this.attributesProperty = attributesProperty;
            }


            protected override Expression VisitMember(MemberExpression node)
            {
                if (this.serverKnownInterfaces.Contains(node.Member.DeclaringType))
                    return base.VisitMember(node);

                var propInfo = node.Member as PropertyInfo;

                // TODO: Support attribute for specifying what name attribute should have in dictionary

                var attrName = propInfo.Name;

                return Expression.Call(
                    Expression.Property(this.targetParameter, this.attributesProperty),
                    OdataFunctionMapping.DictGetMethod,
                    Expression.Constant(attrName));

                throw new NotImplementedException();
            }
        }

        #endregion
    }
}