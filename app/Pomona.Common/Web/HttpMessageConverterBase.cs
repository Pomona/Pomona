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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    public abstract class HttpMessageConverterBase : JsonConverter
    {
        private static readonly HashSet<string> contentHeaderKeys = new HashSet<string>(new string[]
        {
            "Allow",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            "Content-Type",
            "Expires",
            "Last-Modified"
        }, StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> ignoredHeaders =
            new HashSet<string>(new[] { "Content-Length", "Expect", "Accept-Encoding", "Cache-Control" });

        private readonly HttpMessageContentWriter contentWriter;


        protected HttpMessageConverterBase(HttpMessageContentWriter contentWriter)
        {
            this.contentWriter = contentWriter ?? new HttpMessageContentWriter();
        }


        protected static HttpContent ReadBody(JObject jobj)
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
                    // When format is text and body is a JSON array containing only strings,
                    // the strings will be joined together to form the body.
                    var bodyAsArray = bodyToken as JArray;
                    if (bodyAsArray != null)
                    {
                        var sb = new StringBuilder();
                        foreach (var jstr in bodyAsArray)
                            sb.Append((string)jstr);
                        body = Encoding.UTF8.GetBytes(sb.ToString());
                    }
                    else
                        body = Encoding.UTF8.GetBytes((string)bodyToken);
                    break;
                default:
                    throw new JsonSerializationException("Format of request body " + format + " not recognized");
            }
            return new ByteArrayContent(body);
        }


        protected static void ReadHeaders(JObject jobj,
                                          JsonSerializer serializer,
                                          HttpHeaders headers,
                                          HttpContent content)
        {
            JToken headersToken;
            if (jobj.TryGetValue("headers", out headersToken))
            {
                var headersJobj = headersToken as JObject;
                if (headersJobj == null)
                    throw new JsonSerializationException("\"headers\" property must be a JSON object token.");
                foreach (var prop in headersJobj)
                {
                    var key = prop.Key;
                    if (IsIgnoredHeader(key))
                        continue;

                    var jValues = prop.Value;
                    List<string> values = new List<string>();
                    if (jValues.Type == JTokenType.Array)
                    {
                        foreach (var item in (JArray)jValues)
                        {
                            if (item.Type != JTokenType.String)
                                throw new JsonSerializationException("Header value must be a string.");
                            values.Add(item.Value<string>());
                        }
                    }
                    else
                    {
                        if (jValues.Type != JTokenType.String)
                            throw new JsonSerializationException("Header value must be a string.");
                        values.Add(jValues.Value<string>());
                    }
                    if (contentHeaderKeys.Contains(key))
                    {
                        if (content == null)
                            throw new JsonSerializationException("There is no content, unable to add content header " + key);
                        content.Headers.Add(key, values);
                    }
                    else
                        headers.Add(key, values);
                }
            }
        }


        protected void WriteBody(JsonWriter writer, HttpContent content)
        {
            this.contentWriter.Write(writer, content);
        }


        protected static void WriteHeaders(JsonWriter writer,
                                           JsonSerializer serializer,
                                           HttpHeaders headers,
                                           HttpContent content)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> allHeaders = headers;
            if (content != null)
                allHeaders = allHeaders.Concat(content.Headers);
            if (allHeaders.Any())
            {
                writer.WritePropertyName("headers");
                writer.WriteStartObject();
                foreach (var header in allHeaders)
                {
                    var key = header.Key;
                    if (IsIgnoredHeader(key))
                        continue;

                    var values = header.Value;
                    writer.WritePropertyName(key);
                    if (values.Count() == 1)
                        writer.WriteValue(values.First());
                    else
                        serializer.Serialize(writer, values, typeof(IEnumerable<KeyValuePair<string, IEnumerable<string>>>));
                }
                writer.WriteEndObject();
            }
        }


        private static bool IsIgnoredHeader(string key)
        {
            // Content-Length will be implicit by body.
            return ignoredHeaders.Contains(key);
        }
    }
}