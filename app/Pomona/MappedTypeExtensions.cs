// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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
                    typeof (int), typeof (double), typeof (float), typeof (long), typeof (byte),
                    typeof (short)
                };
        }


        public static string GetSchemaTypeName(this IMappedType mappedType)
        {
            if (mappedType.IsCollection)
                return "array";

            if (mappedType.IsNullable)
                return GetSchemaTypeName(mappedType.ElementType);

            var sharedType = mappedType as SharedType;
            if (sharedType != null && sharedType.IsBasicWireType)
            {
                var targetType = sharedType.MappedType;
                if (numberTypes.Contains(targetType))
                {
                    if (targetType == typeof (double) || targetType == typeof (float))
                        return "number";
                    return "integer";
                }

                if (targetType == typeof (string))
                    return "string";

                if (targetType == typeof (bool))
                    return "boolean";

                if (targetType == typeof (decimal))
                    return "decimal";

                if (targetType == typeof (object))
                    return "any";
            }

            return mappedType.Name;
        }
    }
}