#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.IO;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Csv
{
    public abstract class TextSerializerBase<TWriter> : ITextSerializer
    {
        internal void Serialize(
    ISerializationContext serializationContext,
    object obj,
    TextWriter textWriter,
    TypeSpec expectedBaseType)
        {
            var serializer = this;
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");

            var writer = serializer.CreateWriter(textWriter);
            if (obj is QueryResult)
                serializer.SerializeQueryResult((QueryResult)obj, serializationContext, writer, null);
            else
            {
                var itemValueNode = new ItemValueSerializerNode(obj,
                    expectedBaseType,
                    string.Empty,
                    serializationContext,
                    null);
                serializer.SerializeNode(itemValueNode, writer);
            }
        }

        protected void SerializeThroughContext(ISerializerNode node, TWriter writer)
        {
            node.Context.Serialize(node, n => SerializeNode(n, writer));
        }


        protected abstract void SerializeNode(ISerializerNode node, TWriter writer);

        protected abstract void SerializeQueryResult(QueryResult queryResult,
            ISerializationContext context,
            TWriter writer,
            TypeSpec elementType);

        protected abstract TWriter CreateWriter(TextWriter textWriter);

        public abstract void Serialize(TextWriter textWriter, object o, SerializeOptions options = null);
    }
#if false
    public class PomonaCsvSerializer : ITextSerializer
    {
        private readonly ISerializationContextProvider contextProvider;


        public PomonaCsvSerializer(ISerializationContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException("contextProvider");
            this.contextProvider = contextProvider;
        }


        private Writer CreateWriter(TextWriter textWriter)
        {
            return new Writer(textWriter);
        }


        public void Serialize(TextWriter textWriter, object o, SerializeOptions options = null)
        {
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");
            options = options ?? new SerializeOptions();
            var serializationContext = this.contextProvider.GetSerializationContext(options);
            this.Serialize(serializationContext,
                o,
                textWriter,
                options.ExpectedBaseType != null ? serializationContext.GetClassMapping(options.ExpectedBaseType) : null);
        }


        public void SerializeNode(ISerializerNode node, Writer writer)
        {
            if (node.ValueType.SerializationMode != TypeSerializationMode.Array)
                throw new NotSupportedException("When serializing to CSV we only support array");

            var t = node.ValueType as EnumerableTypeSpec;

            if (t == null)
                throw new NotImplementedException();

            var elementType = (TypeSpec)t.ItemType;
            var valueProperties =
                elementType.Properties
                    .Where(x => x.PropertyType.SerializationMode == TypeSerializationMode.Value)
                    .Where(x => node.Context.PropertyIsSerialized(x))
                    .ToList();

            foreach (var item in (IEnumerable)node.Value)
            {
                var colIndex = 0;
                foreach (var prop in valueProperties)
                {
                    if (colIndex > 0)
                        writer.TextWriter.Write(';');

                    writer.TextWriter.Write(prop.Getter(item));

                    colIndex++;
                }
                writer.TextWriter.WriteLine();
            }
        }


        public void SerializeNode(ISerializerNode node, ISerializerWriter writer)
        {
            SerializeNode(node, CastWriter(writer));
        }


        private void SerializeQueryResult(QueryResult queryResult,
            ISerializationContext fetchContext,
            Writer writer,
            TypeSpec elementType)
        {
            var itemNode = new ItemValueSerializerNode(queryResult,
                fetchContext.GetClassMapping(queryResult.ListType),
                string.Empty,
                fetchContext,
                null);
            SerializeThroughContext(itemNode, writer);
        }


        private void SerializeThroughContext(ISerializerNode node, Writer writer)
        {
            node.Context.Serialize(node, n => SerializeNode(n, writer));
        }

        private Writer CastWriter(ISerializerWriter writer)
        {
            var castedWriter = writer as Writer;
            if (castedWriter == null)
                throw new ArgumentException("Writer required to be of type PomonaJsonSerializationWriter", "writer");
            return castedWriter;
        }


        #region Nested type: Writer

        public class Writer : ISerializerWriter
        {
            private readonly TextWriter textWriter;


            public Writer(TextWriter textWriter)
            {
                this.textWriter = textWriter;
            }


            public TextWriter TextWriter
            {
                get { return this.textWriter; }
            }
        }

        #endregion
    }
#endif
}