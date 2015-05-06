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
using System.Reflection;
using System.Reflection.Emit;

using TypeDefinition = System.Reflection.Emit.TypeBuilder;
using TypeReference = System.Type;
using ModuleDefinition = System.Reflection.Emit.ModuleBuilder;
using PropertyDefinition = System.Reflection.Emit.PropertyBuilder;
using MethodReference = System.Reflection.MethodInfo;
using MethodDefinition = System.Reflection.Emit.MethodBuilder;

namespace Pomona.Common.Proxies
{
    public class WrappedPropertyProxyBuilder : ProxyBuilder
    {
        private readonly List<Action<ILGenerator>> initPropertyWrapperIlAction = new List<Action<ILGenerator>>();
        private readonly TypeReference propertyWrapperType;


        public WrappedPropertyProxyBuilder(ModuleDefinition module,
                                           TypeReference proxyBaseTypeDef,
                                           TypeReference propertyWrapperType,
                                           bool isPublic = true,
                                           string typeNameFormat = "Fast{0}Proxy",
                                           string proxyNamespace = null)
            : base(module, typeNameFormat, proxyBaseTypeDef, isPublic, proxyNamespace : proxyNamespace)
        {
            this.propertyWrapperType = propertyWrapperType;
        }


        protected override void OnGeneratePropertyMethods(PropertyInfo targetProp,
                                                          PropertyDefinition proxyProp,
                                                          TypeReference proxyBaseType,
                                                          TypeReference proxyTargetType,
                                                          TypeReference rootProxyTargetType)
        {
            var propWrapperTypeRef = this.propertyWrapperType.MakeGenericType(proxyTargetType, proxyProp.PropertyType);
            var proxyType = (TypeDefinition)proxyProp.DeclaringType;
            if (proxyType == null)
                throw new InvalidOperationException(String.Format("{0} has no declaring type.", proxyProp));

            var propWrapperCtor = propWrapperTypeRef.GetConstructor(typeof(string));
            var propertyWrapperField = proxyType.DefineField("_pwrap_" + targetProp.Name,
                                                             propWrapperTypeRef,
                                                             FieldAttributes.Private | FieldAttributes.Static);

            this.initPropertyWrapperIlAction.Add(il =>
            {
                il.Emit(OpCodes.Ldstr, targetProp.Name);
                il.Emit(OpCodes.Newobj, propWrapperCtor);
                il.Emit(OpCodes.Stsfld, propertyWrapperField);
            });

            /*
            var initPropertyWrappersMethod = GetInitPropertyWrappersMethod(proxyProp);

            var initIl = initPropertyWrappersMethod.Body.GetILProcessor();
            var lastInstruction = initPropertyWrappersMethod.Body.Instructions.Last();
            if (lastInstruction.OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Expected to find ret instruction as last instruction");

            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Ldstr, targetProp.Name));
            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Newobj, propWrapperCtor));
            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Stsfld, propertyWrapperField));
            */

            var baseDef = proxyBaseType;
            var proxyOnGetMethod = baseDef.GetGenericInstanceMethod("OnGet", proxyTargetType, proxyProp.PropertyType);
            var proxyOnGetMethodInstance = proxyOnGetMethod.MakeGenericMethod(proxyTargetType, proxyProp.PropertyType);
            var proxyOnSetMethod = baseDef.GetGenericInstanceMethod("OnSet", proxyTargetType, proxyProp.PropertyType);
            var proxyOnSetMethodInstance = proxyOnSetMethod.MakeGenericMethod(proxyTargetType, proxyProp.PropertyType);

            var getMethod = (MethodDefinition)proxyProp.GetGetMethod(true);
            if (getMethod != null)
            {
                var getIl = getMethod.GetILGenerator();

                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldsfld, propertyWrapperField);
                getIl.Emit(OpCodes.Callvirt, proxyOnGetMethodInstance);
                getIl.Emit(OpCodes.Ret);
            }

            var setMethod = (MethodDefinition)proxyProp.GetSetMethod(true);
            if (setMethod != null)
            {
                var setIl = setMethod.GetILGenerator();
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldsfld, propertyWrapperField);
                setIl.Emit(OpCodes.Ldarg_1);
                setIl.Emit(OpCodes.Callvirt, proxyOnSetMethodInstance);
                setIl.Emit(OpCodes.Ret);
            }
        }


        protected override void OnPropertyGenerationComplete(TypeDefinition proxyType)
        {
            const string initPropertyWrappersMethodName = "InitPropertyWrappers";

            var initPropertyWrappersMethod = proxyType.DefineMethod(
                initPropertyWrappersMethodName,
                MethodAttributes.Static | MethodAttributes.Private,
                null, Type.EmptyTypes);

            var il = initPropertyWrappersMethod.GetILGenerator();
            this.initPropertyWrapperIlAction.ForEach(m => m(il));
            il.Emit(OpCodes.Ret);

            var cctor = proxyType.DefineTypeInitializer();

            var cctorIl = cctor.GetILGenerator();
            cctorIl.Emit(OpCodes.Call, initPropertyWrappersMethod);
            cctorIl.Emit(OpCodes.Ret);
        }
    }
}

#endif