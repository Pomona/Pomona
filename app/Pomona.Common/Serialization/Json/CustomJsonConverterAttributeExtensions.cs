#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Newtonsoft.Json;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Json
{
    public static class CustomJsonConverterAttributeExtensions
    {
        public static JsonConverter GetCustomJsonConverter(this TypeSpec type)
        {
            return
                type.DeclaredAttributes.OfType<CustomJsonConverterAttribute>().MaybeFirst().Select(x => x.JsonConverter)
                    .OrDefault();
        }
    }
}
