#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public static class MappedTypeExtensions
    {
        private static readonly Type[] numberTypes;


        static MappedTypeExtensions()
        {
            numberTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(long), typeof(byte),
                typeof(short)
            };
        }


        public static string GetSchemaTypeName(this TypeSpec mappedType)
        {
            if (mappedType.IsCollection)
                return "array";

            var dictType = mappedType as DictionaryTypeSpec;
            if (dictType != null && dictType.KeyType == typeof(string))
                return "dictionary";

            if (mappedType.IsNullable)
                return GetSchemaTypeName(mappedType.ElementType);

            var sharedType = mappedType as RuntimeTypeSpec;
            if (sharedType != null)
            {
                var targetType = sharedType.Type;
                if (numberTypes.Contains(targetType))
                {
                    if (targetType == typeof(double) || targetType == typeof(float))
                        return "number";
                    return "integer";
                }

                if (targetType == typeof(string))
                    return "string";

                if (targetType == typeof(bool))
                    return "boolean";

                if (targetType == typeof(decimal))
                    return "decimal";

                if (targetType == typeof(object))
                    return "any";
            }

            return mappedType.Name;
        }
    }
}

