#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Newtonsoft.Json;

using Pomona.Common;

namespace Pomona.Schemas
{
    public class SchemaTypeEntry
    {
        public SchemaTypeEntry()
        {
            Properties = new Dictionary<string, SchemaPropertyEntry>();
        }


        public bool Abstract { get; set; }

        [JsonIgnore]
        public HttpMethod AllowedMethods { get; set; }

        [JsonProperty(PropertyName = "access")]
        public string[] AllowedMethodsAsArray
        {
            get { return AllowedMethods != 0 ? Schema.HttpAccessModeToMethodsArray(AllowedMethods) : null; }
            set { AllowedMethods = Schema.MethodsArrayToHttpAccessMode(value); }
        }

        public string Extends { get; set; }
        public string Name { get; set; }
        public IDictionary<string, SchemaPropertyEntry> Properties { get; set; }
        public string Uri { get; set; }
    }
}
