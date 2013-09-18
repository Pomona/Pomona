#region License

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

#endregion

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
            get { return module; }
        }

        public virtual string ProxyNameFormat
        {
            get { return proxyNameFormat; }
            protected set { proxyNameFormat = value; }
        }


        public TypeDefinition CreateProxyType(
            string nameBase,
            IEnumerable<TypeReference> interfacesToImplement)
        {
            var proxyBaseCtor =
                proxyBaseTypeDef.GetConstructors().First(x => x.GetParameters().Length == 0);

            var proxyTypeName = string.Format(proxyNameFormat, nameBase);

            var typeAttributes = isPublic
                                     ? TypeAttributes.Public
                                     : TypeAttributes.NotPublic;

            var proxyType =
                Module.DefineType(proxyNamespace + "." + proxyTypeName, typeAttributes, proxyBaseTypeDef);

            foreach (var interfaceDef in interfacesToImplement)
                proxyType.AddInterfaceImplementation(interfaceDef);

            // Empty public constructor
            var ctor =
                proxyType.DefineConstructor(MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                                            MethodAttributes.RTSpecialName
                                            | MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            var ctorIlProcessor = ctor.GetILGenerator();
            ctorIlProcessor.Emit(OpCodes.Ldarg_0);
            ctorIlProcessor.Emit(OpCodes.Call, proxyBaseCtor);
            ctorIlProcessor.Emit(OpCodes.Ret);

            var interfaces = interfacesToImplement.SelectMany(GetAllInterfacesRecursive).Distinct().ToList();

            foreach (var targetProp in interfaces.SelectMany(x => x.GetProperties()))
            {
                var proxyPropDef = AddProperty(proxyType, targetProp.Name, targetProp.PropertyType);
                OnGeneratePropertyMethods(
                    targetProp,
                    proxyPropDef,
                    proxyBaseTypeDef,
                    targetProp.DeclaringType,
                    interfacesToImplement.First());
            }

            GenerateProxyMethods(interfaces, proxyType);

            OnPropertyGenerationComplete(proxyType);

            return proxyType;
        }

        private void GenerateProxyMethods(List<Type> interfaces, TypeDefinition proxyType)
        {
            foreach (
                var targetMethod in
                    interfaces.SelectMany(x => x.GetMethods().Except(x.GetProperties().SelectMany(GetPropertyMethods))))
            {
                var baseDef = proxyBaseTypeDef;
                if (BaseTypeHasMatchingPublicMethod(baseDef, targetMethod))
                {
                    continue;
                }

                var proxyOnGetMethod =
                    baseDef.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                           .FirstOrDefault(x => x.Name == "OnInvokeMethod");

                if (proxyOnGetMethod == null)
                    throw new InvalidOperationException("Unable to generate proxy for " +
                                                        targetMethod.DeclaringType + ":" + targetMethod.Name + " using " +
                                                        baseDef.FullName + " as base: base is missing OnInvokeMethod.");

                var parameters = targetMethod.GetParameters();
                var paramTypes = parameters.Select(x => x.ParameterType).ToArray();
                var method = proxyType.DefineMethod(
                    targetMethod.Name,
                    MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual |
                    MethodAttributes.Public,
                    targetMethod.ReturnType,
                    paramTypes);


                var il = method.GetILGenerator();
                var argsLocal = il.DeclareLocal(typeof (object[]));

                il.Emit(OpCodes.Ldc_I4, paramTypes.Length);
                il.Emit(OpCodes.Newarr, typeof (object));
                il.Emit(OpCodes.Stloc, argsLocal);

                foreach (var param in parameters)
                {
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.Emit(OpCodes.Ldc_I4, param.Position);
                    il.Emit(OpCodes.Ldarg, param.Position + 1); // +1 since Ldarg0 means this
                    if (param.ParameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, param.ParameterType);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof (MethodBase).GetMethod("GetCurrentMethod"));
                il.Emit(OpCodes.Castclass, typeof (MethodInfo));
                //il.Emit(OpCodes.Ldstr, targetMethod.Name);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, proxyOnGetMethod);

                if (method.ReturnType == typeof (void))
                {
                    il.Emit(OpCodes.Pop);
                }
                else if (method.ReturnType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, method.ReturnType);
                }
                else if (method.ReturnType != typeof (object))
                {
                    il.Emit(OpCodes.Castclass, method.ReturnType);
                }

                il.Emit(OpCodes.Ret);
            }
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

        private static IEnumerable<MethodInfo> GetPropertyMethods(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod();
            var setMethod = propertyInfo.GetSetMethod();

            if (getMethod != null)
                yield return getMethod;
            if (setMethod != null)
                yield return setMethod;
        }

        protected virtual void OnPropertyGenerationComplete(TypeDefinition proxyType)
        {
        }

        protected virtual void OnGeneratePropertyMethods(
            PropertyInfo targetProp,
            PropertyDefinition proxyProp,
            TypeReference proxyBaseType,
            TypeReference proxyTargetType,
            TypeReference rootProxyTargetType)
        {
            if (onGeneratePropertyMethodsFunc == null)
            {
                throw new InvalidOperationException(
                    "Either onGenerateMethodsFunc must be provided in argument, or OnGenerateMethods must be overrided");
            }

            onGeneratePropertyMethodsFunc(targetProp, proxyProp, proxyBaseType, proxyTargetType);
        }


        private static IEnumerable<TypeReference> GetAllInterfacesRecursive(TypeReference typeDefinition)
        {
            return
                typeDefinition.WrapAsEnumerable().Concat(
                    typeDefinition.GetInterfaces().SelectMany(x => GetAllInterfacesRecursive(x)));
        }


        /// <summary>
        /// Create property with public getter and setter, with no method defined.
        /// </summary>
        private PropertyDefinition AddProperty(TypeDefinition declaringType, string name, TypeReference propertyType)
        {
            var proxyPropDef = declaringType.DefineProperty(name, PropertyAttributes.None, propertyType, null);
            var proxyPropGetter = declaringType.DefineMethod(
                "get_" + name,
                MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig
                | MethodAttributes.Virtual | MethodAttributes.Public,
                propertyType, Type.EmptyTypes);

            proxyPropDef.SetGetMethod(proxyPropGetter);

            var proxyPropSetter = declaringType.DefineMethod(
                "set_" + name,
                MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig
                | MethodAttributes.Virtual | MethodAttributes.Public,
                null, new[] { propertyType });

            proxyPropSetter.DefineParameter(0, ParameterAttributes.None, "value");

            proxyPropDef.SetSetMethod(proxyPropSetter);

            return proxyPropDef;
        }
    }
}