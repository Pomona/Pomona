﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Internals.Formatting;
using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    public static class TypeExtensions
    {
        public static string GetPhysicalLocation(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var assemblyPath = assembly.Location;

            if (String.IsNullOrEmpty(assemblyPath))
            {
                var uri = new UriBuilder(assembly.CodeBase);
                var unescapeDataString = Uri.UnescapeDataString(uri.Path);
                assemblyPath = Path.GetFullPath(unescapeDataString);
            }

            return assemblyPath;
        }


        public static IEnumerable<PropertyInfo> GetAllInheritedPropertiesFromInterface(this Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            return sourceType
                .WrapAsEnumerable()
                .Concat(sourceType.GetInterfaces())
                .SelectMany(x => x.GetProperties())
                .Distinct();
        }


        public static PropertyInfo GetBaseDefinition(this PropertyInfo propertyInfo)
        {
            var virtualPropertyInfo = propertyInfo as VirtualPropertyInfo;
            if (virtualPropertyInfo != null)
                return virtualPropertyInfo.BaseDefinition;
            var method = propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true);
            if (method == null)
                return propertyInfo.NormalizeReflectedType();

            var baseMethod = method.GetBaseDefinition();
            if (method == baseMethod)
                return propertyInfo.NormalizeReflectedType();

            if (baseMethod.DeclaringType == null)
                throw new InvalidOperationException($"{baseMethod} has no declaring type.");

            return baseMethod.DeclaringType.GetProperty(propertyInfo.Name,
                                                        BindingFlags.Instance | BindingFlags.NonPublic |
                                                        BindingFlags.Public);
        }


        /// <summary>
        /// Gets the public constructor on the specified <paramref name="type" /> that has a
        /// parameter list matching that of <paramref name="parameterTypes" />.
        /// </summary>
        /// <param name="type">The type on which to find a constructor.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>
        /// The public constructor on the specified <paramref name="type" /> that has a
        /// parameter list matching that of <paramref name="parameterTypes" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">type
        /// or
        /// parameterTypes</exception>
        /// <exception cref="System.MissingMethodException"></exception>
        public static ConstructorInfo GetConstructor(this Type type, params Type[] parameterTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (parameterTypes == null || parameterTypes.Length == 0)
                throw new ArgumentNullException(nameof(parameterTypes));

            var constructor = type
                .GetConstructors()
                .FirstOrDefault(x => ConstructorMatchesArguments(x, parameterTypes));

            if (constructor != null)
                return constructor;

            string argumentString = String.Join(", ", parameterTypes.Select(x => x.FullName));
            var message = $"Could not find the constructor {type}({argumentString})";
            throw new MissingMethodException(message);
        }


        public static MethodInfo GetDelegateInvokeMethod(this Type delegateType)
        {
            if (delegateType == null)
                throw new ArgumentNullException(nameof(delegateType));

            return delegateType.GetMethod("Invoke");
        }


        public static TAttribute GetFirstOrDefaultAttribute<TAttribute>(this MemberInfo member, bool inherit)
            where TAttribute : Attribute
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return member.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().FirstOrDefault();
        }


        /// <summary>
        /// Returns a <see cref="String"/> representation of the specified <paramref name="method"/>
        /// that is fully qualified (not including <see cref="Assembly.FullName"/>) and contains all
        /// generic and regular parameters, if they exist.
        /// </summary>
        /// <param name="method">The method to return a <see cref="String"/> representation of.</param>
        /// <returns>
        /// </returns>
        /// A <see cref="String"/> representation of the specified <paramref name="method"/>
        /// that is fully qualified (not including <see cref="Assembly.FullName"/>) and contains all
        /// generic and regular parameters, if they exist.
        /// <exception cref="System.ArgumentNullException">method</exception>
        public static string GetFullNameWithSignature(this MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var methodFormatter = new MethodFormatter(method);
            return methodFormatter.ToString();
        }


        public static IEnumerable<Type> GetFullTypeHierarchy(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.WalkTree(x => x.BaseType);
        }


        /// <summary>
        /// Gets a public or non-public generic instance method named <paramref name="methodName" /> on the
        /// specified <paramref name="type" /> with a number of generic arguments matching <paramref name="genericArgumentCount"/>.
        /// </summary>
        /// <param name="type">The type on which to find the method named <paramref name="methodName" />.</param>
        /// <param name="methodName">The name of the method to find on <paramref name="type" />.</param>
        /// <param name="genericArgumentCount">The generic argument count.</param>
        /// <returns>
        /// A public or non-public generic instance method named <paramref name="methodName" /> on the
        /// specified <paramref name="type" /> with generic arguments matching <paramref name="genericArgumentCount"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">type
        /// or
        /// methodName
        /// or
        /// genericArgumentTypes</exception>
        /// <exception cref="System.MissingMethodException"></exception>
        public static MethodInfo GetGenericInstanceMethod(this Type type,
                                                          string methodName,
                                                          int genericArgumentCount)
        {
            var genericArgumentTypes = Enumerable.Repeat(typeof(object), genericArgumentCount).ToArray();
            return GenericInstanceMethodInternal(type, methodName, genericArgumentTypes, false);
        }


        /// <summary>
        /// Gets a public or non-public generic instance method named <paramref name="methodName" /> on the
        /// specified <paramref name="type" /> with generic arguments matching those
        /// specified in <paramref name="genericArgumentTypes" />.
        /// </summary>
        /// <param name="type">The type on which to find the method named <paramref name="methodName" />.</param>
        /// <param name="methodName">The name of the method to find on <paramref name="type" />.</param>
        /// <param name="genericArgumentTypes">The generic argument types as they occur on the method.</param>
        /// <returns>
        /// A public or non-public generic instance method named <paramref name="methodName" /> on the
        /// specified <paramref name="type" /> with generic arguments matching those
        /// specified in <paramref name="genericArgumentTypes" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// type
        /// or
        /// methodName
        /// or
        /// genericArgumentTypes
        /// </exception>
        /// <exception cref="System.MissingMethodException"></exception>
        public static MethodInfo GetGenericInstanceMethod(this Type type,
                                                          string methodName,
                                                          params Type[] genericArgumentTypes)
        {
            return GenericInstanceMethodInternal(type, methodName, genericArgumentTypes, true);
        }


        public static IEnumerable<Type> GetInterfacesOfGeneric(this Type type, Type genericTypeDefinition)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var metadataToken = genericTypeDefinition.UniqueToken();

            return type
                .WrapAsEnumerable()
                .Concat(type.GetInterfaces())
                .Where(t => t.UniqueToken() == metadataToken);
        }


        public static TypeSpec GetItemType(this TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));

            var enumerableTypeSpec = typeSpec as EnumerableTypeSpec;
            if (enumerableTypeSpec != null)
                return enumerableTypeSpec.ItemType;

            return typeSpec;
        }


        /// <summary>
        /// Gets all loadable <see cref="Type"/>s from the specified <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// All loadable <see cref="Type"/>s from the specified <paramref name="assembly"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">assembly</exception>
        /// <remarks>
        /// Shamelessly stolen form <a href="http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/">Phil Haack</a>
        /// </remarks>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }


        public static Type GetPropertyOrFieldType(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

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
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

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
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            return sourceType
                .WrapAsEnumerable()
                .Append(sourceType.GetInterfaces())
                .Select(x => x.GetProperty(propertyName)).FirstOrDefault(x => x != null);
        }


        public static bool HasAttribute<TAttribute>(this MemberInfo member, bool inherit)
            where TAttribute : Attribute
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

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
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (targetAndArgs == null)
                throw new ArgumentNullException(nameof(targetAndArgs));

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
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return IsAnonymousTypeName(type.Name);
        }


        public static bool IsAnonymous(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return IsAnonymousTypeName(type.Name);
        }


        public static bool IsAnonymousTypeName(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return typeName.StartsWith("<>f__AnonymousType") || typeName.StartsWith("<>__AnonType");
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
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

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


        public static bool IsGenericInstanceOf(this Type type, params Type[] genericTypeDefinitions)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (genericTypeDefinitions == null)
                throw new ArgumentNullException(nameof(genericTypeDefinitions));

            return genericTypeDefinitions.Any(x => x.UniqueToken() == type.UniqueToken());
        }


        public static bool IsGenericInstanceOf(this MethodInfo type, params MethodInfo[] genericTypeDefinitions)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (genericTypeDefinitions == null)
                throw new ArgumentNullException(nameof(genericTypeDefinitions));

            return genericTypeDefinitions.Any(x => x.UniqueToken() == type.UniqueToken());
        }


        public static bool IsNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // TODO: Shouldn't this also check type.IsValueType? @asbjornu
            return Nullable.GetUnderlyingType(type) != null;
        }


        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            return propertyInfo.GetAccessors(true).First(x => x != null).IsStatic;
        }


        public static bool IsTuple(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.Namespace == "System" && type.Name.StartsWith("Tuple`");
        }


        /// <summary>
        /// Gets version of member where reflected type is same as declaring type.
        /// </summary>
        /// <returns>Normalized member.</returns>
        public static TMemberInfo NormalizeReflectedType<TMemberInfo>(this TMemberInfo memberInfo)
            where TMemberInfo : MemberInfo
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (memberInfo.DeclaringType == null || memberInfo.DeclaringType == memberInfo.ReflectedType)
                return memberInfo;

            return
                (TMemberInfo)memberInfo.DeclaringType.GetMember(memberInfo.Name,
                                                                BindingFlags.Instance | BindingFlags.NonPublic |
                                                                BindingFlags.Public)
                                       .FirstOrDefault(x => x.MetadataToken == memberInfo.MetadataToken);
        }


        /// <summary>
        /// Returns <c>true</c> if the parameters of the specified <paramref name="method" />
        /// matches the number of type arguments and their type; otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TParam1">The type of the first parameter of the <paramref name="method" />.</typeparam>
        /// <typeparam name="TParam2">The type of the second parameter of the <paramref name="method" />.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns>
        /// <c>true</c> if the parameters of the specified <paramref name="method" />
        /// matches the number of type arguments and their type; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">method</exception>
        public static bool ParameterTypesMatch<TParam1, TParam2>(this MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var parameters = method.GetParameters();
            if (parameters.Length != 2)
                return false;

            return parameters[0].ParameterType == typeof(TParam1)
                   && parameters[1].ParameterType == typeof(TParam2);
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
                throw new ArgumentNullException(nameof(typeInstance));

            if (genTypeDef == null)
                throw new ArgumentNullException(nameof(genTypeDef));

            if (!genTypeDef.IsGenericTypeDefinition)
                throw new ArgumentException("gentTypeDef is required to be a generic type definition.", nameof(genTypeDef));

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
        public static bool TryFillGenericTypeParameters(Type wantedType,
                                                        Type actualType,
                                                        Type[] methodTypeArgs,
                                                        out bool typeArgsWasResolved)
        {
            if (wantedType == null)
                throw new ArgumentNullException(nameof(wantedType));

            if (actualType == null)
                throw new ArgumentNullException(nameof(actualType));

            if (methodTypeArgs == null)
                throw new ArgumentNullException(nameof(methodTypeArgs));

            typeArgsWasResolved = false;

            if (wantedType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "Does not expect genDefArgType to be a generic type definition.",
                    nameof(wantedType));
            }

            if (actualType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "Does not expect instanceArgType to be a generic type definition.",
                    nameof(actualType));
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
                    var isValueType = wantedType.IsValueType
                                      || (wantedType.GenericParameterAttributes
                                          & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
                    if (!isValueType && actualType.GetConstructor(Type.EmptyTypes) == null)
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
                    if (!TryFillGenericTypeParameters(wantedTypeArg,
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
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type[] typeArgs;
            if (TryExtractTypeArguments(type, typeof(IEnumerable<>), out typeArgs))
            {
                elementType = typeArgs[0];
                return true;
            }
            elementType = null;
            return false;
        }


        public static bool TryGetPropertyByName(this TypeSpec type,
                                                string name,
                                                StringComparison stringComparison,
                                                out PropertySpec property)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            property = type.AllProperties.FirstOrDefault(x => String.Equals(x.Name, name, stringComparison));
            return property != null;
        }


        public static bool TryGetPropertyByUriName(this ResourceType type, string name, out PropertySpec property)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

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
                throw new ArgumentNullException(nameof(methodDefinition));

            if (argumentTypes == null)
                throw new ArgumentNullException(nameof(argumentTypes));

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
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return UniqueMemberToken.FromMemberInfo(member);
        }


        public static bool IsCollection(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type[] _;
            return type.TryExtractTypeArguments(typeof(ICollection<>), out _);
        }


        private static bool ConstructorMatchesArguments(ConstructorInfo constructor, Type[] parameterTypes)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var parameters = constructor.GetParameters();
            if (parameters.Length != parameterTypes.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                var expectedParameterType = parameterTypes[i];
                var actualParameter = parameters[i];

                if (actualParameter.ParameterType != expectedParameterType)
                    return false;
            }

            return true;
        }


        private static MethodInfo GenericInstanceMethodInternal(Type type,
                                                                string methodName,
                                                                Type[] genericArgumentTypes,
                                                                bool validateArgumentTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (String.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));

            if (genericArgumentTypes == null || genericArgumentTypes.Length == 0)
                throw new ArgumentNullException(nameof(genericArgumentTypes));

            var method = type
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(x => x.Name == methodName);

            if (method != null
                && method.IsGenericMethod
                && method.GetGenericArguments().Length == genericArgumentTypes.Length)
            {
                // TODO: If validateArgumentTypes is true, verify each argument's generic type constraint to see if it is compatible with the specified generic argument type. @asbjornu
                return method;
            }

            string argumentString = String.Join(", ", genericArgumentTypes.Select(x => x.FullName));
            var message = $"Could not find the method {type}.{methodName}<{argumentString}>().";

            throw new MissingMethodException(message);
        }


        private static Type SubstituteTypeParameters(Type type, Type[] methodTypeArgs)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (methodTypeArgs == null)
                throw new ArgumentNullException(nameof(methodTypeArgs));

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