#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    public class HttpRequestMessageConverter : HttpMessageConverterBase
    {
        public HttpRequestMessageConverter()
            : this(null)
        {
        }


        public HttpRequestMessageConverter(HttpMessageContentWriter contentWriter)
            : base(contentWriter)
        {
        }


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