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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Pomona.CodeGen
{
    public class WrappedPropertyProxyBuilder : ProxyBuilder
    {
        private readonly TypeDefinition propertyWrapperType;


        public WrappedPropertyProxyBuilder(
            ModuleDefinition module,
            TypeReference proxySuperBaseTypeDef,
            TypeDefinition propertyWrapperType,
            bool isPublic = true) : base(module, "{0}LazyProxy", proxySuperBaseTypeDef, isPublic)
        {
            this.propertyWrapperType = propertyWrapperType;
        }


        protected override void OnGeneratePropertyMethods(
            PropertyDefinition targetProp,
            PropertyDefinition proxyProp,
            TypeReference proxyBaseType,
            TypeReference proxyTargetType,
            TypeReference rootProxyTargetType)
        {
            var propWrapperTypeDef = propertyWrapperType;
            var propWrapperTypeRef =
                Module.Import(
                    propWrapperTypeDef.MakeGenericInstanceType(proxyTargetType, proxyProp.PropertyType));

            var propWrapperCtor = Module.Import(
                propWrapperTypeDef.GetConstructors().First(
                    x => !x.IsStatic &&
                         x.Parameters.Count == 1 &&
                         x.Parameters[0].ParameterType.FullName == Module.TypeSystem.String.FullName).
                                   MakeHostInstanceGeneric(proxyTargetType, proxyProp.PropertyType));

            var propertyWrapperField = new FieldDefinition(
                "_pwrap_" + targetProp.Name,
                /*FieldAttributes.SpecialName | */FieldAttributes.Private | FieldAttributes.Static,
                propWrapperTypeRef);
            proxyProp.DeclaringType.Fields.Add(propertyWrapperField);

            var initPropertyWrappersMethod = GetInitPropertyWrappersMethod(proxyProp);

            var initIl = initPropertyWrappersMethod.Body.GetILProcessor();
            var lastInstruction = initPropertyWrappersMethod.Body.Instructions.Last();
            if (lastInstruction.OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Expected to find ret instruction as last instruction");

            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Ldstr, targetProp.Name));
            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Newobj, propWrapperCtor));
            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Stsfld, propertyWrapperField));

            var baseDef = proxyBaseType.Resolve();
            var proxyOnGetMethod =
                Module.Import(baseDef.Methods.First(x => x.Name == "OnGet"));
            if (proxyOnGetMethod.GenericParameters.Count != 2)
            {
                throw new InvalidOperationException(
                    "OnGet method of base class is required to have two generic parameters.");
            }
            var proxyOnGetMethodInstance = new GenericInstanceMethod(proxyOnGetMethod);
            proxyOnGetMethodInstance.GenericArguments.Add(proxyTargetType);
            proxyOnGetMethodInstance.GenericArguments.Add(proxyProp.PropertyType);

            var proxyOnSetMethod =
                Module.Import(baseDef.Methods.First(x => x.Name == "OnSet"));
            if (proxyOnSetMethod.GenericParameters.Count != 2)
            {
                throw new InvalidOperationException(
                    "OnSet method of base class is required to have two generic parameters.");
            }
            var proxyOnSetMethodInstance = new GenericInstanceMethod(proxyOnSetMethod);
            proxyOnSetMethodInstance.GenericArguments.Add(proxyTargetType);
            proxyOnSetMethodInstance.GenericArguments.Add(proxyProp.PropertyType);


            var getIl = proxyProp.GetMethod.Body.GetILProcessor();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldsfld, propertyWrapperField);
            getIl.Emit(OpCodes.Callvirt, proxyOnGetMethodInstance);
            getIl.Emit(OpCodes.Ret);

            var setIl = proxyProp.SetMethod.Body.GetILProcessor();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldsfld, propertyWrapperField);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Callvirt, proxyOnSetMethodInstance);
            setIl.Emit(OpCodes.Ret);
        }


        private MethodDefinition GetInitPropertyWrappersMethod(PropertyDefinition def)
        {
            const string initPropertyWrappersMethodName = "InitPropertyWrappers";
            var initPropertyWrappersMethod =
                def.DeclaringType.Methods.FirstOrDefault(x => x.IsStatic && x.Name == initPropertyWrappersMethodName);

            if (initPropertyWrappersMethod == null)
            {
                initPropertyWrappersMethod = new MethodDefinition(
                    initPropertyWrappersMethodName,
                    MethodAttributes.Static | MethodAttributes.Private,
                    Module.TypeSystem.Void);
                def.DeclaringType.Methods.Add(initPropertyWrappersMethod);

                initPropertyWrappersMethod.Body.MaxStackSize = 8;
                var il = initPropertyWrappersMethod.Body.GetILProcessor();
                il.Emit(OpCodes.Ret);

                var cctor = new MethodDefinition(
                    ".cctor",
                    MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.SpecialName
                    | MethodAttributes.RTSpecialName | MethodAttributes.Static,
                    Module.TypeSystem.Void);

                def.DeclaringType.Methods.Add(cctor);

                cctor.Body.MaxStackSize = 8;
                var cctorIl = cctor.Body.GetILProcessor();
                cctorIl.Emit(OpCodes.Call, initPropertyWrappersMethod);
                cctorIl.Emit(OpCodes.Ret);
            }
            return initPropertyWrappersMethod;
        }
    }
}