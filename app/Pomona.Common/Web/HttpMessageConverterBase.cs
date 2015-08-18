using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    public abstract class HttpMessageConverterBase : JsonConverter
    {
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
                    body =  Encoding.UTF8.GetBytes((string)bodyToken);
                    break;
                default:
                    throw new JsonSerializationException("Format of request body " + format + " not recognized");
            }
            return new ByteArrayContent(body);
        }





        private readonly static HashSet<string> contentHeaderKeys = new HashSet<string>(new string[]
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

        protected static void ReadHeaders(JObject jobj, JsonSerializer serializer, System.Net.Http.Headers.HttpHeaders headers, HttpContent content)
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
                    {
                        headers.Add(key, values);
                    }
                }
            }
        }


        protected static void WriteBody(JsonWriter writer, HttpContent content)
        {
            if (content == null)
                return;

            var bytes = content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            IEnumerable<string> contentTypeValues;
            string contentTypeHeaderValue;
            if (content.Headers.TryGetValues("Content-Type", out contentTypeValues))
            {
                contentTypeHeaderValue = contentTypeValues.Last();
            }
            else
            {
                contentTypeHeaderValue = "text/html; charset=utf-8";
            }

            writer.WritePropertyName("format");

            var contentType = new ContentType(contentTypeHeaderValue);
            bool formatAsBinary = false;
            var encoding = Encoding.ASCII;
            if (contentType.CharSet != null)
                encoding = Encoding.GetEncoding(contentType.CharSet);
            else
                formatAsBinary = bytes.Any(c => c == 0 || c > 127);
            if (formatAsBinary)
            {
                writer.WriteValue("binary");
                writer.WritePropertyName("body");
                writer.WriteValue(Convert.ToBase64String(bytes));
            }
            else
            {
                // Need to use memory stream to avoid UTF-8 BOM weirdness
                string str;
                using (var ms = new MemoryStream(bytes))
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


        protected static void WriteHeaders(JsonWriter writer, JsonSerializer serializer, System.Net.Http.Headers.HttpHeaders headers, HttpContent content)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> allHeaders = headers;
            if (content != null)
            {
                allHeaders = allHeaders.Concat(content.Headers);
            }
            if (allHeaders.Any())
            {
                writer.WritePropertyName("headers");
                writer.WriteStartObject();
                foreach (var header in allHeaders)
                {
                    var key = header.Key;
                    var values = header.Value;
                    writer.WritePropertyName(key);
                    if (values.Count() == 1)
                    {
                        writer.WriteValue(values.First());
                    }
                    else
                    {
                        serializer.Serialize(writer, values, typeof(IEnumerable<KeyValuePair<string, IEnumerable<string>>>));
                    }
                }
                writer.WriteEndObject();
            }
        }
    }
}
