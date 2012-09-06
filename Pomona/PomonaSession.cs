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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pomona.Queries;

namespace Pomona
{
    /// <summary>
    /// A PomonaSession can be queried for data, and performs the necesarry serialization
    /// to and from JSON (for now).
    /// </summary>
    public class PomonaSession
    {
        private static readonly MethodInfo postGenericMethod;
        private static readonly MethodInfo queryGenericMethod;
        private readonly Func<Uri> baseUriGetter;
        private readonly IPomonaDataSource dataSource;
        private readonly TypeMapper typeMapper;


        static PomonaSession()
        {
            postGenericMethod =
                typeof (PomonaSession).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(
                    x => x.Name == "PostGeneric");
            queryGenericMethod =
                typeof (PomonaSession).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(
                    x => x.Name == "QueryGeneric");
        }


        /// <summary>
        /// Constructor for PomonaSession.
        /// </summary>
        /// <param name="dataSource">Data source used for this session.</param>
        /// <param name="typeMapper">Typemapper for session.</param>
        /// <param name="baseUriGetter"> </param>
        public PomonaSession(IPomonaDataSource dataSource, TypeMapper typeMapper, Func<Uri> baseUriGetter)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (baseUriGetter == null)
                throw new ArgumentNullException("baseUriGetter");
            this.dataSource = dataSource;
            this.typeMapper = typeMapper;
            this.baseUriGetter = baseUriGetter;
        }


        public TypeMapper TypeMapper
        {
            get { return typeMapper; }
        }


        public string GetAsJson<T>(object id, string expand)
        {
            using (var textWriter = new StringWriter())
            {
                GetAsJson<T>(id, expand, textWriter);
                textWriter.Flush();
                return textWriter.ToString();
            }
        }


        public void GetAsJson<T>(object id, string expand, TextWriter textWriter)
        {
            var o = dataSource.GetById<T>(id);
            var mappedType = typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(string.Format("{0},{1}", rootPath, expand), false, this);
            var wrapper = new ObjectWrapper(o, string.Empty, context, mappedType);
            wrapper.ToJson(textWriter);
        }


        public string GetPropertyAsJson<T>(object id, string propertyName, string expand)
        {
            using (var textWriter = new StringWriter())
            {
                GetPropertyAsJson<T>(id, propertyName, expand, textWriter);
                textWriter.Flush();
                return textWriter.ToString();
            }
        }


        public void GetPropertyAsJson<T>(object id, string propertyName, string expand, TextWriter textWriter)
        {
            // Note this is NOT optimized, as we should make the API in a way where it's possible to select by parent id.
            propertyName = propertyName.ToLower();

            var o = dataSource.GetById<T>(id);
            var mappedType = (TransformedType) typeMapper.GetClassMapping(o.GetType());

            var property = mappedType.Properties.First(x => x.Name.ToLower() == propertyName);

            var propertyValue = property.Getter(o);
            var propertyType = property.PropertyType;

            var rootPath = propertyName.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(string.Format("{0},{1}", rootPath, expand), false, this);

            var wrapper = context.CreateWrapperFor(propertyValue, rootPath, propertyType);
            wrapper.ToJson(textWriter);
        }


        public string GetUri(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            var transformedType = (TransformedType) TypeMapper.GetClassMapping(entity.GetType());

            return
                new Uri(
                    baseUriGetter(), "/" + transformedType.UriRelativePath + "/" + transformedType.GetId(entity)).
                    ToString();
        }


        public string ListAsJson<T>(string expand)
        {
            using (var textWriter = new StringWriter())
            {
                ListAsJson<T>(expand, textWriter);
                textWriter.Flush();
                return textWriter.ToString();
            }
        }


