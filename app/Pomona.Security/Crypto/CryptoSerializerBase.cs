#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

using Pomona.Security.Authentication;

namespace Pomona.Security.Crypto
{
    public abstract class CryptoSerializerBase : ICryptoSerializer
    {
        private readonly bool compressionEnabled = true;
        private readonly RandomNumberGenerator randomNumberGenerator;
        private readonly ISiteKeyProvider siteKeyProvider;


        public CryptoSerializerBase(ISiteKeyProvider siteKeyProvider, RandomNumberGenerator randomNumberGenerator)
        {
            if (siteKeyProvider == null)
                throw new ArgumentNullException("siteKeyProvider");
            if (randomNumberGenerator == null)
                throw new ArgumentNullException("randomNumberGenerator");
            this.siteKeyProvider = siteKeyProvider;
            this.randomNumberGenerator = randomNumberGenerator;
        }


        private byte[] KeyBytes
        {
            get { return this.siteKeyProvider.SiteKey; }
        }


        protected virtual SymmetricAlgorithm CreateSymmetricalAlgorithm()
        {
            var codec = Rijndael.Create();
            if (codec == null)
                throw new InvalidOperationException("What? Should not get a null crypto codec!");
            return codec;
        }


        private Stream CreateCompressStream(MemoryStream memStream)
        {
            if (this.compressionEnabled)
                return new DeflateStream(memStream, CompressionMode.Compress);
            return memStream;
        }


        private Stream CreateDecompressStream(MemoryStream memStream)
        {
            if (this.compressionEnabled)
                return new DeflateStream(memStream, CompressionMode.Decompress);
            return memStream;
        }


        private static byte[] DecodeUrlSafeBase64<T>(string hexString)
        {
            // Url-safe base64
            return Convert.FromBase64String(hexString.Replace('-', '+').Replace('_', '/').Replace('.', '='));
        }


        private T DeserializeUnprotected<T>(string hexString)
        {
            var encryptedTokenBytes = DecodeUrlSafeBase64<T>(hexString);
            using (var codec = GetInitializedCodec())
            {
                var blockSizeBytes = codec.BlockSize / 8; // Should be 128/8 = 16 bytes
                var iv = new byte[blockSizeBytes];
                Array.Copy(encryptedTokenBytes, iv, blockSizeBytes);
                codec.IV = iv;
                var cipherTextLength = encryptedTokenBytes.Length - blockSizeBytes;

                using (var decryptor = codec.CreateDecryptor(codec.Key, codec.IV))
                {
                    var plainBytes = decryptor.TransformFinalBlock(encryptedTokenBytes,
                                                                   blockSizeBytes
                                                                   /* skip iv bytes */,
                                                                   cipherTextLength);
                    using (var memStream = new MemoryStream(plainBytes))
                    {
                        using (var gzipStream = CreateDecompressStream(memStream))
                        {
                            using (var textReader = new StreamReader(gzipStream, Encoding.UTF8))
                                //using (var jsonReader = new JsonTextReader(textReader))
                            {
                                var str = textReader.ReadToEnd();
                                return
                                    JsonSerializer.CreateDefault(new JsonSerializerSettings()).Deserialize<T>(
                                        new JsonTextReader(new StringReader(str)));
                                //return JsonSerializer.CreateDefault(new JsonSerializerSettings()).Deserialize<T>(jsonReader);
                            }
                        }
                    }
                }
            }
        }


        private static string EncodeUrlSafeBase64(byte[] ivAndEncryptedMessageBytes)
        {
            return Convert.ToBase64String(ivAndEncryptedMessageBytes).Replace('+', '-').Replace('/', '_').Replace('=',
                                                                                                                  '.');
        }


        private SymmetricAlgorithm GetInitializedCodec()
        {
            var codec = CreateSymmetricalAlgorithm();

            codec.Key = KeyBytes;
            codec.Mode = CipherMode.CFB; // For arbitrary lenghts
            codec.Padding = PaddingMode.None;
            codec.FeedbackSize = 8;
            return codec;
        }


        public T Deserialize<T>(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            try
            {
                return DeserializeUnprotected<T>(hexString);
            }
            catch (Exception ex)
            {
                throw new SerializationException("Unable to deserialize encrypted string", ex);
            }
        }


        public string Serialize(object obj)
        {
            using (var memStream = new MemoryStream())
            {
                using (var gzipStream = CreateCompressStream(memStream))
                {
                    using (var textWriter = new StreamWriter(gzipStream, new UTF8Encoding(false)))
                    {
                        JsonSerializer.CreateDefault(new JsonSerializerSettings() { Formatting = Formatting.None })
                                      .Serialize(
                                          textWriter,
                                          obj);
                        textWriter.Flush();
                        gzipStream.Flush();
                    }
                }
                using (var codec = GetInitializedCodec())
                {
                    var iv = new byte[codec.BlockSize / 8];
                    this.randomNumberGenerator.GetBytes(iv);
                    codec.IV = iv;

                    //codec.IV = Enumerable.Repeat((byte)99, codec.BlockSize / 8).ToArray();
                    using (var encryptor = codec.CreateEncryptor(codec.Key, codec.IV))
                    {
                        var plainBytes = memStream.ToArray();
                        var encryptedMessageBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        var ivAndEncryptedMessageBytes = new byte[codec.IV.Length + encryptedMessageBytes.Length];
                        Array.Copy(codec.IV, ivAndEncryptedMessageBytes, codec.IV.Length);
                        Array.Copy(encryptedMessageBytes,
                                   0,
                                   ivAndEncryptedMessageBytes,
                                   codec.IV.Length,
                                   encryptedMessageBytes.Length);
                        return EncodeUrlSafeBase64(ivAndEncryptedMessageBytes);
                    }
                }
            }
        }
    }
}