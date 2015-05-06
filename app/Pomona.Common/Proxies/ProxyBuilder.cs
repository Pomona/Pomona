#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

#if !DISABLE_PROXY_GENERATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Pomona.Common.Internals;

using TypeDefinition = System.Reflection.Emit.TypeBuilder;
using TypeReference = System.Type;
using ModuleDefinition = System.Reflection.Emit.ModuleBuilder;
using PropertyDefinition = System.Reflection.Emit.PropertyBuilder;
using MethodReference = System.Reflection.MethodInfo;
using MethodDefinition = System.Reflection.Emit.MethodBuilder;

namespace Pomona.Common.Proxies
{
    public class ProxyBuilder
    {
        private readonly bool isPublic;
        private readonly ModuleDefinition module;

        private readonly Action<PropertyInfo, PropertyDefinition, TypeReference, TypeReference>
            onGeneratePropertyMethodsFunc;

        private readonly TypeReference proxyBaseTypeDef;
        private readonly string proxyNamespace;
        private string proxyNameFormat;


        public ProxyBuilder(
            ModuleDefinition module,
            string proxyNameFormat,
            TypeReference proxyBaseTypeDef,
            bool isPublic,
            Action<PropertyInfo, PropertyDefinition, TypeReference, TypeReference> onGeneratePropertyMethodsFunc =
                null,
            string proxyNamespace = null)
        {
            this.proxyNamespace = proxyNamespace ?? module.Assembly.GetName().Name;
            this.module = module;
            this.proxyBaseTypeDef = proxyBaseTypeDef;
            this.onGeneratePropertyMethodsFunc = onGeneratePropertyMethodsFunc;
            this.proxyNameFormat = proxyNameFormat;
            this.isPublic = isPublic;
        }


        public virtual ModuleDefinition Module
        {
            get { return this.module; }
        }

        public virtual string ProxyNameFormat
        {
            get { return this.proxyNameFormat; }
            protected set { this.proxyNameFormat = value; }
        }


        public TypeDefinition CreateProxyType(
            string nameBase,
            IEnumerable<TypeReference> interfacesToImplement)
        {
            var proxyBaseCtor =
                this.proxyBaseTypeDef.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic
                                                      | BindingFlags.Public).First(x => x.GetParameters().Length == 0);

            var proxyTypeName = string.Format(this.proxyNameFormat, nameBase);

            var typeAttributes = this.isPublic
                ? TypeAttributes.Public
                : TypeAttributes.NotPublic;

            var proxyType =
                Module.DefineType(this.proxyNamespace + "." + proxyTypeName, typeAttributes, this.proxyBaseTypeDef);

            foreach (var interfaceDef in interfacesToImplement)
                proxyType.AddInterfaceImplementation(interfaceDef);

            // Empty public constructor
            var ctor =
                proxyType.DefineConstructor(MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                                            MethodAttributes.RTSpecialName
                                            | MethodAttributes.Public,
                                            CallingConventions.Standard,
                                            Type.EmptyTypes);

            var ctorIlProcessor = ctor.GetILGenerator();
            ctorIlProcessor.Emit(OpCodes.Ldarg_0);
            ctorIlProcessor.Emit(OpCodes.Call, proxyBaseCtor);
            ctorIlProcessor.Emit(OpCodes.Ret);

            var interfaces = interfacesToImplement.SelectMany(GetAllInterfacesRecursive).Distinct().ToList();

            foreach (var targetProp in interfaces.SelectMany(x => x.GetProperties()))
            {
                var proxyPropDef = AddProperty(proxyType,
                                               targetProp.Name,
                                               targetProp.PropertyType,
                                               targetProp.DeclaringType.FullName + ".",
                                               targetProp);
                OnGeneratePropertyMethods(
                    targetProp,
                    proxyPropDef,
                    this.proxyBaseTypeDef,
                    targetProp.DeclaringType,
                    interfacesToImplement.First());
            }

            GenerateProxyMethods(interfaces, proxyType);

            OnPropertyGenerationComplete(proxyType);

            return proxyType;
        }


        protected virtual void OnGeneratePropertyMethods(
            PropertyInfo targetProp,
            PropertyDefinition proxyProp,
            TypeReference proxyBaseType,
            TypeReference proxyTargetType,
            TypeReference rootProxyTargetType)
        {
            if (this.onGeneratePropertyMethodsFunc == null)
            {
                throw new InvalidOperationException(
                    "Either onGenerateMethodsFunc must be provided in argument, or OnGenerateMethods must be overrided");
            }

            this.onGeneratePropertyMethodsFunc(targetProp, proxyProp, proxyBaseType, proxyTargetType);
        }


