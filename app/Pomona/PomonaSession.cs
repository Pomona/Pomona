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
        private static readonly MethodInfo patchGenericMethod;
        private readonly IPomonaDataSource dataSource;
        private readonly IDeserializer deserializer;
        private readonly ISerializer serializer;
        private readonly TypeMapper typeMapper;
        private readonly IPomonaUriResolver uriResolver;


        static PomonaSession()
        {
            postGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<IPomonaDataSource>(dst => dst.Post<object>(null)).
                                 GetGenericMethodDefinition();
            getByIdMethod = new GenericMethodCaller<IPomonaDataSource, object, object>(
                ReflectionHelper.GetGenericMethodDefinition<IPomonaDataSource>(dst => dst.GetById<object>(null)));
            patchGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<IPomonaDataSource>(dst => dst.Patch((object) null));
        }


        /// <summary>
        /// Constructor for PomonaSession.
        /// </summary>
        /// <param name="dataSource">Data source used for this session.</param>
        /// <param name="typeMapper">Typemapper for session.</param>
        /// <param name="baseUriGetter"> </param>
        /// <param name="uriResolver"></param>
        public PomonaSession(IPomonaDataSource dataSource, TypeMapper typeMapper, IPomonaUriResolver uriResolver)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.dataSource = dataSource;
            this.typeMapper = typeMapper;
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
            return
                new PomonaResponse(
                    new PomonaQuery(transformedType) {ExpandedPaths = expand, ResultType = transformedType}, o, this);
        }

        internal object GetResultByUri(string uri)
        {
            return uriResolver.GetResultByUri(uri);
        }


        public PomonaResponse GetPropertyAsJson(
            TransformedType transformedType, object id, string propertyName, string expand)
        {
            // Note this is NOT optimized, as we should make the API in a way where it's possible to select by parent id.
            propertyName = propertyName.ToLower();

            var o = GetById(transformedType, id);
            var mappedType = (TransformedType) typeMapper.GetClassMapping(o.GetType());

            var property = mappedType.Properties.First(x => x.Name.ToLower() == propertyName);

            var propertyValue = property.Getter(o);
            var propertyType = property.PropertyType;

            return
                new PomonaResponse(
                    new PomonaQuery(transformedType) {ExpandedPaths = expand, ResultType = propertyType}, propertyValue,
                    this);
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
                uriResolver.RelativeToAbsoluteUri(string.Format("{0}/{1}", transformedType.UriRelativePath,
                                                                transformedType.GetId(entity)));
        }


        public PomonaResponse PostJson(TransformedType transformedType, Stream readStream)
        {
            using (var textReader = new StreamReader(readStream))
            {
                return DeserializePostOrPatch(transformedType, textReader);
            }
        }

        private PomonaResponse DeserializePostOrPatch(TransformedType transformedType, TextReader textReader,
                                                      object patchedObject = null)
        {
            var deserializationContext = new ServerDeserializationContext(this);
            var postResource = deserializer.Deserialize(textReader, transformedType, deserializationContext,
                                                        patchedObject);

            var method = patchedObject != null ? patchGenericMethod : postGenericMethod;
            var postResponse = method.MakeGenericMethod(postResource.GetType())
                                                .Invoke(dataSource, new[] {postResource});

            return new PomonaResponse(new PomonaQuery(transformedType) {ExpandedPaths = string.Empty}, postResponse,
                                      this);
        }

        public PomonaResponse Query(IPomonaQuery query)
        {
            var queryResult = dataSource.Query(query);

            var pq = (PomonaQuery) query;
            if (pq.Projection == PomonaQuery.ProjectionType.First || pq.Projection == PomonaQuery.ProjectionType.FirstOrDefault)
            {
                var foundNoResults = queryResult.Count < 1;
                if (pq.Projection == PomonaQuery.ProjectionType.First && foundNoResults)
                    throw new InvalidOperationException("No resources found.");

                var firstResult = foundNoResults ? null : ((IEnumerable) queryResult).Cast<object>().First();
                return new PomonaResponse(query, firstResult, this);
            }
            return new PomonaResponse(query, queryResult, this);
        }


        public PomonaResponse PatchJson(
            TransformedType transformedType, object id, Stream readStream)
        {
            var o = GetById(transformedType, id);

            using (var textReader = new StreamReader(readStream))
            {
                var objType = (TransformedType) typeMapper.GetClassMapping(o.GetType());
                return DeserializePostOrPatch(objType, textReader, o);
            }
        }


        public void WriteClientLibrary(Stream stream, bool embedPomonaClient = true)
        {
            ClientLibGenerator.WriteClientLibrary(typeMapper, stream, embedPomonaClient);
        }


        private object GetById(TransformedType transformedType, object id)
        {
            // TODO: Maybe cache method instance?

            return getByIdMethod.Call(transformedType.MappedType, dataSource, id);
        }

    }
}