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
using System.IO;
using System.Linq;
using System.Reflection;
using Pomona.CodeGen;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.Internals;

namespace Pomona
{
    /// <summary>
    /// A PomonaSession can be queried for data, and performs the necesarry serialization
    /// to and from JSON (for now).
    /// </summary>
    public class PomonaSession
    {
        private static readonly GenericMethodCaller<IPomonaDataSource, object, object> getByIdMethod;
        private static readonly MethodInfo postGenericMethod;
        private static readonly MethodInfo updateMethod;
        private readonly Func<Uri> baseUriGetter;
        private readonly IPomonaDataSource dataSource;
        private readonly IDeserializer deserializer;
        private readonly ISerializer serializer;
        private readonly TypeMapper typeMapper;
        private readonly IPomonaUriResolver uriResolver;


        static PomonaSession()
        {
            postGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<PomonaSession>(dst => dst.PostGeneric<object>(null)).
                                 GetGenericMethodDefinition();
            getByIdMethod = new GenericMethodCaller<IPomonaDataSource, object, object>(
                ReflectionHelper.GetGenericMethodDefinition<IPomonaDataSource>(dst => dst.GetById<object>(null)));
            updateMethod =
                ReflectionHelper.GetGenericMethodDefinition<IPomonaDataSource>(dst => dst.Update((object) null));
        }


        /// <summary>
        /// Constructor for PomonaSession.
        /// </summary>
        /// <param name="dataSource">Data source used for this session.</param>
        /// <param name="typeMapper">Typemapper for session.</param>
        /// <param name="baseUriGetter"> </param>
        /// <param name="uriResolver"></param>
        public PomonaSession(IPomonaDataSource dataSource, TypeMapper typeMapper, Func<Uri> baseUriGetter,
                             IPomonaUriResolver uriResolver)
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
            this.uriResolver = uriResolver;
            serializer = typeMapper.SerializerFactory.GetSerialier();
            deserializer = typeMapper.SerializerFactory.GetDeserializer();
        }


        public TypeMapper TypeMapper
        {
            get { return typeMapper; }
        }


        public PomonaResponse GetAsJson(TransformedType transformedType, object id, string expand)
        {
            var o = GetById(transformedType, id);
            return new PomonaResponse(new PomonaQuery(transformedType) {ExpandedPaths = expand}, o, this);
        }

        internal object GetResultByUri(string uri)
        {
            return uriResolver.GetResultByUri(uri);
        }

        [Obsolete]
        private void SerializeSingleObject(string expand, TextWriter textWriter, object o)
        {
            var mappedType = typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var context = new ServerSerializationContext(string.Format("{0},{1}", rootPath, expand), false, this);

            ISerializerWriter writer = null;
            try
            {
                writer = serializer.CreateWriter(textWriter);
                serializer.SerializeNode(
                    new ItemValueSerializerNode(o, null /*transformedType*/, string.Empty, context), writer);
            }
            finally
            {
                if (writer != null && writer is IDisposable)
                    ((IDisposable) writer).Dispose();
            }
        }


        public string GetPropertyAsJson(TransformedType transformedType, object id, string propertyName, string expand)
        {
            using (var textWriter = new StringWriter())
            {
                GetPropertyAsJson(transformedType, id, propertyName, expand, textWriter);
                textWriter.Flush();
                return textWriter.ToString();
            }
        }


        public void GetPropertyAsJson(
            TransformedType transformedType, object id, string propertyName, string expand, TextWriter textWriter)
        {
            // Note this is NOT optimized, as we should make the API in a way where it's possible to select by parent id.
            propertyName = propertyName.ToLower();

            var o = GetById(transformedType, id);
            var mappedType = (TransformedType) typeMapper.GetClassMapping(o.GetType());

            var property = mappedType.Properties.First(x => x.Name.ToLower() == propertyName);

            var propertyValue = property.Getter(o);
            var propertyType = property.PropertyType;

            var rootPath = propertyName.ToLower(); // We want paths to be case insensitive
            var context = new ServerSerializationContext(string.Format("{0},{1}", rootPath, expand), false, this);

            var wrapper = context.CreateWrapperFor(propertyValue, rootPath, propertyType);
            wrapper.ToJson(textWriter);
        }


