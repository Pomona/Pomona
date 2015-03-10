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
    public class PomonaCsvSerializer : TextSerializerBase<PomonaCsvSerializer.Writer>
    {
        private readonly ISerializationContextProvider contextProvider;


        public PomonaCsvSerializer(ISerializationContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException("contextProvider");
            this.contextProvider = contextProvider;
        }


        protected override Writer CreateWriter(TextWriter textWriter)
        {
            return new Writer(textWriter);
        }


        public override void Serialize(TextWriter textWriter, object o, SerializeOptions options = null)
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


        protected override void SerializeNode(ISerializerNode node, Writer writer)
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
                    .Where(x => x.IsSerialized)
                    .ToList();

            foreach (var item in (IEnumerable)node.Value)
            {
                var colIndex = 0;
                foreach (var prop in valueProperties)
                {
                    if (colIndex > 0)
                        writer.TextWriter.Write(';');

                    writer.TextWriter.Write(prop.GetValue(item, node.Context));

                    colIndex++;
                }
                writer.TextWriter.WriteLine();
            }
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
}