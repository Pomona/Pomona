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
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Csv
{
    public class PomonaCsvSerializer : ISerializer<PomonaCsvSerializer.Writer>
    {
        ISerializerWriter ISerializer.CreateWriter(TextWriter textWriter)
        {
            return CreateWriter(textWriter);
        }

        public void SerializeNode(ISerializerNode node, Writer writer)
        {
            if (node.ValueType.SerializationMode != TypeSerializationMode.Array)
                throw new NotSupportedException("When serializing to CSV we only support array");


            var elementType = node.ExpectedBaseType.ElementType;
            var valueProperties =
                elementType.Properties
                           .Where(x => x.PropertyType.SerializationMode == TypeSerializationMode.Value)
                           .Where(x => node.Context.PropertyIsSerialized(x))
                           .ToList();

            foreach (var item in (IEnumerable) node.Value)
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


        public void SerializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, Writer writer,
                                         IMappedType elementType)
        {
            var itemNode = new ItemValueSerializerNode(queryResult, fetchContext.GetClassMapping(queryResult.ListType),
                                                       string.Empty, fetchContext, null);
            itemNode.Serialize(this, writer);
        }

        public Writer CreateWriter(TextWriter textWriter)
        {
            return new Writer(textWriter);
        }

        public void SerializeNode(ISerializerNode node, ISerializerWriter writer)
        {
            SerializeNode(node, CastWriter(writer));
        }

        public void SerializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext,
                                         ISerializerWriter writer,
                                         IMappedType elementType)
        {
            SerializeQueryResult(queryResult, fetchContext, CastWriter(writer), elementType);
        }

        private Writer CastWriter(ISerializerWriter writer)
        {
            var castedWriter = writer as Writer;
            if (castedWriter == null)
                throw new ArgumentException("Writer required to be of type PomonaJsonSerializationWriter", "writer");
            return castedWriter;
        }

        public class Writer : ISerializerWriter
        {
            private readonly TextWriter textWriter;

            public Writer(TextWriter textWriter)
            {
                this.textWriter = textWriter;
            }

            public TextWriter TextWriter
            {
                get { return textWriter; }
            }
        }
    }
}