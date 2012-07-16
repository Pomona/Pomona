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
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona
{
    /// <summary>
    /// A PomonaSession can be queried for data, and performs the necesarry serialization
    /// to and from JSON (for now).
    /// </summary>
    public class PomonaSession
    {
        private readonly IPomonaDataSource dataSource;
        private readonly TypeMapper typeMapper;
        private readonly Func<object, string> uriResolver;


        /// <summary>
        /// Constructor for PomonaSession.
        /// </summary>
        /// <param name="dataSource">Data source used for this session.</param>
        /// <param name="typeMapper">Typemapper for session.</param>
        /// <param name="uriResolver">Lambda for uri resolver. TODO: Make this an interface.</param>
        public PomonaSession(IPomonaDataSource dataSource, TypeMapper typeMapper, Func<object, string> uriResolver)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (uriResolver == null)
                throw new ArgumentNullException("uriResolver");
            this.dataSource = dataSource;
            this.typeMapper = typeMapper;
            this.uriResolver = uriResolver;
        }


        public TypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }


        public void GetAsJson<T>(object id, string expand, TextWriter textWriter)
        {
            var o = this.dataSource.GetById<T>(id);
            var mappedType = this.typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(this.uriResolver, string.Format("{0},{1}", rootPath, expand), false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.ToJson(textWriter);
        }


        public void ListAsJson<T>(string expand, TextWriter textWriter)
        {
            var o = this.dataSource.List<T>();
            var mappedType = this.typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.GenericArguments.First().Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(this.uriResolver, string.Format("{0},{1}", rootPath, expand), false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.ToJson(textWriter);
        }


        public void PostJson<T>(TextReader textReader, TextWriter textWriter)
        {
            var mappedType = (TransformedType)this.typeMapper.GetClassMapping<T>();
            var newInstance =
                mappedType.NewInstance(
                    JObject.Load(new JsonTextReader(textReader)).Children().OfType<JProperty>().ToDictionary(
                        x => x.Name.ToLower(), x => ((JValue)x.Value).Value));
            var o = this.dataSource.Post(newInstance);
            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(this.uriResolver, rootPath, false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.ToJson(textWriter);
        }


        public void UpdateFromJson<T>(object id, TextReader textReader, TextWriter textWriter)
        {
            var o = this.dataSource.GetById<T>(id);
            var mappedType = this.typeMapper.GetClassMapping(o.GetType());
            var rootPath = mappedType.Name.ToLower(); // We want paths to be case insensitive
            var context = new FetchContext(this.uriResolver, rootPath, false, this);
            var wrapper = new ObjectWrapper(o, rootPath, context, mappedType);
            wrapper.UpdateFromJson(textReader);
            wrapper.ToJson(textWriter);
        }


        public void WriteClientLibrary(Stream stream)
        {
            var clientLibGenerator = new ClientLibGenerator(this.typeMapper);
            clientLibGenerator.CreateClientDll(stream);
        }
    }
}