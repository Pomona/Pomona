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
using System.IO;
using System.Text;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this ITextDeserializer deserializer,
            TextReader textReader,
            DeserializeOptions options = null)
        {
            options = options ?? new DeserializeOptions();
            if (options.ExpectedBaseType == null)
                options.ExpectedBaseType = typeof(T);
            return (T)deserializer.Deserialize(textReader, options);
        }


        public static T DeserializeFromString<T>(this ITextDeserializer deserializer,
            string serializedObj,
            DeserializeOptions options = null)
        {
            if (deserializer == null)
                throw new ArgumentNullException("deserializer");
            using (var textReader = new StringReader(serializedObj))
            {
                return deserializer.Deserialize<T>(textReader, options);
            }
        }


        public static object DeserializeString(this ITextDeserializer deserializer,
            string serializedObj,
            DeserializeOptions options = null)
        {
            if (deserializer == null)
                throw new ArgumentNullException("deserializer");
            if (serializedObj == null)
                throw new ArgumentNullException("serializedObj");
            using (var textReader = new StringReader(serializedObj))
            {
                return deserializer.Deserialize(textReader, options);
            }
        }


        /// <summary>
        /// Serialize to a byte array.
        /// </summary>
        /// <param name="serializer">The serializer to use.</param>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="options">Serialization options. Is optional.</param>
        /// <param name="encoding">Text encoding. Optional, UTF-8 by default.</param>
        /// <returns>The serialized byte array.</returns>
        public static byte[] SerializeToBytes(this ITextSerializer serializer,
            object obj,
            SerializeOptions options = null,
            Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            using (var memStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memStream, encoding))
                {
                    serializer.Serialize(streamWriter, obj, options);
                }
                return memStream.ToArray();
            }
        }


        public static string SerializeToString(this ITextSerializer serializer,
            object obj,
            SerializeOptions options = null)
        {
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, obj, options);
                return sw.ToString();
            }
        }
    }
}