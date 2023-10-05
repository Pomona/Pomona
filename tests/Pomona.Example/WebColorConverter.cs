#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Newtonsoft.Json;

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class WebColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(WebColor);
        }


        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var webColorString = reader.ReadAsString();
            return new WebColor(webColorString);
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var webColor = (WebColor)value;
            writer.WriteValue(webColor.ToStringConverted());
        }
    }
}

