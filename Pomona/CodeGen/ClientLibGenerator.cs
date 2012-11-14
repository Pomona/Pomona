#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Pomona.Client;

namespace Pomona.CodeGen
{
    public class NoCommitType
    {
        // NOCOMMIT!
        private static List<int> blah = new List<int>();
    }

    public class ClientLibGenerator
    {
        private ModuleDefinition module;
        private Dictionary<IMappedType, TypeCodeGenInfo> toClientTypeDict;
        private TypeMapper typeMapper;


        public ClientLibGenerator(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;

            PomonaClientEmbeddingEnabled = true;
        }


        public bool MakeProxyTypesPublic { get; set; }

        public bool PomonaClientEmbeddingEnabled { get; set; }

        private TypeReference StringTypeRef
        {
            get { return this.module.Import(typeof(string)); }
        }

        private TypeReference VoidTypeRef
        {
            get { return this.module.Import(typeof(void)); }
        }


        public void CreateClientDll(Stream stream)
        {
            var types = this.typeMapper.TransformedTypes.ToList();

            // Use Pomona.Client lib as starting point!
            AssemblyDefinition assembly;

            if (PomonaClientEmbeddingEnabled)
                assembly = AssemblyDefinition.ReadAssembly(typeof(ResourceBase).Assembly.Location);
            else
            {
                assembly =
                    AssemblyDefinition.CreateAssembly(
                        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);
            }

            assembly.Name = new AssemblyNameDefinition("Critter.Client", new Version(1, 0, 0, 0));

            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            this.module = assembly.MainModule;
            this.module.Name = "Critter.Client.dll";

            if (PomonaClientEmbeddingEnabled)
            {
                foreach (var clientHelperType in this.module.Types.Where(x => x.Namespace == "Pomona.Client"))
                    clientHelperType.Namespace = "CritterClient";
            }

            this.toClientTypeDict = new Dictionary<IMappedType, TypeCodeGenInfo>();

            TypeReference resourceBaseRef;
            TypeReference resourceInterfaceRef;
            if (PomonaClientEmbeddingEnabled)
            {
                resourceBaseRef = this.module.GetType("Pomona.Client.ResourceBase");
                resourceInterfaceRef = this.module.GetType("Pomona.Client.IClientResource");
            }
            else
            {
                resourceBaseRef = this.module.Import(typeof(ResourceBase));
                resourceInterfaceRef = this.module.Import(typeof(IClientResource));
            }

            var resourceBaseCtor =
                this.module.Import(
                    resourceBaseRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 0));
            var msObjectTypeRef = this.module.Import(typeof(object));
            var msObjectCtor =
                this.module.Import(
                    msObjectTypeRef.Resolve().Methods.First(
                        x => x.Name == ".ctor" && x.IsConstructor && x.Parameters.Count == 0));

            var voidTypeRef = VoidTypeRef;

            foreach (var t in types)
            {
                var typeInfo = new TypeCodeGenInfo();
                this.toClientTypeDict[t] = typeInfo;

                typeInfo.TransformedType = (TransformedType)t;

                var interfaceDef = new TypeDefinition(
                    "CritterClient",
                    "I" + t.Name,
                    TypeAttributes.Interface | TypeAttributes.Public |
                    TypeAttributes.Abstract);

                typeInfo.InterfaceType = interfaceDef;

                //var typeDef = new TypeDefinition(
                //    "CritterClient", "I" + t.Name, TypeAttributes.Interface | TypeAttributes.Public);
                var pocoDef = new TypeDefinition(
                    "CritterClient", t.Name + "Resource", TypeAttributes.Public);

                typeInfo.PocoType = pocoDef;

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    voidTypeRef);

                typeInfo.EmptyPocoCtor = ctor;
                pocoDef.Methods.Add(ctor);

                this.module.Types.Add(interfaceDef);
                this.module.Types.Add(pocoDef);
            }

