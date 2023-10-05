#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    public class HttpResponseMessageConverter : HttpMessageConverterBase
    {
        public HttpResponseMessageConverter()
            : this(null)
        {
        }


        public HttpResponseMessageConverter(HttpMessageContentWriter contentWriter)
            : base(contentWriter)
        {
        }


        public override bool CanConvert(Type objectType)
        {
            return typeof(HttpResponseMessage).IsAssignableFrom(objectType);
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var statusCodeToken = jobj["status"] ?? jobj["statusCode"];
            var statusCode = statusCodeToken != null
                ? serializer.Deserialize<HttpStatusCode>(statusCodeToken.CreateReader())
                : HttpStatusCode.OK;
            var response = new HttpResponseMessage(statusCode);
            response.Content = ReadBody(jobj);
            ReadHeaders(jobj, serializer, response.Headers, response.Content);
            return response;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resp = (HttpResponseMessage)value;
            writer.WriteStartObject();
            writer.WritePropertyName("status");
            writer.WriteValue(resp.StatusCode);
            WriteHeaders(writer, serializer, resp.Headers, resp.Content);
            WriteBody(writer, resp.Content);
            writer.WriteEndObject();
        }
    }
}
