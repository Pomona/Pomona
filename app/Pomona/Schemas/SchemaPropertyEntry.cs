using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pomona.Schemas
{
    public class SchemaPropertyEntry
    {
        public bool Generated { get; set; }
        public IList<SchemaArrayItem> Items { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public bool Required { get; set; }

        public bool ReadOnly { get; set; }
        public string Type { get; set; }
    }
}