            foreach (var kvp in this.toClientTypeDict)
            {
                var type = (TransformedType)kvp.Key;
                var typeInfo = kvp.Value;
                var pocoDef = typeInfo.PocoType;
                var interfaceDef = typeInfo.InterfaceType;
                var classMapping = type;

                // Implement interfaces

                pocoDef.Interfaces.Add(interfaceDef);

                // Inherit correct base class

                MethodReference baseCtorReference;

                if (type.BaseType != null && this.toClientTypeDict.ContainsKey(type.BaseType))
                {
                    var baseTypeInfo = this.toClientTypeDict[type.BaseType];
                    pocoDef.BaseType = baseTypeInfo.PocoType;

                    baseCtorReference = baseTypeInfo.PocoType.GetConstructors().First(x => x.Parameters.Count == 0);

                    interfaceDef.Interfaces.Add(baseTypeInfo.InterfaceType);

                    typeInfo.UriBaseType = this.toClientTypeDict[type.UriBaseType].InterfaceType;
                }
                else
                {
                    interfaceDef.Interfaces.Add(resourceInterfaceRef);
                    pocoDef.BaseType = resourceBaseRef;
                    typeInfo.UriBaseType = typeInfo.InterfaceType;
                    baseCtorReference = resourceBaseCtor;
                }

                var ctor = typeInfo.EmptyPocoCtor;
                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorReference));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                foreach (var prop in classMapping.Properties.Where(x => x.DeclaringType == classMapping))
                {
                    var propTypeRef = GetPropertyTypeReference(prop);

                    // For interface getters and setters
                    var interfacePropDef = new PropertyDefinition(prop.Name, PropertyAttributes.None, propTypeRef);
                    var interfaceGetMethod = new MethodDefinition(
                        "get_" + prop.Name,
                        MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public,
                        propTypeRef);
                    var interfaceSetMethod = new MethodDefinition(
                        "set_" + prop.Name,
                        MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public,
                        voidTypeRef);
                    interfaceSetMethod.Parameters.Add(
                        new ParameterDefinition(
                            "value",
                            ParameterAttributes.None,
                            propTypeRef));

                    interfacePropDef.GetMethod = interfaceGetMethod;
                    interfacePropDef.SetMethod = interfaceSetMethod;

                    interfaceDef.Methods.Add(interfaceGetMethod);
                    interfaceDef.Methods.Add(interfaceSetMethod);
                    interfaceDef.Properties.Add(interfacePropDef);

                    AddAutomaticProperty(pocoDef, prop.Name, propTypeRef);
                }
            }

            // Add attribute with resource info

            // Create proxy types

            CreateProxyType("{0}OldProxy", GetProxyType("LazyProxyBase"), (info, def) => { info.LazyProxyType = def; });

            CreateProxyType(
                "{0}Proxy",
                GetProxyType("LazyProxyBase"),
                (info, def) => { info.LazyProxyType = def; },
                GenerateAcceleratedPropertyProxyMethods);

            CreateProxyType(
                "{0}Update",
                GetProxyType("PutResourceBase"),
                (info, def) => { info.PutFormType = def; },
                GeneratePropertyMethodsForUpdateProxy);

            CreateProxyType(
                "{0}Form",
                GetProxyType("PutResourceBase"),
                (info, def) => { info.PostFormType = def; },
                GeneratePropertyMethodsForNewResource,
                alwaysPublic : true);

            CreateClientType("Client");

            foreach (var typeInfo in this.toClientTypeDict.Values)
                AddResourceInfoAttribute(typeInfo);

            // Copy types from running assembly

            var memstream = new MemoryStream();
            assembly.Write(memstream);

            var array = memstream.ToArray();

            stream.Write(array, 0, array.Length);

            //assembly.Write(stream);
        }


        private PropertyDefinition AddAutomaticProperty(
            TypeDefinition declaringType, string name, TypeReference propertyType)
        {
            var propertyDefinition = AddProperty(declaringType, name, propertyType);

            var propField =
                new FieldDefinition(
                    "_" + name.Substring(0, 1).ToLower() + name.Substring(1),
                    FieldAttributes.Private,
                    propertyType);

            declaringType.Fields.Add(propField);

            propertyDefinition.GetMethod.Body.MaxStackSize = 1;
            var pocoGetIlProcessor = propertyDefinition.GetMethod.Body.GetILProcessor();
            pocoGetIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            pocoGetIlProcessor.Append(Instruction.Create(OpCodes.Ldfld, propField));
            pocoGetIlProcessor.Append(Instruction.Create(OpCodes.Ret));
            // Create set method body

            propertyDefinition.SetMethod.Body.MaxStackSize = 8;

            var setIlProcessor = propertyDefinition.SetMethod.Body.GetILProcessor();
            setIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            setIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            setIlProcessor.Append(Instruction.Create(OpCodes.Stfld, propField));
            setIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            return propertyDefinition;
        }


        /// <summary>
        /// Create property with public getter and setter, with no method defined.
        /// </summary>
        private PropertyDefinition AddProperty(TypeDefinition declaringType, string name, TypeReference propertyType)
        {
            var proxyPropDef = new PropertyDefinition(name, PropertyAttributes.None, propertyType);
            var proxyPropGetter = new MethodDefinition(
                "get_" + name,
                MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig
                | MethodAttributes.Virtual | MethodAttributes.Public,
                propertyType);
            proxyPropDef.GetMethod = proxyPropGetter;

            var proxyPropSetter = new MethodDefinition(
                "set_" + name,
                MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig
                | MethodAttributes.Virtual | MethodAttributes.Public,
                VoidTypeRef);
            proxyPropSetter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propertyType));
            proxyPropDef.SetMethod = proxyPropSetter;

            declaringType.Methods.Add(proxyPropGetter);
            declaringType.Methods.Add(proxyPropSetter);
            declaringType.Properties.Add(proxyPropDef);
            return proxyPropDef;
        }


        private void AddResourceInfoAttribute(TypeCodeGenInfo typeInfo)
        {
            var interfaceDef = typeInfo.InterfaceType;
            var type = typeInfo.TransformedType;
            var attr = this.module.Import(typeof(ResourceInfoAttribute));
            var methodDefinition =
                this.module.Import(attr.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var custAttr =
                new CustomAttribute(methodDefinition);
            var stringTypeReference = this.module.TypeSystem.String;
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "UrlRelativePath", new CustomAttributeArgument(stringTypeReference, type.UriRelativePath)));

            var typeTypeReference = this.module.Import(typeof(Type));
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "PocoType", new CustomAttributeArgument(typeTypeReference, typeInfo.PocoType)));
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "InterfaceType", new CustomAttributeArgument(typeTypeReference, typeInfo.InterfaceType)));
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "LazyProxyType", new CustomAttributeArgument(typeTypeReference, typeInfo.LazyProxyType)));
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "PostFormType", new CustomAttributeArgument(typeTypeReference, typeInfo.PostFormType)));
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "PutFormType", new CustomAttributeArgument(typeTypeReference, typeInfo.PutFormType)));

            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "JsonTypeName", new CustomAttributeArgument(stringTypeReference, type.Name)));

            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "UriBaseType", new CustomAttributeArgument(typeTypeReference, typeInfo.UriBaseType)));

            interfaceDef.CustomAttributes.Add(custAttr);
            //var attrConstructor = attr.Resolve().GetConstructors();
        }


        private void CreateClientType(string clientTypeName)
        {
            var clientBaseTypeRef = GetClientTypeReference(typeof(ClientBase<>));

            var clientTypeDefinition = new TypeDefinition(
                "CritterClient", clientTypeName, TypeAttributes.Public);

            var clientBaseTypeGenericInstance = clientBaseTypeRef.MakeGenericInstanceType(clientTypeDefinition);
            clientTypeDefinition.BaseType = clientBaseTypeGenericInstance;

            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                | MethodAttributes.Public,
                VoidTypeRef);

            ctor.Parameters.Add(new ParameterDefinition("uri", ParameterAttributes.None, StringTypeRef));

            var clientBaseTypeCtor =
                this.module.Import(
                    clientBaseTypeRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 1));
            clientBaseTypeCtor.DeclaringType =
                clientBaseTypeCtor.DeclaringType.MakeGenericInstanceType(clientTypeDefinition);

            ctor.Body.MaxStackSize = 8;
            var ctorIlProcessor = ctor.Body.GetILProcessor();
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, clientBaseTypeCtor));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            clientTypeDefinition.Methods.Add(ctor);

            // Add repository properties

            foreach (var resourceTypeInfo in this.toClientTypeDict.Values.Where(x => x.UriBaseType == x.InterfaceType))
            {
                var repoPropName = resourceTypeInfo.TransformedType.PluralName;
                var repoPropType =
                    GetClientTypeReference(typeof(ClientRepository<,>)).MakeGenericInstanceType(
                        resourceTypeInfo.InterfaceType, resourceTypeInfo.InterfaceType);
                var repoProp = AddAutomaticProperty(clientTypeDefinition, repoPropName, repoPropType);
                repoProp.SetMethod.IsPublic = false;
            }

            this.module.Types.Add(clientTypeDefinition);
        }


        private PropertyDefinition CreateProperty(TypeDefinition proxyType, PropertyMapping prop)
        {
            var propTypeRef = GetPropertyTypeReference(prop);
            var name = prop.Name;

            var proxyPropDef = AddProperty(proxyType, name, propTypeRef);
            return proxyPropDef;
        }


        private void CreateProxyType(
            string proxyTypeFormat,
            TypeReference proxyBaseTypeDef,
            Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated,
            Action<PropertyMapping, PropertyDefinition, TypeReference, TypeReference> propertyMethodGenerator
                = null,
            bool alwaysPublic = false)
        {
            if (propertyMethodGenerator == null)
                propertyMethodGenerator = GeneratePropertyProxyMethods;

            MethodReference proxyBaseCtor =
                proxyBaseTypeDef.Resolve().GetConstructors().First(x => x.Parameters.Count == 0);
            proxyBaseCtor = this.module.Import(proxyBaseCtor);

            foreach (var typeInfo in this.toClientTypeDict.Values)
            {
                var targetType = typeInfo.TransformedType;
                var name = targetType.Name;
                var proxyTypeName = string.Format(proxyTypeFormat, name);

                var typeAttributes = (MakeProxyTypesPublic || alwaysPublic)
                                         ? TypeAttributes.Public
                                         : TypeAttributes.NotPublic;

                var proxyType =
                    new TypeDefinition(
                        "CritterClient",
                        proxyTypeName,
                        typeAttributes,
                        proxyBaseTypeDef);
                this.module.Types.Add(proxyType);
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
                    var proxyPropDef = CreateProperty(proxyType, prop);
                    var declaringTypeInfo = this.toClientTypeDict[prop.DeclaringType];
                    propertyMethodGenerator(prop, proxyPropDef, proxyBaseTypeDef, declaringTypeInfo.InterfaceType);
                }

                //this.module.Types.Add(proxyType);

                if (onTypeGenerated != null)
                    onTypeGenerated(typeInfo, proxyType);
            }
        }


        private void GenerateAcceleratedPropertyProxyMethods(
            PropertyMapping prop, PropertyDefinition def, TypeReference baseRef, TypeReference proxyTargetType)
        {
            var propWrapperTypeDef = GetClientTypeReference(typeof(PropertyWrapper<,>)).Resolve();
            var propWrapperTypeRef =
                this.module.Import(
                    propWrapperTypeDef.MakeGenericInstanceType(proxyTargetType, def.PropertyType));

            var propWrapperCtor = this.module.Import(
                propWrapperTypeDef.GetConstructors().First(
                    x => !x.IsStatic &&
                         x.Parameters.Count == 1 &&
                         x.Parameters[0].ParameterType.FullName == this.module.TypeSystem.String.FullName).
                    MakeHostInstanceGeneric(proxyTargetType, def.PropertyType));

            var propertyWrapperField = new FieldDefinition(
                "_pwrap_" + prop.Name,
                /*FieldAttributes.SpecialName | */FieldAttributes.Private | FieldAttributes.Static,
                propWrapperTypeRef);
            def.DeclaringType.Fields.Add(propertyWrapperField);

            var initPropertyWrappersMethod = GetInitPropertyWrappersMethod(def);

            var initIl = initPropertyWrappersMethod.Body.GetILProcessor();
            var lastInstruction = initPropertyWrappersMethod.Body.Instructions.Last();
            if (lastInstruction.OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Expected to find ret instruction as last instruction");

            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Ldstr, prop.Name));
            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Newobj, propWrapperCtor));
            initIl.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Stsfld, propertyWrapperField));

            var baseDef = baseRef.Resolve();
            var proxyOnGetMethod =
                this.module.Import(baseDef.Methods.First(x => x.Name == "OnGet"));
            var proxyOnSetMethod =
                this.module.Import(baseDef.Methods.First(x => x.Name == "OnSet"));

            if (proxyOnGetMethod.GenericParameters.Count != 2)
                throw new InvalidOperationException(
                    "OnGet method of base class is required to have two generic parameters.");
            if (proxyOnSetMethod.GenericParameters.Count != 2)
                throw new InvalidOperationException(
                    "OnSet method of base class is required to have two generic parameters.");

            var proxyOnGetMethodInstance = new GenericInstanceMethod(proxyOnGetMethod);
            proxyOnGetMethodInstance.GenericArguments.Add(proxyTargetType);
            proxyOnGetMethodInstance.GenericArguments.Add(def.PropertyType);

            var proxyOnSetMethodInstance = new GenericInstanceMethod(proxyOnSetMethod);
            proxyOnSetMethodInstance.GenericArguments.Add(proxyTargetType);
            proxyOnSetMethodInstance.GenericArguments.Add(def.PropertyType);

            var getIl = def.GetMethod.Body.GetILProcessor();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldsfld, propertyWrapperField);
            getIl.Emit(OpCodes.Callvirt, proxyOnGetMethodInstance);
            getIl.Emit(OpCodes.Ret);

            var setIl = def.SetMethod.Body.GetILProcessor();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldsfld, propertyWrapperField);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Callvirt, proxyOnSetMethodInstance);
            setIl.Emit(OpCodes.Ret);
        }


        private void GeneratePropertyMethodsForNewResource(
            PropertyMapping prop,
            PropertyDefinition proxyPropDef,
            TypeReference proxyBaseDefinition,
            TypeReference proxyTargetType)
        {
            if (prop.CreateMode == PropertyMapping.PropertyCreateMode.Required ||
                prop.CreateMode == PropertyMapping.PropertyCreateMode.Optional)
                GeneratePropertyProxyMethods(prop, proxyPropDef, proxyBaseDefinition, proxyTargetType);
            else
            {
                var invalidOperationStrCtor = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                var invalidOperationStrCtorRef = this.module.Import(invalidOperationStrCtor);

                foreach (var method in new[] { proxyPropDef.GetMethod, proxyPropDef.SetMethod })
                {
                    var ilproc = method.Body.GetILProcessor();
                    ilproc.Append(Instruction.Create(OpCodes.Ldstr, prop.Name + " can't be set during initialization."));
                    ilproc.Append(Instruction.Create(OpCodes.Newobj, invalidOperationStrCtorRef));
                    ilproc.Append(Instruction.Create(OpCodes.Throw));
                }
            }
        }


        private void GeneratePropertyMethodsForUpdateProxy(
            PropertyMapping prop,
            PropertyDefinition proxyPropDef,
            TypeReference proxyBaseDefinition,
            TypeReference proxyTargetType)
        {
            if (prop.IsWriteable)
                GeneratePropertyProxyMethods(prop, proxyPropDef, proxyBaseDefinition, proxyTargetType);
            else
            {
                var invalidOperationStrCtor = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                var invalidOperationStrCtorRef = this.module.Import(invalidOperationStrCtor);

                foreach (var method in new[] { proxyPropDef.GetMethod, proxyPropDef.SetMethod })
                {
                    var ilproc = method.Body.GetILProcessor();
                    ilproc.Append(Instruction.Create(OpCodes.Ldstr, "Illegal to update remote property " + prop.Name));
                    ilproc.Append(Instruction.Create(OpCodes.Newobj, invalidOperationStrCtorRef));
                    ilproc.Append(Instruction.Create(OpCodes.Throw));
                }
            }
        }


        private void GeneratePropertyProxyMethods(
            PropertyMapping prop,
            PropertyDefinition proxyPropDef,
            TypeReference proxyBaseDefinition,
            TypeReference proxyTargetType)
        {
            var proxyOnPropertyGetMethod =
                this.module.Import(proxyBaseDefinition.Resolve().Methods.First(x => x.Name == "OnPropertyGet"));
            var proxyOnPropertySetMethod =
                this.module.Import(proxyBaseDefinition.Resolve().Methods.First(x => x.Name == "OnPropertySet"));

            var getterOpcodes = proxyPropDef.GetMethod.Body.GetILProcessor();
            getterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_0));
            getterOpcodes.Append(Instruction.Create(OpCodes.Ldstr, prop.Name));
            getterOpcodes.Append(Instruction.Create(OpCodes.Call, proxyOnPropertyGetMethod));
            if (prop.PropertyType.IsValueType)
                getterOpcodes.Append(Instruction.Create(OpCodes.Unbox_Any, proxyPropDef.PropertyType));
            else
                getterOpcodes.Append(Instruction.Create(OpCodes.Castclass, proxyPropDef.PropertyType));
            getterOpcodes.Append(Instruction.Create(OpCodes.Ret));

            var setterOpcodes = proxyPropDef.SetMethod.Body.GetILProcessor();
            setterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_0));
            setterOpcodes.Append(Instruction.Create(OpCodes.Ldstr, prop.Name));
            setterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_1));
            if (prop.PropertyType.IsValueType)
                setterOpcodes.Append(Instruction.Create(OpCodes.Box, proxyPropDef.PropertyType));
            setterOpcodes.Append(Instruction.Create(OpCodes.Call, proxyOnPropertySetMethod));
            setterOpcodes.Append(Instruction.Create(OpCodes.Ret));
        }


        private TypeReference GetClientTypeReference(Type type)
        {
            TypeReference typeReference;
            if (PomonaClientEmbeddingEnabled)
            {
                // clientBaseTypeRef = this.module.GetType("Pomona.Client.ClientBase`1");
                typeReference = this.module.GetType(type.FullName);
            }
            else
                typeReference = this.module.Import(type);
            return typeReference;
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
                    this.module.TypeSystem.Void);
                def.DeclaringType.Methods.Add(initPropertyWrappersMethod);

                initPropertyWrappersMethod.Body.MaxStackSize = 8;
                var il = initPropertyWrappersMethod.Body.GetILProcessor();
                il.Emit(OpCodes.Ret);

                var cctor = new MethodDefinition(
                    ".cctor",
                    MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.SpecialName
                    | MethodAttributes.RTSpecialName | MethodAttributes.Static,
                    this.module.TypeSystem.Void);

                def.DeclaringType.Methods.Add(cctor);

                cctor.Body.MaxStackSize = 8;
                var cctorIl = cctor.Body.GetILProcessor();
                cctorIl.Emit(OpCodes.Call, initPropertyWrappersMethod);
                cctorIl.Emit(OpCodes.Ret);
            }
            return initPropertyWrappersMethod;
        }


        private TypeReference GetPropertyTypeReference(PropertyMapping prop)
        {
            TypeReference propTypeRef;
            if (prop.IsOneToManyCollection
                && this.typeMapper.Filter.ClientPropertyIsExposedAsRepository(prop.PropertyInfo))
            {
                var elementTypeReference = GetTypeReference(prop.PropertyType.CollectionElementType);
                propTypeRef =
                    GetClientTypeReference(typeof(ClientRepository<,>)).MakeGenericInstanceType(
                        elementTypeReference, elementTypeReference);
            }
            else
                propTypeRef = GetTypeReference(prop.PropertyType);
            return propTypeRef;
        }


        private TypeReference GetProxyType(string proxyTypeName)
        {
            if (PomonaClientEmbeddingEnabled)
                return this.module.Types.First(x => x.Name == proxyTypeName);
            else
                return this.module.Import(typeof(ClientBase).Assembly.GetTypes().First(x => x.Name == proxyTypeName));
        }


        private TypeReference GetTypeReference(IMappedType type)
        {
            // TODO: Cache typeRef

            var sharedType = type as SharedType;
            var transformedType = type as TransformedType;
            TypeReference typeRef = null;

            if (type.CustomClientLibraryType != null)
                typeRef = this.module.Import(type.CustomClientLibraryType);
            else if (sharedType != null)
            {
                typeRef = this.module.Import(sharedType.TargetType);

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
            else if (transformedType != null)
                typeRef = this.toClientTypeDict[transformedType].InterfaceType;

            if (typeRef == null)
                throw new InvalidOperationException("Unable to get TypeReference for IMappedType");

            return typeRef;
        }

        #region Nested type: TypeCodeGenInfo

        private class TypeCodeGenInfo
        {
            public MethodDefinition EmptyPocoCtor { get; set; }
            public TypeDefinition InterfaceType { get; set; }

            public TypeDefinition LazyProxyType { get; set; }
            public TypeDefinition PocoType { get; set; }
            public TypeDefinition PostFormType { get; set; }
            public TypeDefinition PutFormType { get; set; }

            public TransformedType TransformedType { get; set; }
            public TypeDefinition UriBaseType { get; set; }
        }

        #endregion
    }
}