        protected virtual void OnPropertyGenerationComplete(TypeDefinition proxyType)
        {
        }


        /// <summary>
        /// Create property with explicit implementation of getter and setter, with no method defined.
        /// </summary>
        private PropertyDefinition AddProperty(TypeDefinition declaringType,
                                               string name,
                                               TypeReference propertyType,
                                               string explicitPrefix,
                                               PropertyInfo overridedProp)
        {
            var proxyPropDef = declaringType.DefineProperty(explicitPrefix + name,
                                                            PropertyAttributes.None | PropertyAttributes.SpecialName,
                                                            propertyType,
                                                            null);
            const MethodAttributes methodAttributes = MethodAttributes.Private
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.SpecialName;

            var overridedGetMethod = overridedProp.GetGetMethod();
            if (overridedGetMethod != null)
            {
                var proxyPropGetter = declaringType.DefineMethod(
                    string.Format("{0}get_{1}", explicitPrefix, name),
                    methodAttributes,
                    propertyType,
                    Type.EmptyTypes);
                declaringType.DefineMethodOverride(proxyPropGetter, overridedGetMethod);

                proxyPropDef.SetGetMethod(proxyPropGetter);
            }

            var overridedSetMethod = overridedProp.GetSetMethod();

            if (overridedSetMethod != null)
            {
                var proxyPropSetter = declaringType.DefineMethod(
                    string.Format("{0}set_{1}", explicitPrefix, name),
                    methodAttributes,
                    null,
                    new[] { propertyType });

                proxyPropSetter.DefineParameter(0, ParameterAttributes.None, "value");
                declaringType.DefineMethodOverride(proxyPropSetter, overridedSetMethod);
                proxyPropDef.SetSetMethod(proxyPropSetter);
            }

            return proxyPropDef;
        }


        private static void AdjustParameterTypes(ParameterInfo[] parameters,
                                                 Func<Type, Type> typeReplacer,
                                                 MethodDefinition method)
        {
            var fixedParams = new Type[parameters.Length];
            foreach (var parameter in parameters)
            {
                var paramType = typeReplacer(parameter.ParameterType);
                fixedParams[parameter.Position] = paramType;
                method.DefineParameter(parameter.Position, parameter.Attributes, parameter.Name);
            }
            // Set fixed parameters having correct generic parameters.
            method.SetParameters(fixedParams);

            // Set fixed return type with correct generic parameter.
            method.SetReturnType(typeReplacer(method.ReturnType));
        }


        private static bool BaseTypeHasMatchingPublicMethod(Type baseDef, MethodInfo targetMethod)
        {
            return baseDef.GetMethods()
                          .Any(
                              x =>
                                  x.Name == targetMethod.Name &&
                                  x.ReturnType == targetMethod.ReturnType &&
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(targetMethod.GetParameters().Select(y => y.ParameterType)));
        }


        private static void CopyGenericMethodParameters(MethodInfo targetMethod,
                                                        MethodDefinition method,
                                                        Dictionary<Type, Type> genArgMapping,
                                                        Func<Type, Type> typeReplacer)
        {
            if (targetMethod.IsGenericMethodDefinition)
            {
                var targetGenArgs = targetMethod.GetGenericArguments();

                var items =
                    method.DefineGenericParameters(targetGenArgs.Select(x => x.Name).ToArray()).Zip(targetGenArgs,
                                                                                                    (paramBuilder,
                                                                                                     target) =>
                                                                                                        new
                                                                                                        {
                                                                                                            paramBuilder,
                                                                                                            target
                                                                                                        })
                          .ToList();
                foreach (var arg in items)
                {
                    arg.paramBuilder.SetGenericParameterAttributes(arg.target.GenericParameterAttributes);
                    genArgMapping[arg.target] = arg.paramBuilder;
                }
                foreach (var arg in items)
                {
                    IEnumerable<Type> interfaceConstraints =
                        arg.target.GetGenericParameterConstraints().Select(typeReplacer).ToList();
                    if (arg.target.BaseType != null)
                    {
                        var baseTypeFixed = typeReplacer(arg.target.BaseType);
                        if (!baseTypeFixed.IsInterface)
                            arg.paramBuilder.SetBaseTypeConstraint(baseTypeFixed);
                        else
                        {
                            //arg.paramBuilder.SetBaseTypeConstraint(typeof(object));
                            interfaceConstraints = interfaceConstraints.Append(baseTypeFixed);
                        }
                        //if (arg.target.BaseType.IsGenericParameter)
                        //{
                        //    interfaceConstraints = interfaceConstraints.Except(arg.target.BaseType.GetInterfaces());
                        //}
                    }
                    arg.paramBuilder.SetInterfaceConstraints(interfaceConstraints.ToArray());
                }
            }
        }


