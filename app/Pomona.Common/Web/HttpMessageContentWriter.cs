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
    }
}