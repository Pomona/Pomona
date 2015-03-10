using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona.Common.Web
{
    internal class HttpHeadersConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var headers = (HttpHeaders)value;
            foreach (var kvp in headers)
            {
                var values = kvp.Value;
                if (values.Count == 0)
                    continue;

                writer.WritePropertyName(kvp.Key);
                if (values.Count == 1)
                {
                    writer.WriteValue(values[0]);
                }
                else
                {
                    serializer.Serialize(writer, values);
                }
            }
            writer.WriteEndObject();
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return
                new HttpHeaders(
                    serializer.Deserialize<Dictionary<string, object>>(reader)
                              .Select(
                                  x =>
                                      new KeyValuePair<string, IEnumerable<string>>(x.Key, SelectHeaderValue(x.Value))));
        }


        private IEnumerable<string> SelectHeaderValue(object value)
        {
            try
            {
                var arr = value as JArray;
                if (arr != null)
                    return arr.Select(x => (string)x);
                return new[] { (string)value };
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Unable to decode header values from JSON.", ex);
            }
        }


        public override bool CanConvert(Type objectType)
        {
            return typeof(HttpHeaders).IsAssignableFrom(objectType);
        }
    }
}