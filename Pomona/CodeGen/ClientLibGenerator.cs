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
using System.Diagnostics;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Pomona.Client;
using Pomona.Client.Internals;
using Pomona.Client.Proxies;

namespace Pomona.CodeGen
{
    public class ClientLibGenerator
    {
        private string assemblyName;
        private Dictionary<IMappedType, TypeCodeGenInfo> clientTypeInfoDict;
        private Dictionary<EnumType, TypeDefinition> enumClientTypeDict;
        private ModuleDefinition module;
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
            var transformedTypes = this.typeMapper.TransformedTypes.ToList();

            // Use Pomona.Client lib as starting point!
            AssemblyDefinition assembly;

            this.assemblyName = this.typeMapper.Filter.GetClientAssemblyName();

            if (PomonaClientEmbeddingEnabled)
                assembly = AssemblyDefinition.ReadAssembly(typeof(ResourceBase).Assembly.Location);
            else
            {
                assembly =
                    AssemblyDefinition.CreateAssembly(
                        new AssemblyNameDefinition(this.assemblyName, new Version(1, 0, 0, 0)),
                        this.assemblyName,
                        ModuleKind.Dll);
            }

            assembly.Name = new AssemblyNameDefinition(this.assemblyName, new Version(1, 0, 0, 0));

            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            this.module = assembly.MainModule;
            this.module.Name = this.assemblyName + ".dll";

            if (PomonaClientEmbeddingEnabled)
            {
                foreach (var clientHelperType in this.module.Types.Where(x => x.Namespace == "Pomona.Client"))
                    clientHelperType.Namespace = this.assemblyName;
            }

            this.clientTypeInfoDict = new Dictionary<IMappedType, TypeCodeGenInfo>();
            this.enumClientTypeDict = new Dictionary<EnumType, TypeDefinition>();

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

            BuildEnumTypes();

