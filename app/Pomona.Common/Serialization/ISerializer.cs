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

using System.IO;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface ISerializer
    {
        ISerializerWriter CreateWriter(TextWriter textWriter);
        void SerializeNode(ISerializerNode node, ISerializerWriter writer);

        void SerializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, ISerializerWriter writer,
                                  TypeSpec elementType);
    }

    public interface ISerializer<TWriter> : ISerializer
        where TWriter : ISerializerWriter
    {
        new TWriter CreateWriter(TextWriter textWriter);
        void SerializeNode(ISerializerNode node, TWriter writer);

        void SerializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, TWriter writer,
                                  TypeSpec elementType);
    }

    public interface ISerializerReader
    {
    }

    public interface IDeserializer
    {
        ISerializerReader CreateReader(TextReader textReader);

        object Deserialize(TextReader textReader, TypeSpec expectedBaseType, IDeserializationContext context,
                           object patchedObject = null);

        void DeserializeNode(IDeserializerNode node, ISerializerReader reader);

        void DeserializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext,
                                    ISerializerReader reader);
    }

    public interface IDeserializer<TReader> : IDeserializer
        where TReader : ISerializerReader
    {
        new TReader CreateReader(TextReader textReader);
        void DeserializeNode(IDeserializerNode node, TReader reader);
        void DeserializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, TReader reader);
    }
}