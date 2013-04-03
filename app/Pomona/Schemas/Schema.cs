using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pomona.Schemas
{
    public class Schema
    {
        public IList<SchemaTypeEntry> Types { get; set; }
        public string Version { get; set; }


        public Schema FromJson(string jsonString)
        {
            var serializer = GetSerializer();
            var stringReader = new StringReader(jsonString);
            var schema = (Schema) serializer.Deserialize(stringReader, typeof (Schema));
            // Fix property names (they're not serialized as this would be redundant)..
            foreach (var propKvp in schema.Types.SelectMany(x => x.Properties))
                propKvp.Value.Name = propKvp.Key;
            return schema;
        }


        public string ToJson()
        {
            var serializer = GetSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
        }


        private static JsonSerializer GetSerializer()
        {
            var serializer =
                JsonSerializer.Create(
                    new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                        });
            serializer.Formatting = Formatting.Indented;
            return serializer;
        }
    }
}