        public void ListAsJson<T>(string expand, TextWriter textWriter)
        {
            var o = dataSource.List<T>();
            var mappedType = typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.GenericArguments.First().Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(string.Format("{0},{1}", rootPath, expand), false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.ToJson(textWriter);
        }


        public string PostJson<T>(TextReader textReader)
        {
            using (var textWriter = new StringWriter())
            {
                PostJson<T>(textReader, textWriter);
                return textWriter.ToString();
            }
        }


        public void PostJson<T>(TextReader textReader, TextWriter textWriter)
        {
            var mappedType = (TransformedType) typeMapper.GetClassMapping<T>();
            var jObject = JObject.Load(new JsonTextReader(textReader));

            // A posted JSON object can either contain references to existing objects or
            // new objects that will be posted first.

            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var o = PostJsonInternal(mappedType, jObject);

            var context = new FetchContext(rootPath, false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.ToJson(textWriter);
        }


        private void QueryGeneric<T>(IPomonaQuery query, TextWriter writer)
        {
            var queryResult = dataSource.List<T>(query);
            var queryResultJsonConverter = new QueryResultJsonConverter<T>(this);

            queryResultJsonConverter.ToJson((PomonaQuery) query, queryResult, writer);
        }

        public void Query(IPomonaQuery query, TextWriter writer)
        {
            //var elementType = query.TargetType;
            var o = queryGenericMethod
                .MakeGenericMethod(query.TargetType.SourceType)
                .Invoke(this, new object[] {query, writer});
        }


        public void UpdateFromJson<T>(object id, TextReader textReader, TextWriter textWriter)
        {
            var o = dataSource.GetById<T>(id);
            var mappedType = typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(rootPath, false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.UpdateFromJson(textReader);
            wrapper.ToJson(textWriter);
        }


        public void WriteClientLibrary(Stream stream, bool embedPomonaClient = true)
        {
            var clientLibGenerator = new ClientLibGenerator(typeMapper);
            clientLibGenerator.PomonaClientEmbeddingEnabled = embedPomonaClient;
            clientLibGenerator.CreateClientDll(stream);
        }


        private object GetObjectFromUri(string refUri)
        {
            // HACK! This is not good enough for final solution of how we map urls..
            var uri = new Uri(refUri);

            var parts = uri.LocalPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var entityName = parts[0];
            var id = Convert.ToInt32(parts[1]); // HACK! Id hardcoded to be int.. ooops..

            // Now we need to find matching mapped type
            // Hmm.. Here the _type part could be used.
            var transformedType = TypeMapper.TransformedTypes.First(x => x.Name.ToLower() == entityName.ToLower());
            var sourceType = transformedType.SourceType;

            if (sourceType == null)
            {
                throw new InvalidOperationException(
                    "Don't know how to fetch TrasnformedType that has no SourceType set");
            }

            return dataSource.GetById(sourceType, id);
        }


        private object PostGeneric<T>(T objectToPost)
        {
            return dataSource.Post(objectToPost);
        }


        private object PostJsonInternal(TransformedType mappedType, JObject jObject)
        {
            if (!mappedType.PostAllowed)
                throw new UnauthorizedAccessException("Not allowed to post type " + mappedType.Name);

            // TODO: Support subtypes
            IDictionary<string, object> initValues = new Dictionary<string, object>();

            foreach (var jProp in jObject.Properties())
            {
                var mappedProperty = mappedType.GetPropertyByJsonName(jProp.Name);
                var propTransformedType = mappedProperty.PropertyType as TransformedType;
                object propValueToSet;
                if (propTransformedType != null)
                {
                    // We expect jProp to be an object
                    var jPropObject = jProp.Value as JObject;

                    if (jPropObject == null)
                    {
                        throw new PomonaSerializationException(
                            "The property " + jProp.Name
                            + " is of wrong type, expected to be a JSON object (dictionary).");
                    }

                    JToken refUriToken;
                    if (jPropObject.TryGetValue("_ref", out refUriToken))
                    {
                        if (!(refUriToken is JValue))
                            throw new PomonaSerializationException("Refuri must be a string with valid URI");

                        var refUri = (string) ((JValue) refUriToken).Value;

                        // HMMM How are we supposed to resolve the URI here?? It seems the URI routing stuff needs to go a bit deeper in the architecture..
                        // HACK HACK HACK For now assume that the URL ends with {entityname}/{id}
                        propValueToSet = GetObjectFromUri(refUri);
                    }
                    else
                    {
                        // Create a new one!
                        propValueToSet = PostJsonInternal(propTransformedType, jPropObject);
                    }
                }
                else if (mappedProperty.PropertyType.IsBasicWireType)
                    propValueToSet = ((JValue) jProp.Value).Value;
                else
                    throw new PomonaSerializationException("Don't know how to deserialize JSON property " + jProp.Name);

                initValues.Add(jProp.Name.ToLower(), propValueToSet);
            }

            var newInstance = mappedType.NewInstance(initValues);

            return postGenericMethod.MakeGenericMethod(newInstance.GetType()).Invoke(this, new[] {newInstance});
        }
    }
}