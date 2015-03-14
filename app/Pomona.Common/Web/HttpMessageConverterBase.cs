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
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    internal abstract class HttpMessageConverterBase : JsonConverter
    {
        protected static byte[] ReadBody(JObject jobj)
        {
            byte[] body;
            var bodyToken = jobj["body"];
            if (bodyToken == null || bodyToken.Type == JTokenType.Null)
                return null;
            var format = (string)jobj["format"] ?? "text";
            switch (format)
            {
                case "json":
                    body = Encoding.UTF8.GetBytes(bodyToken.ToString(Formatting.None));
                    break;
                case "binary":
                    body = Convert.FromBase64String((string)bodyToken);
                    break;
                case "text":
                    body = Encoding.UTF8.GetBytes((string)bodyToken);
                    break;
                default:
                    throw new JsonSerializationException("Format of request body " + format + " not recognized");
            }
            return body;
        }


        protected static HttpHeaders ReadHeaders(JObject jobj, JsonSerializer serializer)
        {
            JToken headersToken;
            HttpHeaders headers = null;
            if (jobj.TryGetValue("headers", out headersToken))
                headers = serializer.Deserialize<HttpHeaders>(jobj["headers"].CreateReader());
            return headers ?? new HttpHeaders();
        }


        protected static void WriteBody(JsonWriter writer, byte[] data, string contentTypeHeaderValue)
        {
            if (data == null)
                return;

            if (contentTypeHeaderValue == null)
                contentTypeHeaderValue = "text/html; charset=utf-8";

            writer.WritePropertyName("format");

            var contentType = new ContentType(contentTypeHeaderValue);
            bool formatAsBinary = false;
            var encoding = Encoding.ASCII;
            if (contentType.CharSet != null)
                encoding = Encoding.GetEncoding(contentType.CharSet);
            else
                formatAsBinary = data.Any(c => c == 0 || c > 127);
            if (formatAsBinary)
            {
                writer.WriteValue("binary");
                writer.WritePropertyName("body");
                writer.WriteValue(Convert.ToBase64String(data));
            }
            else
            {
                // Need to use memory stream to avoid UTF-8 BOM weirdness
                string str;
                using (var ms = new MemoryStream(data))
                using (var sr = new StreamReader(ms, encoding))
                {
                    str = sr.ReadToEnd();
                }

                if (contentType.MediaType == "application/json" || contentType.MediaType.EndsWith("+json"))
                {
                    var jtoken = JToken.Parse(str);
                    writer.WriteValue("json");
                    writer.WritePropertyName("body");
                    jtoken.WriteTo(writer);
                }
                else
                {
                    writer.WriteValue("text");
                    writer.WritePropertyName("body");
                    writer.WriteValue(str);
                }
            }
        }


        protected static void WriteHeaders(JsonWriter writer, JsonSerializer serializer, HttpHeaders headers)
        {
            if (headers.Any())
            {
                writer.WritePropertyName("headers");
                serializer.Serialize(writer, headers);
            }
        }
    }
}