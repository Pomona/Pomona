#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Newtonsoft.Json;

namespace Pomona.Security.Authentication
{
    public class UrlToken
    {
        [JsonProperty("e", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Expiration { get; set; }

        [JsonProperty("h")]
        public string Path { get; set; }
    }
}