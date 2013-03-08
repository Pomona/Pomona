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
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.Internals
{
    public static class TypeUtils
    {
        private static readonly HashSet<Type> jsonSupportedNativeTypes = new HashSet<Type>
            {
                typeof (string),
                typeof (int),
                typeof (long),
                typeof (double),
                typeof (float),
                typeof (decimal),
                typeof (DateTime),
                typeof (object),
                typeof (bool),
                typeof (Guid),
                typeof (Uri)
            };

        public static IEnumerable<Type> GetNativeTypes()
        {
            return jsonSupportedNativeTypes;
        }


        public static IEnumerable<Type> AllBaseTypesAndInterfaces(Type type)
        {
            var baseType = type;

            while (baseType != null && baseType != typeof (object))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }

            foreach (var interfaceType in type.GetInterfaces())
                yield return interfaceType;
        }


        /// <summary>
        /// Checks whether type is of genericTypeToSearchFor or implements interface,
        /// if this succeeds return true and generic arguments.
        /// </summary>
        /// <param name="checkType">Type to check</param>
        /// <param name="genericTypeToSearchFor">Generic type to whether checkType implements</param>
        /// <param name="genericArguments">Generic arguments for T</param>
        /// <returns>true if type implements , false if not</returns>
        public static bool TryGetTypeArguments(
            Type checkType, Type genericTypeToSearchFor, out Type[] genericArguments)
        {
            genericArguments = null;

            var genericTypeImplementation =
                AllBaseTypesAndInterfaces(checkType).FirstOrDefault(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == genericTypeToSearchFor);

            if (genericTypeImplementation == null)
                return false;

            genericArguments = genericTypeImplementation.GetGenericArguments();
            return true;
        }
    }
}