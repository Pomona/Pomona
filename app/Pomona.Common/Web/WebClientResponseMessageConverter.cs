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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    internal class WebClientResponseMessageConverter : WebClientRequestResponseMessageJsonConverterBase
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(WebClientResponseMessage).IsAssignableFrom(objectType);
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var statusCodeToken = jobj["statusCode"];
            var statusCode = statusCodeToken != null
                ? serializer.Deserialize<HttpStatusCode>(statusCodeToken.CreateReader())
                : HttpStatusCode.OK;
            var body = ReadBody(jobj);
            var headers = ReadHeaders(jobj, serializer);
            return new WebClientResponseMessage(body, statusCode, headers);
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resp = (WebClientResponseMessage)value;
            writer.WriteStartObject();
            writer.WritePropertyName("statusCode");
            writer.WriteValue(resp.StatusCode);
            WriteHeaders(writer, serializer, resp.Headers);
            WriteBody(writer, resp.Data, resp.Headers.ContentType);
            writer.WriteEndObject();
        }
    }
}