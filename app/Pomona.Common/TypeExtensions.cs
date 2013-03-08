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
using System.Reflection;
using Pomona.Common.Internals;

namespace Pomona.Common
{
    public static class TypeExtensions
    {
        public static long UniqueToken(this MemberInfo member)
        {
            return (long)((ulong)member.Module.MetadataToken | ((ulong) member.MetadataToken) << 32);
        }
        public static IEnumerable<PropertyInfo> GetAllInheritedPropertiesFromInterface(this Type sourceType)
        {
            return
                sourceType
                    .WrapAsEnumerable()
                    .Concat(sourceType.GetInterfaces())
                    .SelectMany(x => x.GetProperties())
                    .Distinct();
        }


        public static bool IsAnonymous(this Type type)
        {
            return type.Name.StartsWith("<>f__AnonymousType");
        }


        public static bool IsGenericInstanceOf(this Type type, params Type[] genericTypeDefinitions)
        {
            return genericTypeDefinitions.Any(x => x.UniqueToken() == type.UniqueToken());
        }


        /// <summary>
        /// Here we check that a, which can be an open type with generic parameters
        /// is equivalent to b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsGenericallyEquivalentTo(this Type a, Type b)
        {
            if (a.IsGenericParameter)
            {
                if (b.IsGenericParameter)
                {
                    if (a.GenericParameterPosition != b.GenericParameterPosition)
                        return false;

                    if (a.GenericParameterAttributes != b.GenericParameterAttributes)
                        return false;

                    var aConstraints = a.GetGenericParameterConstraints();
                    var bConstraints = b.GetGenericParameterConstraints();

                    return aConstraints.Zip(bConstraints, IsGenericallyEquivalentTo).All(x => x);
                }
            }

            if (a.UniqueToken() != b.UniqueToken())
                return false;

            if (a.IsGenericType)
            {
                if (!b.IsGenericType)
                    return false;
                return
                    a.GetGenericArguments()
                     .Zip(b.GetGenericArguments(), IsGenericallyEquivalentTo)
                     .All(x => x);
            }

            return true;
        }

        public static bool IsNullable(this Type type)
        {
            return type.UniqueToken() == typeof (Nullable<>).UniqueToken();
        }

        public static IEnumerable<Type> GetInterfacesOfGeneric(this Type type, Type genericTypeDefinition)
        {
            var metadataToken = genericTypeDefinition.UniqueToken();
            return
                type
                    .WrapAsEnumerable()
                    .Concat(type.GetInterfaces())
                    .Where(t => t.UniqueToken() == metadataToken);
        }

        public static bool TryGetCollectionElementType(this Type type, out Type elementType)
        {
            var enumerableInterface = type.GetInterfacesOfGeneric(typeof (IEnumerable<>)).FirstOrDefault();

            if (enumerableInterface != null)
                elementType = enumerableInterface.GetGenericArguments()[0];
            else
                elementType = null;
            return elementType != null;
        }
    }
}