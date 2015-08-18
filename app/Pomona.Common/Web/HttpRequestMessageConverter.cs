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
using System.Net.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    public class HttpRequestMessageConverter : HttpMessageConverterBase
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(HttpRequestMessage).IsAssignableFrom(objectType);
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var url = (string)jobj["url"];
            var method = new System.Net.Http.HttpMethod((string)jobj["method"]);
            var request = new HttpRequestMessage(method, url);
            request.Content = ReadBody(jobj);
            ReadHeaders(jobj, serializer, request.Headers, request.Content);
            return request;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var req = (HttpRequestMessage)value;
            writer.WriteStartObject();
            writer.WritePropertyName("method");
            writer.WriteValue(req.Method.Method);
            writer.WritePropertyName("url");
            writer.WriteValue(req.RequestUri.ToString());
            WriteHeaders(writer, serializer, req.Headers, req.Content);
            WriteBody(writer, req.Content);
            writer.WriteEndObject();
        }
    }
}