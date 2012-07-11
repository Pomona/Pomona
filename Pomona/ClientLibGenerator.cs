// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Pomona.Client;

namespace Pomona
{
    public class ClientLibGenerator
    {
        private TypeMapper typeMapper;
        private ModuleDefinition module;
        private Dictionary<IMappedType, TypeCodeGenInfo> toClientTypeDict;


        public ClientLibGenerator(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }

        private TypeReference VoidTypeRef
        {
            get { return module.Import(typeof (void)); }
        }

        public void CreateClientDll(Stream stream)
        {
            var types = typeMapper.TransformedTypes.ToList();

            var assembly = AssemblyDefinition.ReadAssembly(typeof (ResourceBase).Assembly.Location);
            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            module = assembly.MainModule;

            toClientTypeDict = new Dictionary<IMappedType, TypeCodeGenInfo>();

            var resourceBaseRef = module.GetType("Pomona.Client.ResourceBase");
            var resourceBaseCtor = resourceBaseRef.GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 0);
            var msObjectTypeRef = module.Import(typeof (object));
            var msObjectCtor =
                module.Import(msObjectTypeRef.Resolve().Methods.First(
                    x => x.Name == ".ctor" && x.IsConstructor && x.Parameters.Count == 0));
            
            var voidTypeRef = VoidTypeRef;

            foreach (var t in types)
            {
                var typeInfo = new TypeCodeGenInfo();
                toClientTypeDict[t] = typeInfo;

                typeInfo.TargetType = (TransformedType) t;

                var interfaceDef = new TypeDefinition("CritterClient", "I" + t.Name,
                                                      TypeAttributes.Interface | TypeAttributes.Public |
                                                      TypeAttributes.Abstract);

                typeInfo.InterfaceType = interfaceDef;

                //var typeDef = new TypeDefinition(
                //    "CritterClient", "I" + t.Name, TypeAttributes.Interface | TypeAttributes.Public);
                var pocoDef = new TypeDefinition(
                    "CritterClient", t.Name, TypeAttributes.Public);

                typeInfo.PocoType = pocoDef;


                module.Types.Add(interfaceDef);
                module.Types.Add(pocoDef);
            }

