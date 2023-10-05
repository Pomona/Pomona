#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Newtonsoft.Json;

using Pomona.Common;

namespace Pomona.Schemas
{
    public class SchemaPropertyEntry
    {
        [JsonIgnore]
        public HttpMethod Access { get; set; }

        [JsonProperty(PropertyName = "access")]
        public string[] AccessMethodsAsArray
        {
            get { return Access == HttpMethod.Get ? null : Schema.HttpAccessModeToMethodsArray(Access); }
            set { Access = value == null ? HttpMethod.Get : Schema.MethodsArrayToHttpAccessMode(value); }
        }

        public IList<SchemaArrayItem> Items { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public bool Required { get; set; }
        public string Type { get; set; }
    }
}
