#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetAllInheritedPropertiesFromInterface(this Type sourceType)
        {
            return
                sourceType
                    .WrapAsEnumerable()
                    .Concat(sourceType.GetInterfaces())
                    .SelectMany(x => x.GetProperties())
                    .Distinct();
        }


        public static PropertyInfo GetBaseDefinition(this PropertyInfo propertyInfo)
        {
            var method = propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true);
            if (method == null)
                return propertyInfo.NormalizeReflectedType();

            var baseMethod = method.GetBaseDefinition();
            if (method == baseMethod)
                return propertyInfo.NormalizeReflectedType();
            return baseMethod.DeclaringType.GetProperty(propertyInfo.Name,
                BindingFlags.Instance | BindingFlags.NonPublic |
                BindingFlags.Public);
        }


        public static MethodInfo GetDelegateInvokeMethod(this Type delegateType)
        {
            return delegateType.GetMethod("Invoke");
        }


        public static TAttribute GetFirstOrDefaultAttribute<TAttribute>(this MemberInfo member, bool inherit)
            where TAttribute : Attribute
        {
            return member.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().FirstOrDefault();
        }


        public static IEnumerable<Type> GetFullTypeHierarchy(this Type type)
        {
            return type.WalkTree(x => x.BaseType);
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


        public static Type GetPropertyOrFieldType(this MemberInfo memberInfo)
        {
            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
                return propInfo.PropertyType;
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.FieldType;
            throw new NotSupportedException("Can only get type from property or field.");
        }


        public static object GetPropertyOrFieldValue(this MemberInfo memberInfo, object obj)
        {
            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
                return propInfo.GetValue(obj, null);
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);
            throw new NotSupportedException("Can only get value from property or field.");
        }


        public static PropertyInfo GetPropertySearchInheritedInterfaces(this Type sourceType, string propertyName)
        {
            return
                sourceType
                    .WrapAsEnumerable()
                    .Concat(sourceType.GetInterfaces())
                    .Select(x => x.GetProperty(propertyName)).FirstOrDefault(x => x != null);
        }


        public static bool HasAttribute<TAttribute>(this MemberInfo member, bool inherit)
            where TAttribute : Attribute
        {
            return member.GetCustomAttributes(typeof(TAttribute), inherit).Any();
        }


        /// <summary>
        /// Invoke method without wrapping exceptions in TargetInvocationException
        /// </summary>
        /// <param name="method">the method to invoke</param>
        /// <param name="targetAndArgs">Array of arguments, starting with target instance for non-static methods.</param>
        /// <returns>Return value from invoked method.</returns>
        public static object InvokeDirect(this MethodInfo method, params object[] targetAndArgs)
        {
            IEnumerable<object> args = targetAndArgs;
            var argCount = targetAndArgs.Length;
            Expression target = null;
            if (!method.IsStatic)
            {
                if (targetAndArgs.Length < 1)
                {
                    throw new InvalidOperationException(
                        "Requires this as first argument when invoking instantiated method.");
                }
                target = Expression.Constant(targetAndArgs[0], method.ReflectedType);
                args = targetAndArgs.Skip(1);
                argCount--;
            }
            var parameters = method.GetParameters();
            if (parameters.Length != argCount)
                throw new InvalidOperationException("Unable to invoke method, got wrong number of arguments.");
            var exprs = parameters.Zip(args, (p, a) => (Expression)Expression.Constant(a, p.ParameterType));
            return
                Expression.Lambda<Func<object>>(Expression.Convert(Expression.Call(target, method, exprs),
                    typeof(object))).Compile()();
        }


        public static bool IsAnonymous(this TypeSpec type)
        {
            return type.Name.StartsWith("<>f__AnonymousType");
        }


        public static bool IsAnonymous(this Type type)
        {
            return type.Name.StartsWith("<>f__AnonymousType");
        }


        public static bool IsGenericInstanceOf(this Type type, params Type[] genericTypeDefinitions)
        {
            return genericTypeDefinitions.Any(x => x.UniqueToken() == type.UniqueToken());
        }


        public static bool IsGenericTypeInstance(this Type typeInstance, Type genTypeDef)
        {
            Type[] tmp;
            return TryExtractTypeArguments(typeInstance, genTypeDef, out tmp);
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
            return type.UniqueToken() == typeof(Nullable<>).UniqueToken();
        }


        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetAccessors(true).First(x => x != null).IsStatic;
        }


        /// <summary>
        /// Gets version of member where reflected type is same as declaring type.
        /// </summary>
        /// <returns>Normalized member.</returns>
        public static TMemberInfo NormalizeReflectedType<TMemberInfo>(this TMemberInfo memberInfo)
            where TMemberInfo : MemberInfo
        {
            if (memberInfo.DeclaringType == null || memberInfo.DeclaringType == memberInfo.ReflectedType)
                return memberInfo;

            return
                (TMemberInfo)memberInfo.DeclaringType.GetMember(memberInfo.Name,
                    BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public)
                    .First(x => x.MetadataToken == memberInfo.MetadataToken);
        }


        /// <summary>
        /// This method attempts to extract the type parameters of a given type, when viewed as a particular generic type.
        /// </summary>
        /// <param name="typeInstance">The type instance, which type args will be extracted from.</param>
        /// <param name="genTypeDef">The generic type definition of class or interface we want to extract type args for.</param>
        /// <param name="typeArgs">The outgoing type arguments.</param>
        /// <returns>true when typeInstance implements genTypeDef generically, false if not.</returns>
        public static bool TryExtractTypeArguments(this Type typeInstance, Type genTypeDef, out Type[] typeArgs)
        {
            if (typeInstance == null)
                throw new ArgumentNullException("typeInstance");
            if (genTypeDef == null)
                throw new ArgumentNullException("genTypeDef");
            if (!genTypeDef.IsGenericTypeDefinition)
                throw new ArgumentException("gentTypeDef is required to be a generic type definition.", "genTypeDef");

            if (typeInstance.IsGenericType && typeInstance.GetGenericTypeDefinition() == genTypeDef)
            {
                typeArgs = typeInstance.GetGenericArguments();
                return true;
            }

            if (genTypeDef.IsInterface)
            {
                foreach (var interfaceType in typeInstance.GetInterfaces())
                {
                    if (TryExtractTypeArguments(interfaceType, genTypeDef, out typeArgs))
                        return true;
                }
            }
            else
            {
                if (typeInstance.BaseType != null &&
                    typeInstance.BaseType.TryExtractTypeArguments(genTypeDef, out typeArgs))
                    return true;
            }

            typeArgs = null;
            return false;
        }


        /// <summary>
        /// This method makes an attempt to fill a list of generic parameters provided a wanted type.
        /// </summary>
        /// <param name="wantedType">The type that is wanted. For example IEnumerable&lt;T&gt;</param>
        /// <param name="actualType">The type we try to fill the type parameter with. For example IGrouping&lt;int, string&gt;</param>
        /// <param name="methodTypeArgs">An array with type parameters to be filled.</param>
        /// <param name="typeArgsWasResolved">One or more type arguments were resolved, which means that methodTypeArgs was changed</param>
        /// <returns>true for match, false if actualType could not match wantedType.</returns>
        public static bool TryFillGenericTypeParameters(
            Type wantedType,
            Type actualType,
            Type[] methodTypeArgs,
            out bool typeArgsWasResolved)
        {
            typeArgsWasResolved = false;
            if (wantedType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "Does not expect genDefArgType to be a generic type definition.",
                    "wantedType");
            }
            if (actualType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "Does not expect instanceArgType to be a generic type definition.",
                    "actualType");
            }

            if (wantedType.IsGenericParameter)
                wantedType = methodTypeArgs[wantedType.GenericParameterPosition];

            if (wantedType.IsGenericParameter)
            {
                var constraints = wantedType.GetGenericParameterConstraints();
                if (
                    !constraints.Select(x => SubstituteTypeParameters(x, methodTypeArgs))
                        .All(x => x.IsAssignableFrom(actualType)))
                    return false;

                if ((wantedType.GenericParameterAttributes &
                     GenericParameterAttributes.NotNullableValueTypeConstraint) ==
                    GenericParameterAttributes.NotNullableValueTypeConstraint && actualType.IsNullable())
                    return false;

                if ((wantedType.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) ==
                    GenericParameterAttributes.DefaultConstructorConstraint)
                {
                    if (!wantedType.IsValueType && actualType.GetConstructor(Type.EmptyTypes) == null)
                        return false;
                }

                typeArgsWasResolved = true;
                methodTypeArgs[wantedType.GenericParameterPosition] = actualType;

                return true;
            }

            if (!wantedType.IsGenericType)
            {
                if (!wantedType.IsAssignableFrom(actualType))
                    return false;
            }
            else
            {
                var wantedTypeArgs = wantedType.GetGenericArguments();
                Type[] actualTypeArgs;
                if (!actualType.TryExtractTypeArguments(wantedType.GetGenericTypeDefinition(), out actualTypeArgs))
                    return false;

                for (var i = 0; i < wantedTypeArgs.Length; i++)
                {
                    var wantedTypeArg = wantedTypeArgs[i];
                    var actualTypeArg = actualTypeArgs[i];

                    bool innerTypeArgsWasResolved;
                    if (
                        !TryFillGenericTypeParameters(wantedTypeArg,
                            actualTypeArg,
                            methodTypeArgs,
                            out innerTypeArgsWasResolved))
                        return false;

                    if (innerTypeArgsWasResolved)
                        typeArgsWasResolved = true;
                }
            }

            return true;
        }


        public static bool TryGetEnumerableElementType(this Type type, out Type elementType)
        {
            var enumerableInterface = type.GetInterfacesOfGeneric(typeof(IEnumerable<>)).FirstOrDefault();

            if (enumerableInterface != null)
                elementType = enumerableInterface.GetGenericArguments()[0];
            else
                elementType = null;
            return elementType != null;
        }


        public static bool TryGetPropertyByName(this TypeSpec type,
            string name,
            StringComparison stringComparison,
            out PropertySpec property)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (name == null)
                throw new ArgumentNullException("name");

            property = type.AllProperties.FirstOrDefault(x => String.Equals(x.Name, name, stringComparison));
            return property != null;
        }


        public static bool TryGetPropertyByUriName(this ResourceType type, string name, out PropertySpec property)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (name == null)
                throw new ArgumentNullException("name");

            property =
                type.Properties.FirstOrDefault(
                    x => (x.PropertyType is ResourceType || x.PropertyType is EnumerableTypeSpec) &&
                         String.Equals(name,
                             NameUtils.ConvertCamelCaseToUri(x.Name),
                             StringComparison.InvariantCultureIgnoreCase));

            return property != null;
        }


        public static bool TryResolveGenericMethod(this MethodInfo methodDefinition,
            Type[] argumentTypes,
            out MethodInfo method)
        {
            if (methodDefinition == null)
                throw new ArgumentNullException("methodDefinition");
            if (argumentTypes == null)
                throw new ArgumentNullException("argumentTypes");

            method = methodDefinition;
            var methodParameters = method.GetParameters();
            var methodTypeArgs = method.GetGenericArguments();

            if (methodParameters.Length != argumentTypes.Length)
                return false;

            var typeArgsWasResolved = false;
            for (var i = 0; i < argumentTypes.Length; i++)
            {
                var argType = argumentTypes[i];
                var param = methodParameters[i];

                bool innerTypeArgsWasResolved;
                if (
                    !TryFillGenericTypeParameters(param.ParameterType,
                        argType,
                        methodTypeArgs,
                        out innerTypeArgsWasResolved))
                    return false;

                typeArgsWasResolved = typeArgsWasResolved || innerTypeArgsWasResolved;
            }

            if (typeArgsWasResolved)
            {
                // Upgrade to real method when all type args are resolved!!
                method = methodDefinition.MakeGenericMethod(methodTypeArgs);
                methodParameters = method.GetParameters();
            }

            if (methodTypeArgs.Any(x => x.IsGenericParameter))
                return false;

            return true;
        }


        public static UniqueMemberToken UniqueToken(this MemberInfo member)
        {
            return UniqueMemberToken.FromMemberInfo(member);
        }


        private static Type SubstituteTypeParameters(Type type, Type[] methodTypeArgs)
        {
            if (type.IsGenericParameter)
                return methodTypeArgs[type.GenericParameterPosition];

            if (!type.IsGenericType)
                return type;

            var genArgs = type.GetGenericArguments();
            var genericArgsReplaced = false;

            for (var i = 0; i < genArgs.Length; i++)
            {
                var original = genArgs[i];
                var substitute = SubstituteTypeParameters(original, methodTypeArgs);
                if (original != substitute)
                {
                    genArgs[i] = substitute;
                    genericArgsReplaced = true;
                }
            }

            if (genericArgsReplaced)
                return type.GetGenericTypeDefinition().MakeGenericType(genArgs);

            return type;
        }
    }
}