            foreach (var kvp in toClientTypeDict)
            {
                var type = (TransformedType) kvp.Key;
                var typeInfo = kvp.Value;
                var pocoDef = typeInfo.PocoType;
                var interfaceDef = typeInfo.InterfaceType;
                var classMapping = type;

                // Implement interfaces

                pocoDef.Interfaces.Add(interfaceDef);

                // Inherit correct base class

                MethodReference baseCtorReference;

                if (type.BaseType != null && toClientTypeDict.ContainsKey(type.BaseType))
                {
                    var baseTypeInfo = toClientTypeDict[type.BaseType];
                    pocoDef.BaseType = baseTypeInfo.PocoType;

                    baseCtorReference = baseTypeInfo.PocoType.GetConstructors().First(x => x.Parameters.Count == 0);

                    interfaceDef.Interfaces.Add(baseTypeInfo.InterfaceType);
                }
                else
                {
                    pocoDef.BaseType = resourceBaseRef;
                    baseCtorReference = resourceBaseCtor;
                }

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    voidTypeRef);

                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorReference));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                pocoDef.Methods.Add(ctor);


                foreach (var prop in classMapping.Properties.Where(x => x.DeclaringType == classMapping))
                {
                    var propTypeRef = GetTypeReference(prop.PropertyType);

                    var pocoPropDef = CreatePropertyDefinition(pocoDef, prop);

                    // For interface getters and setters
                    var interfacePropDef = new PropertyDefinition(prop.Name, PropertyAttributes.None, propTypeRef);
                    var interfaceGetMethod = new MethodDefinition(
                        "get_" + prop.Name,
                        MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public, propTypeRef);
                    var interfaceSetMethod = new MethodDefinition(
                        "set_" + prop.Name,
                        MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public, voidTypeRef);
                    interfaceSetMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None,
                                                                              propTypeRef));

                    interfacePropDef.GetMethod = interfaceGetMethod;
                    interfacePropDef.SetMethod = interfaceSetMethod;

                    interfaceDef.Methods.Add(interfaceGetMethod);
                    interfaceDef.Methods.Add(interfaceSetMethod);
                    interfaceDef.Properties.Add(interfacePropDef);

                    var propField =
                        new FieldDefinition(
                            "_" + prop.Name.Substring(0, 1).ToLower() + prop.Name.Substring(1),
                            FieldAttributes.Private,
                            propTypeRef);

                    pocoDef.Fields.Add(propField);


                    // Create get method

                    pocoPropDef.GetMethod.Body.MaxStackSize = 1;
                    var pocoGetIlProcessor = pocoPropDef.GetMethod.Body.GetILProcessor();
                    pocoGetIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    pocoGetIlProcessor.Append(Instruction.Create(OpCodes.Ldfld, propField));
                    pocoGetIlProcessor.Append(Instruction.Create(OpCodes.Ret));
                    // Create set method body

                    pocoPropDef.SetMethod.Body.MaxStackSize = 8;

                    var setIlProcessor = pocoPropDef.SetMethod.Body.GetILProcessor();
                    setIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    setIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                    setIlProcessor.Append(Instruction.Create(OpCodes.Stfld, propField));
                    setIlProcessor.Append(Instruction.Create(OpCodes.Ret));
                }
            }

            CreateProxyType("Proxy", module.Types.First(x => x.Name == "LazyProxyBase"));

            CreateProxyType("Update", module.Types.First(x => x.Name == "UpdateProxyBase"));

            // Copy types from running assembly

            var memstream = new MemoryStream();
            assembly.Write(memstream);

            var array = memstream.ToArray();

            stream.Write(array, 0, array.Length);

            //assembly.Write(stream);
        }

        private void CreateProxyType(string proxyTypeSuffix, TypeDefinition proxyBaseTypeDef)
        {
            var proxyBaseCtor = proxyBaseTypeDef.GetConstructors().First(x => x.Parameters.Count == 0);
            var proxyOnPropertyGetMethod = proxyBaseTypeDef.Methods.First(x => x.Name == "OnPropertyGet");
            var proxyOnPropertySetMethod = proxyBaseTypeDef.Methods.First(x => x.Name == "OnPropertySet");

            foreach (var typeInfo in toClientTypeDict.Values)
            {
                var targetType = typeInfo.TargetType;
                var name = targetType.Name;
                var proxyType =
                    typeInfo.ProxyType =
                    new TypeDefinition("CritterClient", targetType.Name + proxyTypeSuffix, TypeAttributes.Public,
                                       proxyBaseTypeDef);
                proxyType.Interfaces.Add(typeInfo.InterfaceType);

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    VoidTypeRef);

                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, proxyBaseCtor));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                proxyType.Methods.Add(ctor);

                foreach (var prop in targetType.Properties)
                {
                    var proxyPropDef = CreatePropertyDefinition(proxyType, prop);

                    var getterOpcodes = proxyPropDef.GetMethod.Body.GetILProcessor();
                    getterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_0));
                    getterOpcodes.Append(Instruction.Create(OpCodes.Ldstr, prop.Name));
                    getterOpcodes.Append(Instruction.Create(OpCodes.Call, proxyOnPropertyGetMethod));
                    if (prop.PropertyType.IsValueType)
                    {
                        getterOpcodes.Append(Instruction.Create(OpCodes.Unbox_Any, proxyPropDef.PropertyType));
                    }
                    else
                    {
                        getterOpcodes.Append(Instruction.Create(OpCodes.Castclass, proxyPropDef.PropertyType));
                    }
                    getterOpcodes.Append(Instruction.Create(OpCodes.Ret));

                    var setterOpcodes = proxyPropDef.SetMethod.Body.GetILProcessor();
                    setterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_0));
                    setterOpcodes.Append(Instruction.Create(OpCodes.Ldstr, prop.Name));
                    setterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_1));
                    if (prop.PropertyType.IsValueType)
                    {
                        setterOpcodes.Append(Instruction.Create(OpCodes.Box, proxyPropDef.PropertyType));
                    }
                    setterOpcodes.Append(Instruction.Create(OpCodes.Call, proxyOnPropertySetMethod));
                    setterOpcodes.Append(Instruction.Create(OpCodes.Ret));
                }

                module.Types.Add(proxyType);
            }
        }

        private PropertyDefinition CreatePropertyDefinition(TypeDefinition proxyType, PropertyMapping prop)
        {
            var propTypeRef = GetTypeReference(prop.PropertyType);
            var proxyPropDef = new PropertyDefinition(prop.Name, PropertyAttributes.None, propTypeRef);
            var proxyPropGetter = new MethodDefinition("get_" + prop.Name,
                                                       MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                       MethodAttributes.HideBySig
                                                       | MethodAttributes.Virtual | MethodAttributes.Public,
                                                       propTypeRef);
            proxyPropDef.GetMethod = proxyPropGetter;

            var proxyPropSetter = new MethodDefinition("set_" + prop.Name,
                                                       MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                       MethodAttributes.HideBySig
                                                       | MethodAttributes.Virtual | MethodAttributes.Public,
                                                       VoidTypeRef);
            proxyPropSetter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propTypeRef));
            proxyPropDef.SetMethod = proxyPropSetter;

            proxyType.Methods.Add(proxyPropGetter);
            proxyType.Methods.Add(proxyPropSetter);
            proxyType.Properties.Add(proxyPropDef);
            return proxyPropDef;
        }


        private TypeReference GetTypeReference(IMappedType type)
        {
            // TODO: Cache typeRef

            TypeReference typeRef = null;

            var sharedType = type as SharedType;
            if (sharedType != null)
            {
                typeRef = module.Import(sharedType.TargetType);

                if (sharedType.IsGenericType)
                {
                    if (sharedType.GenericArguments.Count != typeRef.GenericParameters.Count)
                        throw new InvalidOperationException("Generic argument count not matching target type");

                    var typeRefInstance = new GenericInstanceType(typeRef);
                    foreach (var genericArgument in sharedType.GenericArguments)
                        typeRefInstance.GenericArguments.Add(GetTypeReference(genericArgument));

                    typeRef = typeRefInstance;
                }
            }

            var transformedType = type as TransformedType;
            if (transformedType != null)
                typeRef = toClientTypeDict[transformedType].InterfaceType;

            if (typeRef == null)
                throw new InvalidOperationException("Unable to get TypeReference for IMappedType");

            return typeRef;
        }

        #region Nested type: TypeCodeGenInfo

        private class TypeCodeGenInfo
        {
            public TransformedType TargetType { get; set; }
            public TypeDefinition InterfaceType { get; set; }
            public TypeDefinition PocoType { get; set; }
            public TypeDefinition ProxyType { get; set; }
        }

        #endregion
    }
}