        private static void GenerateInvokeMethodIl(MethodDefinition method,
                                                   Type[] paramTypes,
                                                   ParameterInfo[] parameters,
                                                   MethodInfo proxyOnGetMethod)
        {
            var il = method.GetILGenerator();
            var argsLocal = il.DeclareLocal(typeof(object[]));

            il.Emit(OpCodes.Ldc_I4, paramTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, argsLocal);

            foreach (var param in parameters)
            {
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldc_I4, param.Position);
                il.Emit(OpCodes.Ldarg, param.Position + 1); // +1 since Ldarg0 means this
                if (param.ParameterType.IsValueType)
                    il.Emit(OpCodes.Box, param.ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod"));
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));
            //il.Emit(OpCodes.Ldstr, targetMethod.Name);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, proxyOnGetMethod);

            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Pop);
            else if (method.ReturnType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, method.ReturnType);
            else if (method.ReturnType != typeof(object))
                il.Emit(OpCodes.Castclass, method.ReturnType);

            il.Emit(OpCodes.Ret);
        }


        private void GenerateProxyMethods(IEnumerable<Type> interfaces, TypeDefinition proxyType)
        {
            var methodsExcludingGetters = interfaces
                .SelectMany(x => x.GetMethods().Except(x.GetProperties().SelectMany(GetPropertyMethods)));

            foreach (var targetMethod in methodsExcludingGetters)
            {
                var baseDef = this.proxyBaseTypeDef;
                if (BaseTypeHasMatchingPublicMethod(baseDef, targetMethod))
                    continue;

                var genArgMapping = new Dictionary<Type, Type>();

                if (targetMethod.DeclaringType == null)
                    throw new InvalidOperationException(String.Format("{0} has no declaring type.", targetMethod));

                if (targetMethod.DeclaringType.IsGenericType)
                {
                    var declTypeGenArgs = targetMethod.DeclaringType
                                                      .GetGenericArguments()
                                                      .Zip(targetMethod.DeclaringType.GetGenericTypeDefinition().GetGenericArguments(),
                                                           (a, p) => new { a, p });

                    foreach (var declTypeGenArg in declTypeGenArgs)
                        genArgMapping.Add(declTypeGenArg.p, declTypeGenArg.a);
                }

                Func<Type, Type> typeReplacer =
                    t => TypeUtils.ReplaceInGenericArguments(t, x => genArgMapping.SafeGet(x, x));
                var parameters = targetMethod.GetParameters();
                var paramTypes = parameters.Select(x => x.ParameterType).ToArray();
                var method = proxyType.DefineMethod(targetMethod.Name,
                                                    MethodAttributes.NewSlot
                                                    | MethodAttributes.HideBySig
                                                    | MethodAttributes.Virtual
                                                    | MethodAttributes.Public,
                                                    targetMethod.ReturnType,
                                                    paramTypes);

                CopyGenericMethodParameters(targetMethod, method, genArgMapping, typeReplacer);

                AdjustParameterTypes(parameters, typeReplacer, method);

                var proxyOnGetMethod = baseDef
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(x => x.Name == "OnInvokeMethod");

                if (proxyOnGetMethod == null)
                {
                    var message =
                        String.Format("Unable to generate proxy for {0} because {1}.OnInvokeMethod() is missing.",
                                      targetMethod.GetFullNameWithSignature(),
                                      baseDef.FullName);
                    throw new MissingMethodException(message);
                }

                GenerateInvokeMethodIl(method, paramTypes, parameters, proxyOnGetMethod);
            }
        }


        private static IEnumerable<TypeReference> GetAllInterfacesRecursive(TypeReference typeDefinition)
        {
            return typeDefinition
                .WrapAsEnumerable()
                .Concat(typeDefinition.GetInterfaces().SelectMany(GetAllInterfacesRecursive));
        }


        private static IEnumerable<MethodInfo> GetPropertyMethods(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod();
            var setMethod = propertyInfo.GetSetMethod();

            if (getMethod != null)
                yield return getMethod;
            if (setMethod != null)
                yield return setMethod;
        }
    }
}

#endif