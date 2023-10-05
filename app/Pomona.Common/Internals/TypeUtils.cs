#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public static class TypeUtils
    {
        private static readonly HashSet<Type> jsonSupportedNativeTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(int),
            typeof(long),
            typeof(double),
            typeof(float),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(object),
            typeof(bool),
            typeof(Guid),
            typeof(Uri)
        };


        public static IEnumerable<Type> AllBaseTypesAndInterfaces(Type type)
        {
            var baseType = type;

            while (baseType != null && baseType != typeof(object))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }

            foreach (var interfaceType in type.GetInterfaces())
                yield return interfaceType;
        }


        public static IEnumerable<Type> GetNativeTypes()
        {
            return jsonSupportedNativeTypes;
        }


        public static Type[] GetTypes<T1>()
        {
            return new[] { typeof(T1) };
        }


        public static Type[] GetTypes<T1, T2>()
        {
            return new[] { typeof(T1), typeof(T2) };
        }


        public static Type[] GetTypes<T1, T2, T3>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3) };
        }


        public static Type[] GetTypes<T1, T2, T3, T4>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
        }


        public static Type[] GetTypes<T1, T2, T3, T4, T5>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
        }


        public static Type[] GetTypes<T1, T2, T3, T4, T5, T6>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
        }


        public static Type[] GetTypes<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
        }


        public static Type[] GetTypes<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return new[]
            {
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)
            };
        }


        public static Type ReplaceInGenericArguments(Type typeToSearch, Func<Type, Type> typeReplacer)
        {
            typeToSearch = typeReplacer(typeToSearch);

            if (typeToSearch.IsGenericType)
            {
                var genArgs = typeToSearch.GetGenericArguments();
                var newGenArgs =
                    genArgs.Select(x => ReplaceInGenericArguments(x, typeReplacer)).ToArray();

                if (newGenArgs.SequenceEqual(genArgs))
                    return typeToSearch;

                return typeToSearch.GetGenericTypeDefinition().MakeGenericType(newGenArgs);
            }

            return typeToSearch;
        }


        public static MethodInfo ReplaceInGenericMethod(MethodInfo methodToSearch, Func<Type, Type> typeReplacer)
        {
            if (!methodToSearch.IsGenericMethod)
                return methodToSearch;

            var genArgs = methodToSearch.GetGenericArguments();
            var newGenArgs = genArgs.Select(x => ReplaceInGenericArguments(x, typeReplacer)).ToArray();
            if (genArgs.SequenceEqual(newGenArgs))
                return methodToSearch;

            return methodToSearch.GetGenericMethodDefinition().MakeGenericMethod(newGenArgs);
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
            Type checkType,
            Type genericTypeToSearchFor,
            out Type[] genericArguments)
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

