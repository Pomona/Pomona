#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

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


        public WrappedPropertyProxyBuilder(ModuleDefinition module,
                                           TypeReference proxySuperBaseTypeDef,
                                           TypeDefinition propertyWrapperType,
                                           bool isPublic = true,
                                           GeneratePropertyMethods onGeneratePropertyMethods = null,
                                           string proxyNamespace = null)
            : base(module, "{0}LazyProxy", proxySuperBaseTypeDef, isPublic, onGeneratePropertyMethods, proxyNamespace)
        {
            this.propertyWrapperType = propertyWrapperType;
        }


        protected override void OnGeneratePropertyMethods(PropertyDefinition targetProp,
                                                          PropertyDefinition proxyProp,
                                                          TypeReference proxyBaseType,
                                                          TypeReference proxyTargetType,
                                                          TypeReference rootProxyTargetType)
        {
            var propWrapperTypeDef = this.propertyWrapperType;
            var propWrapperTypeRef = Module
                .Import(propWrapperTypeDef)
                .MakeGenericInstanceType(proxyTargetType, proxyProp.PropertyType);

            var constructor = propWrapperTypeDef
                .GetConstructors()
                .FirstOrDefault(x => !x.IsStatic
                                     && x.Parameters.Count == 1
                                     && x.Parameters[0].ParameterType.FullName == Module.TypeSystem.String.FullName);

            if (constructor == null)
            {
                var message = $"Could not find the constructor {propWrapperTypeDef}({Module.TypeSystem.String.FullName}).";
                throw new InvalidOperationException(message);
            }

            var propWrapperCtor = Module
                .Import(constructor)
                .MakeHostInstanceGeneric(proxyTargetType, proxyProp.PropertyType);

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
            var onGetMethod = baseDef.Methods.FirstOrDefault(x => x.Name == "OnGet");
            if (onGetMethod == null || onGetMethod.GenericParameters.Count != 2)
            {
                var message = $"Could not find the method {baseDef}.OnGet<{proxyTargetType}, {proxyProp.PropertyType}>().";
                throw new InvalidOperationException(message);
            }

            var proxyOnGetMethod = Module.Import(onGetMethod);
            var proxyOnGetMethodInstance = new GenericInstanceMethod(proxyOnGetMethod);
            proxyOnGetMethodInstance.GenericArguments.Add(proxyTargetType);
            proxyOnGetMethodInstance.GenericArguments.Add(proxyProp.PropertyType);

            var onSetMethod = baseDef.Methods.FirstOrDefault(x => x.Name == "OnSet");
            if (onSetMethod == null || onSetMethod.GenericParameters.Count != 2)
            {
                var message = $"Could not find the method {baseDef}.OnSet<{proxyTargetType}, {proxyProp.PropertyType}>().";
                throw new InvalidOperationException(message);
            }

            var proxyOnSetMethod = Module.Import(onSetMethod);
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