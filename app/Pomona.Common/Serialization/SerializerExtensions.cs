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
using System.IO;

namespace Pomona.Common.Serialization
{
    public static class SerializerExtensions
    {
        public static string SerializeToString(this ISerializer serializer, ISerializationContext serializationContext,
                                               object obj)
        {
            using (var strWriter = new StringWriter())
            {
                serializer.Serialize(serializationContext, obj, strWriter);
                return strWriter.ToString();
            }
        }

        public static void Serialize(this ISerializer serializer, ISerializationContext serializationContext, object obj,
                                     TextWriter textWriter)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (textWriter == null) throw new ArgumentNullException("textWriter");

            var writer = serializer.CreateWriter(textWriter);
            if (obj is QueryResult)
                serializer.SerializeQueryResult((QueryResult) obj, serializationContext, writer);
            else
            {
                var itemValueNode = new ItemValueSerializerNode(obj, null,
                                                                string.Empty,
                                                                serializationContext);
                serializer.SerializeNode(itemValueNode, writer);
            }
        }
    }
}