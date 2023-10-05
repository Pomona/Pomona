#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common
{
    public static class StringEnumExtensions
    {
        public static IEnumerable<TStringEnum> ScanStringEnumValues<TStringEnum>()
            where TStringEnum : struct, IStringEnum<TStringEnum>
        {
            var type = typeof(TStringEnum);
            return
                type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType == type).Select(
                    member => (TStringEnum)member.GetValue(null));
        }
    }
}
