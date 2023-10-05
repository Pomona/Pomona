#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
                throw new ArgumentNullException(nameof(siteKeyProvider));
            if (randomNumberGenerator == null)
                throw new ArgumentNullException(nameof(randomNumberGenerator));
            this.siteKeyProvider = siteKeyProvider;
            this.randomNumberGenerator = randomNumberGenerator;
        }


        private byte[] KeyBytes => this.siteKeyProvider.SiteKey;


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
                throw new ArgumentNullException(nameof(hexString));
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

