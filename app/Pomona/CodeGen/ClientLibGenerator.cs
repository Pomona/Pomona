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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;

namespace Pomona.CodeGen
{
    public class ClientLibGenerator
    {
        private readonly TypeMapper typeMapper;
        private string assemblyName;
        private TypeDefinition clientInterface;
        private Dictionary<IMappedType, TypeCodeGenInfo> clientTypeInfoDict;
        private Dictionary<EnumType, TypeDefinition> enumClientTypeDict;
        private ModuleDefinition module;


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
            get { return module.Import(typeof (string)); }
        }

        private TypeReference VoidTypeRef
        {
            get { return module.Import(typeof (void)); }
        }

        public static void WriteClientLibrary(TypeMapper typeMapper, Stream stream, bool embedPomonaClient = true)
        {
            var clientLibGenerator = new ClientLibGenerator(typeMapper);
            clientLibGenerator.PomonaClientEmbeddingEnabled = embedPomonaClient;
            clientLibGenerator.CreateClientDll(stream);
        }


        public void CreateClientDll(Stream stream)
        {
            var transformedTypes = typeMapper.TransformedTypes.ToList();

            // Use Pomona.Client lib as starting point!
            AssemblyDefinition assembly;

            assemblyName = typeMapper.Filter.GetClientAssemblyName();

            var assemblyResolver = GetAssemblyResolver();

            if (PomonaClientEmbeddingEnabled)
            {
                var readerParameters = new ReaderParameters {AssemblyResolver = assemblyResolver};
                assembly = AssemblyDefinition.ReadAssembly(typeof (ResourceBase).Assembly.Location, readerParameters);
            }
            else
            {
                var moduleParameters = new ModuleParameters
                    {
                        Kind = ModuleKind.Dll,
                        AssemblyResolver = assemblyResolver
                    };
                assembly =
                    AssemblyDefinition.CreateAssembly(
                        new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
                        assemblyName,
                        moduleParameters);
            }

            assembly.Name = new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0));

            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            module = assembly.MainModule;
            module.Name = assemblyName + ".dll";

            if (PomonaClientEmbeddingEnabled)
            {
                foreach (var clientHelperType in module.Types.Where(x => x.Namespace == "Pomona.Common"))
                    clientHelperType.Namespace = assemblyName;
            }

            clientTypeInfoDict = new Dictionary<IMappedType, TypeCodeGenInfo>();
            enumClientTypeDict = new Dictionary<EnumType, TypeDefinition>();

            BuildEnumTypes();

            BuildInterfacesAndPocoTypes(transformedTypes);

            // Add attribute with resource info

            // Create proxy types

            //CreateProxies(
            //    new ProxyBuilder(
            //        module,
            //        "{0}OldProxy",
            //        GetProxyType("LazyProxyBase"),
            //        MakeProxyTypesPublic,
            //        GeneratePropertyProxyMethods),
            //    (info, def) => { info.LazyProxyType = def; });

            CreateProxies(
                new WrappedPropertyProxyBuilder(
                    module,
                    GetProxyType("LazyProxyBase"),
                    GetClientTypeReference(typeof (PropertyWrapper<,>)).Resolve()),
                (info, def) => { info.LazyProxyType = def; });

            CreateProxies(
                new UpdateProxyBuilder(this, MakeProxyTypesPublic),
                (info, def) => { info.PutFormType = def; });

            CreateProxies(
                new PostFormProxyBuilder(this),
                (info, def) => { info.PostFormType = def; });

            CreateClientInterface("IClient");
            CreateClientType("Client");

            foreach (var typeInfo in clientTypeInfoDict.Values)
                AddResourceInfoAttribute(typeInfo);

            // Copy types from running assembly

            var memstream = new MemoryStream();
            assembly.Write(memstream);

            var array = memstream.ToArray();

            stream.Write(array, 0, array.Length);

            //assembly.Write(stream);
        }

        private void CreateClientInterface(string interfaceName)
        {
            clientInterface = new TypeDefinition(
                assemblyName, interfaceName, TypeAttributes.Interface | TypeAttributes.Public |
                                             TypeAttributes.Abstract);

            clientInterface.Interfaces.Add(GetClientTypeReference(typeof (IPomonaClient)));

            AddRepositoryPropertiesToClientType(clientInterface);

            module.Types.Add(clientInterface);
        }

        private DefaultAssemblyResolver GetAssemblyResolver()
        {
            var assemblyResolver = new DefaultAssemblyResolver();

            // Fix for having path to bin directory when running ASP.NET app.
            var extraSearchDir = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).AbsolutePath);
            assemblyResolver.AddSearchDirectory(extraSearchDir);
            return assemblyResolver;
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
            foreach (
                var resourceTypeInfo in
                    clientTypeInfoDict.Values.Where(
                        x => x.UriBaseType == x.InterfaceType && x.TransformedType.IsExposedAsRepository))
            {
                var transformedType = resourceTypeInfo.TransformedType;
                var repoPropName = transformedType.PluralName;
                var postReturnTypeRef = clientTypeInfoDict[transformedType.PostReturnType].InterfaceType;
                var repoPropType =
                    GetClientTypeReference(typeof (IClientRepository<,>)).MakeGenericInstanceType(
                        resourceTypeInfo.InterfaceType, postReturnTypeRef);

                if (clientTypeDefinition.IsInterface)
                {
                    AddInterfaceProperty(clientTypeDefinition, repoPropName, repoPropType, true);
                }
                else
                {
                    var repoProp = AddAutomaticProperty(clientTypeDefinition, repoPropName, repoPropType);
                    repoProp.SetMethod.IsPublic = false;
                }
            }
        }


        private void AddAttributeToProperty(PropertyDefinition interfacePropDef, Type attributeType)
        {
            var attr = GetClientTypeReference(attributeType);
            var ctor =
                module.Import(attr.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var custAttr =
                new CustomAttribute(ctor);

            interfacePropDef.CustomAttributes.Add(custAttr);
        }


        private void AddResourceInfoAttribute(TypeCodeGenInfo typeInfo)
        {
            var interfaceDef = typeInfo.InterfaceType;
            var type = typeInfo.TransformedType;
            var attr = module.Import(typeof (ResourceInfoAttribute));
            var methodDefinition =
                module.Import(attr.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var custAttr =
                new CustomAttribute(methodDefinition);
            var stringTypeReference = module.TypeSystem.String;
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "UrlRelativePath", new CustomAttributeArgument(stringTypeReference, type.UriRelativePath)));

            var typeTypeReference = module.Import(typeof (Type));
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

            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "IsValueObject",
                    new CustomAttributeArgument(module.TypeSystem.Boolean, typeInfo.TransformedType.MappedAsValueObject)));

            interfaceDef.CustomAttributes.Add(custAttr);
            //var attrConstructor = attr.Resolve().GetConstructors();
        }


        private void BuildEnumTypes()
        {
            foreach (var enumType in typeMapper.EnumTypes)
            {
                var typeDef = new TypeDefinition(
                    assemblyName,
                    enumType.Name,
                    TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.Public,
                    module.Import(typeof (Enum)));

                var fieldDef = new FieldDefinition(
                    "value__",
                    FieldAttributes.FamANDAssem | FieldAttributes.Family
                    | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
                    module.Import(module.TypeSystem.Int32));

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
                        module.TypeSystem.Int32);

                    memberFieldDef.Constant = value;

                    typeDef.Fields.Add(memberFieldDef);
                }

                enumClientTypeDict[enumType] = typeDef;

                module.Types.Add(typeDef);
            }
        }


        private void BuildInterfacesAndPocoTypes(IEnumerable<TransformedType> transformedTypes)
        {
            var resourceBaseRef = GetClientTypeReference(typeof (ResourceBase));
            var resourceInterfaceRef = GetClientTypeReference(typeof (IClientResource));

            var resourceBaseCtor =
                module.Import(
                    resourceBaseRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 0));
            foreach (var transformedType in transformedTypes)
            {
                var typeInfo = new TypeCodeGenInfo();
                clientTypeInfoDict[transformedType] = typeInfo;

                typeInfo.TransformedType = transformedType;

                var interfaceDef = new TypeDefinition(
                    assemblyName,
                    "I" + transformedType.Name,
                    TypeAttributes.Interface | TypeAttributes.Public |
                    TypeAttributes.Abstract);

                typeInfo.InterfaceType = interfaceDef;

                var pocoDef = new TypeDefinition(
                    assemblyName, transformedType.Name + "Resource", TypeAttributes.Public);

                typeInfo.PocoType = pocoDef;

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    module.TypeSystem.Void);

                typeInfo.EmptyPocoCtor = ctor;
                pocoDef.Methods.Add(ctor);

                module.Types.Add(interfaceDef);
                module.Types.Add(pocoDef);
            }

            foreach (var kvp in clientTypeInfoDict)
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

                if (type.BaseType != null && clientTypeInfoDict.ContainsKey(type.BaseType))
                {
                    var baseTypeInfo = clientTypeInfoDict[type.BaseType];
                    pocoDef.BaseType = baseTypeInfo.PocoType;

                    baseCtorReference = baseTypeInfo.PocoType.GetConstructors().First(x => x.Parameters.Count == 0);

                    interfaceDef.Interfaces.Add(baseTypeInfo.InterfaceType);
                }
                else
                {
                    interfaceDef.Interfaces.Add(resourceInterfaceRef);
                    pocoDef.BaseType = resourceBaseRef;
                    baseCtorReference = resourceBaseCtor;
                }

                if (type.UriBaseType != null)
                {
                    typeInfo.UriBaseType = clientTypeInfoDict[type.UriBaseType].InterfaceType;
                }

                var ctor = typeInfo.EmptyPocoCtor;
                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorReference));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                foreach (
                    var prop in
                        classMapping.Properties.Cast<PropertyMapping>().Where(x => x.DeclaringType == classMapping))
                {
                    var propTypeRef = GetPropertyTypeReference(prop);

                    // For interface getters and setters
                    var interfacePropDef = AddInterfaceProperty(interfaceDef, prop.Name, propTypeRef);

                    if (prop.IsAttributesProperty)
                    {
                        AddAttributeToProperty(interfacePropDef, typeof (ResourceAttributesPropertyAttribute));
                    }
                    if (prop.IsEtagProperty)
                    {
                        AddAttributeToProperty(interfacePropDef, typeof (ResourceEtagPropertyAttribute));
                    }

                    AddAutomaticProperty(pocoDef, prop.Name, propTypeRef);
                }
            }
        }

        private PropertyDefinition AddInterfaceProperty(TypeDefinition interfaceDef, string propName,
                                                        TypeReference propTypeRef, bool readOnly = false)
        {
            var interfacePropDef = new PropertyDefinition(propName, PropertyAttributes.None, propTypeRef);
            var interfaceGetMethod = new MethodDefinition(
                "get_" + propName,
                MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public,
                propTypeRef);
            interfacePropDef.GetMethod = interfaceGetMethod;
            interfaceDef.Methods.Add(interfaceGetMethod);

            if (!readOnly)
            {
                var interfaceSetMethod = new MethodDefinition(
                    "set_" + propName,
                    MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public,
                    module.TypeSystem.Void);

                interfaceSetMethod.Parameters.Add(
                    new ParameterDefinition(
                        "value",
                        ParameterAttributes.None,
                        propTypeRef));
                interfacePropDef.SetMethod = interfaceSetMethod;
                interfaceDef.Methods.Add(interfaceSetMethod);
            }

            interfaceDef.Properties.Add(interfacePropDef);
            return interfacePropDef;
        }


        private void CreateClientType(string clientTypeName)
        {
            var clientBaseTypeRef = GetClientTypeReference(typeof (ClientBase<>));

            var clientTypeDefinition = new TypeDefinition(
                assemblyName, clientTypeName, TypeAttributes.Public);

            clientTypeDefinition.Interfaces.Add(clientInterface);

            var clientBaseTypeGenericInstance = clientBaseTypeRef.MakeGenericInstanceType(clientTypeDefinition);
            clientTypeDefinition.BaseType = clientBaseTypeGenericInstance;

            var clientBaseTypeCtor =
                module.Import(
                    clientBaseTypeRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 2));
            clientBaseTypeCtor.DeclaringType =
                clientBaseTypeCtor.DeclaringType.MakeGenericInstanceType(clientTypeDefinition);

            CreateClientConstructor(clientBaseTypeCtor, clientTypeDefinition, false);
            CreateClientConstructor(clientBaseTypeCtor, clientTypeDefinition, true);

            AddRepositoryPropertiesToClientType(clientTypeDefinition);

            module.Types.Add(clientTypeDefinition);
        }

        private void CreateClientConstructor(MethodReference clientBaseTypeCtor, TypeDefinition clientTypeDefinition,
                                             bool includeWebClientArgument)
        {
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                | MethodAttributes.Public,
                VoidTypeRef);

            ctor.Parameters.Add(new ParameterDefinition("uri", ParameterAttributes.None, StringTypeRef));

            if (includeWebClientArgument)
            {
                ctor.Parameters.Add(new ParameterDefinition("webClient", ParameterAttributes.None,
                                                            GetClientTypeReference(typeof (IWebClient))));
            }

            ctor.Body.MaxStackSize = 8;
            var ctorIlProcessor = ctor.Body.GetILProcessor();
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            if (includeWebClientArgument)
            {
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_2));
            }
            else
            {
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldnull));
            }
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, clientBaseTypeCtor));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            clientTypeDefinition.Methods.Add(ctor);
        }


        private void CreateProxies(
            ProxyBuilder proxyBuilder,
            Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated)
        {
            foreach (var typeInfo in clientTypeInfoDict.Values)
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
                module.Import(proxyBaseDefinition.Resolve().Methods.First(x => x.Name == "OnPropertyGet"));
            var proxyOnPropertySetMethod =
                module.Import(proxyBaseDefinition.Resolve().Methods.First(x => x.Name == "OnPropertySet"));

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
                typeReference = module.GetType(type.FullName);
            }
            else
                typeReference = module.Import(type);
            return typeReference;
        }


        private PropertyMapping GetPropertyMapping(
            PropertyDefinition propertyDefinition, TypeReference reflectedInterface = null)
        {
            reflectedInterface = reflectedInterface ?? propertyDefinition.DeclaringType;

            return
                clientTypeInfoDict
                    .Values
                    .First(x => x.InterfaceType == reflectedInterface)
                    .TransformedType.Properties.Cast<PropertyMapping>()
                    .First(x => x.Name == propertyDefinition.Name);
        }


        private TypeReference GetPropertyTypeReference(PropertyMapping prop)
        {
            TypeReference propTypeRef;
            if (prop.IsOneToManyCollection
                && typeMapper.Filter.ClientPropertyIsExposedAsRepository(prop.PropertyInfo))
            {
                var resourceInfo = clientTypeInfoDict[prop.PropertyType.ElementType];
                var elementTypeReference = resourceInfo.InterfaceType;
                propTypeRef =
                    GetClientTypeReference(typeof (ClientRepository<,>)).MakeGenericInstanceType(
                        elementTypeReference, elementTypeReference);
            }
            else
                propTypeRef = GetTypeReference(prop.PropertyType);
            return propTypeRef;
        }


        private TypeReference GetProxyType(string proxyTypeName)
        {
            if (PomonaClientEmbeddingEnabled)
                return module.Types.First(x => x.Name == proxyTypeName);
            else
                return module.Import(typeof (ClientBase).Assembly.GetTypes().First(x => x.Name == proxyTypeName));
        }


        private TypeReference GetTypeReference(IMappedType type)
        {
            // TODO: Cache typeRef

            var sharedType = type as SharedType;
            var transformedType = type as TransformedType;
            var enumType = type as EnumType;
            TypeReference typeRef = null;

            if (type.CustomClientLibraryType != null)
                typeRef = module.Import(type.CustomClientLibraryType);
            else if (enumType != null)
                typeRef = enumClientTypeDict[enumType];
            else if (sharedType != null)
            {
                typeRef = module.Import(sharedType.MappedType);

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
                typeRef = clientTypeInfoDict[transformedType].InterfaceType;

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
                    owner.GetClientTypeReference(typeof (PropertyWrapper<,>)).Resolve(),
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
                var propertyMapping = owner.GetPropertyMapping(targetProp, rootProxyTargetType);

                if (propertyMapping.CreateMode == PropertyCreateMode.Required ||
                    propertyMapping.CreateMode == PropertyCreateMode.Optional)
                {
                    base.OnGeneratePropertyMethods(
                        targetProp, proxyProp, proxyBaseType, proxyTargetType, rootProxyTargetType);
                }
                else
                {
                    var invalidOperationStrCtor =
                        typeof (InvalidOperationException).GetConstructor(new[] {typeof (string)});
                    var invalidOperationStrCtorRef = Module.Import(invalidOperationStrCtor);

                    var getMethod = proxyProp.GetMethod;
                    var setMethod = proxyProp.SetMethod;

                    foreach (var method in new[] {getMethod, setMethod})
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
                    owner.GetClientTypeReference(typeof (PropertyWrapper<,>)).Resolve(),
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
                var propertyMapping = owner.GetPropertyMapping(targetProp);
                base.OnGeneratePropertyMethods(
                    targetProp, proxyProp, proxyBaseType, proxyTargetType, rootProxyTargetType);
                if (!propertyMapping.IsWriteable)
                {
                    var invalidOperationStrCtor =
                        typeof (InvalidOperationException).GetConstructor(new[] {typeof (string)});
                    var invalidOperationStrCtorRef = Module.Import(invalidOperationStrCtor);

                    // Do not disable GETTING of collection types in update proxy, we might want to change
                    // the collection itself.

                    var allowReadingOfProperty = propertyMapping.PropertyType.IsCollection ||
                                                 propertyMapping.PropertyType.IsDictionary;

                    var methodsToRestrict = allowReadingOfProperty
                                                ? new[] {proxyProp.SetMethod}
                                                : new[] {proxyProp.SetMethod, proxyProp.GetMethod};

                    foreach (var method in methodsToRestrict)
                    {
                        method.Body.Instructions.Clear();
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