#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;
using System.Text;

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


        public static T DeserializeString<T>(this ITextDeserializer deserializer,
                                             string serializedObj,
                                             DeserializeOptions options = null)
        {
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));

            if (serializedObj == null)
                throw new ArgumentNullException(nameof(serializedObj));

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
                throw new ArgumentNullException(nameof(deserializer));

            if (serializedObj == null)
                throw new ArgumentNullException(nameof(serializedObj));

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
            encoding = encoding ?? new UTF8Encoding(false);
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
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, obj, options);
                return stringWriter.ToString();
            }
        }
    }
}