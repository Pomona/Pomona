#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Newtonsoft.Json;

using Pomona.Common;

namespace Pomona.Schemas
{
    public class SchemaArrayItem
    {
        [JsonIgnore]
        public HttpMethod Access { get; set; }

        [JsonProperty(PropertyName = "access")]
        public string[] AccessMethodsAsArray
        {
            get { return Access == HttpMethod.Get ? null : Schema.HttpAccessModeToMethodsArray(Access); }
            set { Access = value == null ? HttpMethod.Get : Schema.MethodsArrayToHttpAccessMode(value); }
        }

        public string Type { get; set; }
    }
}

