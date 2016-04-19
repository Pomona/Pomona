#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Newtonsoft.Json;

namespace Pomona.Common.Serialization.Json
{
    public class CustomJsonConverterAttribute : Attribute
    {
        public CustomJsonConverterAttribute(JsonConverter jsonConverter)
        {
            if (jsonConverter == null)
                throw new ArgumentNullException(nameof(jsonConverter));
            JsonConverter = jsonConverter;
        }


        public JsonConverter JsonConverter { get; }
    }
}