            foreach (var transformedType in transformedTypes)
            {
                var typeInfo = new TypeCodeGenInfo();
                this.clientTypeInfoDict[transformedType] = typeInfo;

                typeInfo.TransformedType = (TransformedType)transformedType;

                var interfaceDef = new TypeDefinition(
                    this.assemblyName,
                    "I" + transformedType.Name,
                    TypeAttributes.Interface | TypeAttributes.Public |
                    TypeAttributes.Abstract);

                typeInfo.InterfaceType = interfaceDef;

                var pocoDef = new TypeDefinition(
                    this.assemblyName, transformedType.Name + "Resource", TypeAttributes.Public);

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

            foreach (var kvp in this.clientTypeInfoDict)
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

                if (type.BaseType != null && this.clientTypeInfoDict.ContainsKey(type.BaseType))
                {
                    var baseTypeInfo = this.clientTypeInfoDict[type.BaseType];
                    pocoDef.BaseType = baseTypeInfo.PocoType;

                    baseCtorReference = baseTypeInfo.PocoType.GetConstructors().First(x => x.Parameters.Count == 0);

                    interfaceDef.Interfaces.Add(baseTypeInfo.InterfaceType);

                    typeInfo.UriBaseType = this.clientTypeInfoDict[type.UriBaseType].InterfaceType;
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
                    if (prop.Name == "TheEnumValue")
                        Debugger.Break();

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

            CreateProxies(
                new ProxyBuilder(
                    this.module,
                    "{0}OldProxy",
                    GetProxyType("LazyProxyBase"),
                    MakeProxyTypesPublic,
                    GeneratePropertyProxyMethods),
                (info, def) => { info.LazyProxyType = def; });

            CreateProxies(
                new WrappedPropertyProxyBuilder(
                    this.module,
                    GetProxyType("LazyProxyBase"),
                    GetClientTypeReference(typeof(PropertyWrapper<,>)).Resolve()),
                (info, def) => { info.LazyProxyType = def; });

            CreateProxies(
                new UpdateProxyBuilder(this, MakeProxyTypesPublic),
                (info, def) => { info.PutFormType = def; });

            CreateProxies(
                new PostFormProxyBuilder(this),
                (info, def) => { info.PostFormType = def; });

            CreateClientType("Client");

            foreach (var typeInfo in this.clientTypeInfoDict.Values)
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


        private void AddRepositoryPropertiesToClientType(TypeDefinition clientTypeDefinition)
        {
            foreach (var resourceTypeInfo in this.clientTypeInfoDict.Values.Where(x => x.UriBaseType == x.InterfaceType)
                )
            {
                var transformedType = resourceTypeInfo.TransformedType;
                var repoPropName = transformedType.PluralName;
                var postReturnTypeRef = this.clientTypeInfoDict[transformedType.PostReturnType].InterfaceType;
                var repoPropType =
                    GetClientTypeReference(typeof(ClientRepository<,>)).MakeGenericInstanceType(
                        resourceTypeInfo.InterfaceType, postReturnTypeRef);
                var repoProp = AddAutomaticProperty(clientTypeDefinition, repoPropName, repoPropType);
                repoProp.SetMethod.IsPublic = false;
            }
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


        private void BuildEnumTypes()
        {
            foreach (var enumType in this.typeMapper.EnumTypes)
            {
                var typeDef = new TypeDefinition(
                    this.assemblyName,
                    enumType.Name,
                    TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.Public,
                    this.module.Import(typeof(Enum)));

                var fieldDef = new FieldDefinition(
                    "value__",
                    FieldAttributes.FamANDAssem | FieldAttributes.Family
                    | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
                    this.module.Import(this.module.TypeSystem.Int32));

                typeDef.Fields.Add(fieldDef);

                foreach (var kvp in enumType.EnumValues)
                {
                    var name = kvp.Key;
                    var value = kvp.Value;

                    var memberFieldDef = new FieldDefinition(
                        name,
                        FieldAttributes.FamANDAssem | FieldAttributes.Family
                        | FieldAttributes.Static | FieldAttributes.Literal
                        | FieldAttributes.HasDefault,
                        this.module.TypeSystem.Int32);

                    memberFieldDef.Constant = value;

                    typeDef.Fields.Add(memberFieldDef);
                }

                this.enumClientTypeDict[enumType] = typeDef;

                this.module.Types.Add(typeDef);
            }
        }


        private void CreateClientType(string clientTypeName)
        {
            var clientBaseTypeRef = GetClientTypeReference(typeof(ClientBase<>));

            var clientTypeDefinition = new TypeDefinition(
                this.assemblyName, clientTypeName, TypeAttributes.Public);

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

            AddRepositoryPropertiesToClientType(clientTypeDefinition);

            this.module.Types.Add(clientTypeDefinition);
        }


        private void CreateProxies(
            ProxyBuilder proxyBuilder,
            Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated)
        {
            foreach (var typeInfo in this.clientTypeInfoDict.Values)
            {
                var targetType = typeInfo.TransformedType;
                var name = targetType.Name;

                var proxyType = proxyBuilder.CreateProxyType(name, typeInfo.InterfaceType.WrapAsEnumerable());

                if (onTypeGenerated != null)
                    onTypeGenerated(typeInfo, proxyType);
            }
        }


        private void GeneratePropertyProxyMethods(
            PropertyDefinition targetProp,
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
            getterOpcodes.Append(Instruction.Create(OpCodes.Ldstr, targetProp.Name));
            getterOpcodes.Append(Instruction.Create(OpCodes.Call, proxyOnPropertyGetMethod));
            if (targetProp.PropertyType.IsValueType)
                getterOpcodes.Append(Instruction.Create(OpCodes.Unbox_Any, proxyPropDef.PropertyType));
            else
                getterOpcodes.Append(Instruction.Create(OpCodes.Castclass, proxyPropDef.PropertyType));
            getterOpcodes.Append(Instruction.Create(OpCodes.Ret));

            var setterOpcodes = proxyPropDef.SetMethod.Body.GetILProcessor();
            setterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_0));
            setterOpcodes.Append(Instruction.Create(OpCodes.Ldstr, targetProp.Name));
            setterOpcodes.Append(Instruction.Create(OpCodes.Ldarg_1));
            if (targetProp.PropertyType.IsValueType)
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


        private PropertyMapping GetPropertyMapping(
            PropertyDefinition propertyDefinition, TypeReference reflectedInterface = null)
        {
            reflectedInterface = reflectedInterface ?? propertyDefinition.DeclaringType;

            return
                this.clientTypeInfoDict
                    .Values
                    .First(x => x.InterfaceType == reflectedInterface)
                    .TransformedType.Properties
                    .First(x => x.Name == propertyDefinition.Name);
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
            var enumType = type as EnumType;
            TypeReference typeRef = null;

            if (type.CustomClientLibraryType != null)
                typeRef = this.module.Import(type.CustomClientLibraryType);
            else if (enumType != null)
                typeRef = this.enumClientTypeDict[enumType];
            else if (sharedType != null)
            {
                typeRef = this.module.Import(sharedType.MappedType);

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
                typeRef = this.clientTypeInfoDict[transformedType].InterfaceType;

            if (typeRef == null)
                throw new InvalidOperationException("Unable to get TypeReference for IMappedType");

            return typeRef;
        }

        #region Nested type: PostFormProxyBuilder

        private class PostFormProxyBuilder : WrappedPropertyProxyBuilder
        {
            private readonly ClientLibGenerator owner;


            public PostFormProxyBuilder(ClientLibGenerator owner, bool isPublic = true)
                : base(
                    owner.module,
                    owner.GetProxyType("PutResourceBase"),
                    owner.GetClientTypeReference(typeof(PropertyWrapper<,>)).Resolve(),
                    isPublic)
            {
                this.owner = owner;
                ProxyNameFormat = "{0}Form";
            }


            protected override void OnGeneratePropertyMethods(
                PropertyDefinition targetProp,
                PropertyDefinition proxyProp,
                TypeReference proxyBaseType,
                TypeReference proxyTargetType,
                TypeReference rootProxyTargetType)
            {
                var propertyMapping = this.owner.GetPropertyMapping(targetProp, rootProxyTargetType);

                if (propertyMapping.CreateMode == PropertyMapping.PropertyCreateMode.Required ||
                    propertyMapping.CreateMode == PropertyMapping.PropertyCreateMode.Optional)
                    base.OnGeneratePropertyMethods(
                        targetProp, proxyProp, proxyBaseType, proxyTargetType, rootProxyTargetType);
                else
                {
                    var invalidOperationStrCtor =
                        typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                    var invalidOperationStrCtorRef = Module.Import(invalidOperationStrCtor);

                    var getMethod = proxyProp.GetMethod;
                    var setMethod = proxyProp.SetMethod;

                    foreach (var method in new[] { getMethod, setMethod })
                    {
                        var ilproc = method.Body.GetILProcessor();
                        ilproc.Append(
                            Instruction.Create(
                                OpCodes.Ldstr, propertyMapping.Name + " can't be set during initialization."));
                        ilproc.Append(Instruction.Create(OpCodes.Newobj, invalidOperationStrCtorRef));
                        ilproc.Append(Instruction.Create(OpCodes.Throw));
                    }

                    var explicitPropNamePrefix = targetProp.DeclaringType + ".";
                    proxyProp.Name = explicitPropNamePrefix + targetProp.Name;
                    getMethod.Name = explicitPropNamePrefix + getMethod.Name;
                    setMethod.Name = explicitPropNamePrefix + setMethod.Name;
                    var exlicitMethodAttrs = MethodAttributes.Private | MethodAttributes.Final
                                             | MethodAttributes.HideBySig | MethodAttributes.SpecialName
                                             | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                    getMethod.Attributes = exlicitMethodAttrs;
                    setMethod.Attributes = exlicitMethodAttrs;
                    getMethod.Overrides.Add(Module.Import(targetProp.GetMethod));
                    setMethod.Overrides.Add(Module.Import(targetProp.SetMethod));
                }
            }
        }

        #endregion

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

        #region Nested type: UpdateProxyBuilder

        private class UpdateProxyBuilder : WrappedPropertyProxyBuilder
        {
            private readonly ClientLibGenerator owner;


            public UpdateProxyBuilder(ClientLibGenerator owner, bool isPublic = true)
                : base(
                    owner.module,
                    owner.GetProxyType("PutResourceBase"),
                    owner.GetClientTypeReference(typeof(PropertyWrapper<,>)).Resolve(),
                    isPublic)
            {
                this.owner = owner;
                ProxyNameFormat = "{0}Update";
            }


            protected override void OnGeneratePropertyMethods(
                PropertyDefinition targetProp,
                PropertyDefinition proxyProp,
                TypeReference proxyBaseType,
                TypeReference proxyTargetType,
                TypeReference rootProxyTargetType)
            {
                var propertyMapping = this.owner.GetPropertyMapping(targetProp);
                if (propertyMapping.IsWriteable)
                    base.OnGeneratePropertyMethods(
                        targetProp, proxyProp, proxyBaseType, proxyTargetType, rootProxyTargetType);
                else
                {
                    var invalidOperationStrCtor =
                        typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                    var invalidOperationStrCtorRef = Module.Import(invalidOperationStrCtor);

                    foreach (var method in new[] { proxyProp.GetMethod, proxyProp.SetMethod })
                    {
                        var ilproc = method.Body.GetILProcessor();
                        ilproc.Append(
                            Instruction.Create(
                                OpCodes.Ldstr, "Illegal to update remote property " + propertyMapping.Name));
                        ilproc.Append(Instruction.Create(OpCodes.Newobj, invalidOperationStrCtorRef));
                        ilproc.Append(Instruction.Create(OpCodes.Throw));
                    }
                }
            }
        }

        #endregion
    }
}