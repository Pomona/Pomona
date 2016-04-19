#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

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
    public class HttpMessageContentWriter
    {
        public virtual void Write(JsonWriter writer, HttpContent content)
        {
            if (content == null)
                return;

            var bytes = content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            IEnumerable<string> contentTypeValues;
            string contentTypeHeaderValue;
            if (content.Headers.TryGetValues("Content-Type", out contentTypeValues))
                contentTypeHeaderValue = contentTypeValues.Last();
            else
                contentTypeHeaderValue = "text/html; charset=utf-8";

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
                {
                    using (var sr = new StreamReader(ms, encoding))
                    {
                        str = sr.ReadToEnd();
                    }
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
                    //writer.WriteValue(str);

                    // Represent the text as an array of strings split at \r\n to make long text content easier to read.
                    WriteStringFomat(writer, contentType, str);
                }
            }
        }


        protected virtual void WriteStringFomat(JsonWriter writer, ContentType contentType, string str)
        {
            writer.WriteStartArray();
            StringBuilder sb = new StringBuilder();
            foreach (var c in str)
            {
                sb.Append(c);
                if (c == '\n')
                {
                    writer.WriteValue(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
                writer.WriteValue(sb.ToString());
            writer.WriteEndArray();
        }
    }
}