        public string GetUri(IPropertyInfo property, object entity)
        {
            return GetUri(entity) + "/" + property.LowerCaseName;
        }


        public string GetUri(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            var transformedType = (TransformedType) TypeMapper.GetClassMapping(entity.GetType());

            return
                new Uri(
                    string.Format("{0}{1}/{2}", baseUriGetter(), transformedType.UriRelativePath,
                                  transformedType.GetId(entity))).
                    ToString();
        }


        public void PostJson(TransformedType transformedType, Stream readStream, Stream writeStream)
        {
            using (var textReader = new StreamReader(readStream))
            using (var textWriter = new StreamWriter(writeStream))
            {
                DeserializePostOrPatch(transformedType, textReader, textWriter);
            }
        }

        private void DeserializePostOrPatch(TransformedType transformedType, TextReader textReader,
                                            TextWriter textWriter, object patchedObject = null)
        {
            var deserializationContext = new ServerDeserializationContext(this);
            var postResource = deserializer.Deserialize(textReader, transformedType, deserializationContext,
                                                        patchedObject);
            var postResponse = postGenericMethod.MakeGenericMethod(postResource.GetType())
                                                .Invoke(this, new[] {postResource});
            var serializationContext = new ServerSerializationContext("", false, this);
            var writer = serializer.CreateWriter(textWriter);
            var node = new ItemValueSerializerNode(postResponse, /* transformedType.PostReturnType */ null, "",
                                                   serializationContext);
            serializer.SerializeNode(node, writer);
        }

        public PomonaResponse Query(IPomonaQuery query)
        {
            var queryResult = dataSource.Query(query);

            var pq = (PomonaQuery) query;
            if (pq.Projection == PomonaQuery.ProjectionType.First)
            {
                if (queryResult.Count < 1)
                    throw new InvalidOperationException("No resources found.");

                var firstResult = ((IEnumerable) queryResult).Cast<object>().First();
                return new PomonaResponse(query, firstResult, this);
            }
            return new PomonaResponse(query, queryResult, this);
        }

        [Obsolete("Remove this when serialization has been moved.")]
        public void Query(IPomonaQuery query, TextWriter writer)
        {
            //var elementType = query.TargetType;
            var queryResult = dataSource.Query(query);

            var context = new ServerSerializationContext(query.ExpandedPaths, false, this);
            var state = new PomonaJsonSerializer.Writer(writer);

            var pq = (PomonaQuery) query;
            if (pq.Projection == PomonaQuery.ProjectionType.First)
            {
                if (queryResult.Count < 1)
                    throw new InvalidOperationException("No resources found.");

                var firstResult = ((IEnumerable) queryResult).Cast<object>().First();
                var node = new ItemValueSerializerNode(firstResult, query.TargetType, query.ExpandedPaths, context);
                serializer.SerializeNode(node, state);
            }
            else
            {
                serializer.SerializeQueryResult(queryResult, context, state);
            }
        }


        public void UpdateFromJson(
            TransformedType transformedType, object id, Stream readStream, Stream writeStream)
        {
            var o = GetById(transformedType, id);

            using (var textReader = new StreamReader(readStream))
            using (var textWriter = new StreamWriter(writeStream))
            {
                var objType = (TransformedType) typeMapper.GetClassMapping(o.GetType());
                DeserializePostOrPatch(objType, textReader, textWriter, o);
            }
        }


        public void WriteClientLibrary(Stream stream, bool embedPomonaClient = true)
        {
            var clientLibGenerator = new ClientLibGenerator(typeMapper);
            clientLibGenerator.PomonaClientEmbeddingEnabled = embedPomonaClient;
            clientLibGenerator.CreateClientDll(stream);
        }


        private object GetById(TransformedType transformedType, object id)
        {
            // TODO: Maybe cache method instance?

            return getByIdMethod.Call(transformedType.MappedType, dataSource, id);
        }


        private object PostGeneric<T>(T objectToPost)
        {
            return dataSource.Post(objectToPost);
        }
    }
}