using System.Collections.Generic;

namespace Pomona.Schemas
{
    public class SchemaTypeEntry
    {
        public string Uri { get; set; }
        public string Name { get; set; }
        public string Extends { get; set; }
        public IDictionary<string, SchemaPropertyEntry> Properties { get; set; }
    }
}