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
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using NuGet;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;
using CustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using ResourceType = Pomona.Common.TypeSystem.ResourceType;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Pomona.CodeGen
{
    public class ClientLibGenerator
    {
        private static readonly Assembly commonAssembly = typeof (IClientResource).Assembly;
        private readonly TypeMapper typeMapper;
        private readonly Dictionary<Type, TypeReference> typeReferenceCache = new Dictionary<Type, TypeReference>();
        private string assemblyName;
        private TypeDefinition clientInterface;
        private Dictionary<TypeSpec, TypeCodeGenInfo> clientTypeInfoDict;
        private Dictionary<EnumTypeSpec, TypeReference> enumClientTypeDict;
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
            get { return this.Import(typeof (string)); }
        }

        private TypeReference VoidTypeRef
        {
            get { return this.Import(typeof (void)); }
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

            var version = new Version(string.Join(".", typeMapper.Filter.ApiVersion.Split('.').Pad(4, "0").Take(4)));
            if (PomonaClientEmbeddingEnabled)
            {
                var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
                assembly = AssemblyDefinition.ReadAssembly(typeof (ResourceBase).Assembly.Location, readerParameters);
                assembly.CustomAttributes.Clear();
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
                        new AssemblyNameDefinition(assemblyName, version),
                        assemblyName,
                        moduleParameters);
            }

            assembly.Name = new AssemblyNameDefinition(assemblyName, version);

            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            module = assembly.MainModule;
            module.Name = assemblyName + ".dll";
            module.Mvid = Guid.NewGuid();

            clientTypeInfoDict = new Dictionary<TypeSpec, TypeCodeGenInfo>();
            enumClientTypeDict = new Dictionary<EnumTypeSpec, TypeReference>();

            BuildEnumTypes();

            BuildInterfacesAndPocoTypes(transformedTypes);


            CreateProxies(
                new WrappedPropertyProxyBuilder(
                    module,
                    GetProxyType("LazyProxyBase"),
                    Import(typeof (PropertyWrapper<,>)).Resolve()),
                (info, def) => { info.LazyProxyType = def; });

            CreateProxies(
                new PatchFormProxyBuilder(this, MakeProxyTypesPublic),
                (info, def) => { info.PatchFormType = def; },
                typeIsGeneratedPredicate: x => x.TransformedType.PatchAllowed);

            CreateProxies(
                new PostFormProxyBuilder(this),
                (info, def) => { info.PostFormType = def; },
                typeIsGeneratedPredicate: x => x.TransformedType.PostAllowed);

            CreateClientInterface("IClient");
            CreateClientType("Client");

            foreach (var typeInfo in clientTypeInfoDict.Values)
                AddResourceInfoAttribute(typeInfo);

            foreach (var typeInfo in clientTypeInfoDict.Values.Where(x => x.CustomRepositoryInterface != null))
                CreateRepositoryInterfaceAndImplementation(typeInfo);

            //AddRepositoryPostExtensionMethods();

            // Copy types from running assembly
            
            if (PomonaClientEmbeddingEnabled)
            {
                foreach (var clientHelperType in module.Types.Where(methodDefinition => !methodDefinition.Namespace.StartsWith(assemblyName) && !string.IsNullOrEmpty(methodDefinition.Namespace)))
                    clientHelperType.Namespace = assemblyName + "." + clientHelperType.Namespace;
            }
            
            var memstream = new MemoryStream();
            assembly.Write(memstream);

            var array = memstream.ToArray();

            stream.Write(array, 0, array.Length);

            //assembly.Write(stream);
        }

        private void CreatePostToResourceExtensionMethods()
        {
        }

        private void CreateClientInterface(string interfaceName)
        {
            clientInterface = new TypeDefinition(
                assemblyName, interfaceName, TypeAttributes.Interface | TypeAttributes.Public |
                                             TypeAttributes.Abstract);

            clientInterface.Interfaces.Add(Import(typeof (IPomonaClient)));

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


        private PropertyDefinition AddAutomaticProperty(TypeDefinition declaringType, string name,
                                                        TypeReference propertyType)
        {
            FieldDefinition _;
            return AddAutomaticProperty(declaringType, name, propertyType, out _);
        }

        private PropertyDefinition AddAutomaticProperty(
            TypeDefinition declaringType, string name, TypeReference propertyType, out FieldDefinition propField)
        {
            var propertyDefinition = AddProperty(declaringType, name, propertyType);

            propField =
                new FieldDefinition(
                    "_" + name.LowercaseFirstLetter(),
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
            foreach (var resourceTypeInfo in GetAllUriBaseTypesExposedAsRepositories())
            {
                var transformedType = (ResourceType)resourceTypeInfo.TransformedType;
                var repoPropName = transformedType.PluralName;

                var repoPropType = resourceTypeInfo.CustomRepositoryInterface;

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

        private void CreateRepositoryInterfaceAndImplementation(TypeCodeGenInfo resourceTypeInfo)
        {
            var queryableRepoType =
                Import(typeof (IQueryableRepository<>))
                    .MakeGenericInstanceType(resourceTypeInfo.InterfaceType);

            var interfacesToImplement = new List<TypeReference> { queryableRepoType };

            var tt = resourceTypeInfo.TransformedType as ResourceType;
            if (tt.PatchAllowed || tt.MergedTypes.Any(x => x.PatchAllowed))
            {
                interfacesToImplement.Add(
                    Import(typeof (IPatchableRepository<>))
                        .MakeGenericInstanceType(resourceTypeInfo.InterfaceType));
            }

            if (tt.PostAllowed ||
                tt.MergedTypes.Any(x => x.PostAllowed))
            {
                interfacesToImplement.Add(
                    Import(typeof (IPostableRepository<,>))
                        .MakeGenericInstanceType(resourceTypeInfo.InterfaceType,
                                                 clientTypeInfoDict[tt.PostReturnType]
                                                     .InterfaceType));
            }

            var repoInterface = CreateRepositoryType(resourceTypeInfo.CustomRepositoryInterface,
                                                    resourceTypeInfo,
                                                     MethodAttributes.Abstract |
                                                     MethodAttributes.HideBySig |
                                                     MethodAttributes.NewSlot |
                                                     MethodAttributes.Virtual |
                                                     MethodAttributes.Public, "I{0}Repository",
                                                     TypeAttributes.Interface | TypeAttributes.Public |
                                                     TypeAttributes.Abstract,
                                                     false,
                                                     interfacesToImplement);

            var repoImplementation = CreateRepositoryType(new TypeDefinition(null, null, 0),  resourceTypeInfo,
                                                          MethodAttributes.NewSlot |
                                                          MethodAttributes.HideBySig
                                                          | MethodAttributes.Virtual |
                                                          MethodAttributes.Public, "{0}Repository",
                                                          TypeAttributes.Public, true, repoInterface.WrapAsEnumerable());
        }

        private IEnumerable<TypeCodeGenInfo> GetAllUriBaseTypesExposedAsRepositories()
        {
            return clientTypeInfoDict.Values.Where(
                x => x.UriBaseType == x.InterfaceType && x.TransformedType.Maybe().OfType<ResourceType>().Select(y => y.IsRootResource && y.IsExposedAsRepository).OrDefault());
        }

        private TypeDefinition CreateRepositoryType(TypeDefinition repoTypeDef, TypeCodeGenInfo rti, MethodAttributes methodAttributes,
                                                    string repoTypeNameFormat, TypeAttributes typeAttributes,
                                                    bool isImplementation,
                                                    IEnumerable<TypeReference> interfacesToImplement = null)
        {
            var tt = (ResourceType)rti.TransformedType;

            repoTypeDef.Namespace = assemblyName;
            repoTypeDef.Name = string.Format(repoTypeNameFormat, rti.TransformedType.Name);
            repoTypeDef.Attributes = typeAttributes;

            if (isImplementation)
                repoTypeDef.BaseType = rti.CustomRepositoryBaseTypeReference;

            repoTypeDef.Interfaces.AddRange(interfacesToImplement ?? Enumerable.Empty<TypeReference>());
            var baseTypeGenericDef = rti.CustomRepositoryBaseTypeDefinition;
            var baseTypeGenericArgs = rti.CustomRepositoryBaseTypeReference.GenericArguments.ToArray();

            foreach (var subType in tt.MergedTypes.Concat(tt))
            {
                if (subType.PostAllowed)
                {
                    AddRepositoryFormPostMethod(methodAttributes, isImplementation, subType, repoTypeDef,
                                                baseTypeGenericDef,
                                                baseTypeGenericArgs);
                }
            }
            if (tt.PrimaryId != null)
            {
                AddRepositoryGetByIdMethod(rti, methodAttributes, isImplementation, tt, repoTypeDef, baseTypeGenericDef,
                                           baseTypeGenericArgs, "Get");
                AddRepositoryGetByIdMethod(rti, methodAttributes, isImplementation, tt, repoTypeDef, baseTypeGenericDef,
                                           baseTypeGenericArgs, "GetLazy");
            }

            // Constructor
            if (isImplementation)
            {
                var baseCtor = baseTypeGenericDef.GetConstructors().First();
                var baseCtorRef =
                    this.Import(baseCtor).MakeHostInstanceGeneric(baseTypeGenericArgs);

                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    VoidTypeRef);

                baseCtor.Parameters.Select(x => new ParameterDefinition(x.Name, x.Attributes, this.Import(x.ParameterType)))
                        .AddTo(ctor.Parameters);

                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                foreach (var ctorParam in ctor.Parameters)
                {
                    ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg, ctorParam));
                }
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorRef));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));
                repoTypeDef.Methods.Add(ctor);
            }

            if (!module.Types.Contains(repoTypeDef))
                module.Types.Add(repoTypeDef);
            return repoTypeDef;
        }

        private void AddRepositoryGetByIdMethod(TypeCodeGenInfo rti, MethodAttributes methodAttributes,
                                                bool isImplementation,
                                                TransformedType tt, TypeDefinition repoTypeDef,
                                                TypeDefinition baseTypeGenericDef, TypeReference[] baseTypeGenericArgs, string methodName)
        {
            var method = new MethodDefinition(methodName, methodAttributes, rti.InterfaceType);
            var idType = tt.PrimaryId.PropertyType;
            if (!(idType is TypeSpec))
                throw new NotSupportedException("Id needs to be a shared type.");
            var idTypeRef = Import(idType.Type);
            method.Parameters.Add(new ParameterDefinition(tt.PrimaryId.LowerCaseName, 0,
                                                          idTypeRef));
            repoTypeDef.Methods.Add(method);

            if (isImplementation)
            {
                var baseGetMethodRef =
                    this.Import(Import(typeof(ClientRepository<,>)).Resolve().Methods.First(x => x.Name == methodName))
                                                    .MakeHostInstanceGeneric(baseTypeGenericArgs);
                var ilproc = method.Body.GetILProcessor();

                ilproc.Emit(OpCodes.Ldarg_0);
                ilproc.Emit(OpCodes.Ldarg_1);
                if (idType.Type.IsValueType)
                {
                    ilproc.Emit(OpCodes.Box, idTypeRef);
                }
                else
                {
                    ilproc.Emit(OpCodes.Castclass, idTypeRef);
                }
                ilproc.Emit(OpCodes.Callvirt, baseGetMethodRef);
                ilproc.Emit(OpCodes.Ret);
            }
        }

        private void AddRepositoryFormPostMethod(MethodAttributes methodAttributes, bool isImplementation,
                                                 ResourceType subType,
                                                 TypeDefinition repoTypeDef, TypeDefinition baseTypeGenericDef,
                                                 TypeReference[] baseTypeGenericArgs)
        {
            var postReturnTypeRef = clientTypeInfoDict[subType.PostReturnType].InterfaceType;
            var method = new MethodDefinition("Post",
                                              methodAttributes,
                                              postReturnTypeRef);
            method.Parameters.Add(new ParameterDefinition("form", 0, clientTypeInfoDict[subType].PostFormType));
            repoTypeDef.Methods.Add(method);

            if (isImplementation)
            {
                var basePostMethodRef =
                    this.Import(
                        Import(typeof(ClientRepository<,>)).Resolve().GetMethods()
                                          .Single(
                                              x =>
                                              x.Name == "Post" && x.Parameters.Count == 1 &&
                                              x.Parameters[0].Name == "form")
                                          ).MakeHostInstanceGeneric(baseTypeGenericArgs);

                var ilproc = method.Body.GetILProcessor();

                ilproc.Emit(OpCodes.Ldarg_0);
                ilproc.Emit(OpCodes.Ldarg_1);

                ilproc.Emit(OpCodes.Callvirt, basePostMethodRef);

                ilproc.Emit(OpCodes.Castclass, postReturnTypeRef);
                ilproc.Emit(OpCodes.Ret);
            }
        }

        private CustomAttribute AddAttribute(ICustomAttributeProvider interfacePropDef, Type attributeType)
        {
            var attr = Import(attributeType);
            var ctor =
                this.Import(attr.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var custAttr =
                new CustomAttribute(ctor);

            interfacePropDef.CustomAttributes.Add(custAttr);
            return custAttr;
        }

        private TypeReference Import(TypeReference typeReference)
        {
            if (typeReference is TypeDefinition && typeReference.Module == module)
                return typeReference;
            return module.Import(typeReference);
        }


        private MethodReference Import(MethodReference methodReference)
        {
            if (methodReference is MethodDefinition && methodReference.Module == module)
                return methodReference;
            return module.Import(methodReference);
        }

        private MethodReference Import(MethodBase methodBase)
        {
            if (PomonaClientEmbeddingEnabled && methodBase.DeclaringType.Assembly == commonAssembly)
            {
                return
                    module.GetType(methodBase.DeclaringType.FullName)
                        .Methods.First(
                            x =>
                                x.Name == methodBase.Name &&
                                ParametersAreEqual(methodBase, x));
            }
            return module.Import(methodBase);
        }

        private static bool ParametersAreEqual(MethodBase methodBase, MethodDefinition methodDefinition)
        {
            return methodDefinition.Parameters.Select(y => y.ParameterType.FullName)
                .SequenceEqual(methodBase.GetParameters().Select(y => y.ParameterType.FullName));
        }


        private void AddResourceInfoAttribute(TypeCodeGenInfo typeInfo)
        {
            var interfaceDef = typeInfo.InterfaceType;
            var type = typeInfo.TransformedType;
            var attr = Import(typeof (ResourceInfoAttribute));
            var methodDefinition =
                Import(attr.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var custAttr =
                new CustomAttribute(methodDefinition);
            var stringTypeReference = module.TypeSystem.String;
            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "UrlRelativePath", new CustomAttributeArgument(stringTypeReference, type.Maybe().OfType<ResourceType>().Select(x => x.UriRelativePath).OrDefault())));

            var typeTypeReference = Import(typeof (Type));
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
                    "PatchFormType", new CustomAttributeArgument(typeTypeReference, typeInfo.PatchFormType)));

            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "JsonTypeName", new CustomAttributeArgument(stringTypeReference, type.Name)));

            custAttr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "UriBaseType", new CustomAttributeArgument(typeTypeReference, typeInfo.UriBaseType)));

            var resourceType = typeInfo.TransformedType as ResourceType;
            if (resourceType != null && resourceType.ParentResourceType != null)
            {
                var parentResourceTypeInfo = clientTypeInfoDict[resourceType.ParentResourceType];
                custAttr.Properties.Add(
                    new CustomAttributeNamedArgument("ParentResourceType",
                        new CustomAttributeArgument(typeTypeReference, parentResourceTypeInfo.InterfaceType)));
            }

            if (typeInfo.BaseType != null)
            {
                custAttr.Properties.Add(
                    new CustomAttributeNamedArgument(
                        "BaseType", new CustomAttributeArgument(typeTypeReference, typeInfo.BaseType)));
            }

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
                    this.Import(typeof (Enum)));

                var fieldDef = new FieldDefinition(
                    "value__",
                    FieldAttributes.FamANDAssem | FieldAttributes.Family
                    | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
                    this.Import(module.TypeSystem.Int32));

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
            var resourceBaseRef = Import(typeof (ResourceBase));
            var resourceInterfaceRef = Import(typeof (IClientResource));

            var resourceBaseCtor =
                this.Import(
                    resourceBaseRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 0));
            foreach (var transformedType in transformedTypes)
            {
                var typeInfo = new TypeCodeGenInfo(this, transformedType);
                clientTypeInfoDict[transformedType] = typeInfo;

                typeInfo.InterfaceType.Namespace = assemblyName;

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

                module.Types.Add(typeInfo.InterfaceType);
                module.Types.Add(pocoDef);
            }

            foreach (var kvp in clientTypeInfoDict)
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

                if (type.BaseType != null && clientTypeInfoDict.ContainsKey(type.BaseType))
                {
                    var baseTypeInfo = clientTypeInfoDict[type.BaseType];
                    pocoDef.BaseType = baseTypeInfo.PocoType;

                    baseCtorReference = baseTypeInfo.PocoType.GetConstructors().First(x => x.Parameters.Count == 0);

                    interfaceDef.Interfaces.Add(baseTypeInfo.InterfaceType);
                    typeInfo.BaseType = baseTypeInfo.InterfaceType;
                }
                else
                {
                    interfaceDef.Interfaces.Add(resourceInterfaceRef);
                    pocoDef.BaseType = resourceBaseRef;
                    baseCtorReference = resourceBaseCtor;
                    typeInfo.BaseType = null;
                }

                typeInfo.UriBaseType =
                    type.Maybe()
                    .OfType<ResourceType>()
                    .Select(x => x.UriBaseType)
                    .Select(x => clientTypeInfoDict[x].InterfaceType)
                    .OrDefault();


                var ctorIlActions = new List<Action<ILProcessor>>();

                foreach (
                    var prop in
                        classMapping.Properties.Where(x => x.DeclaringType == classMapping))
                {
                    var propTypeRef = GetPropertyTypeReference(prop);

                    // For interface getters and setters
                    var interfacePropDef = AddInterfaceProperty(interfaceDef, prop.Name, propTypeRef);

                    if (prop.IsAttributesProperty)
                    {
                        AddAttribute(interfacePropDef, typeof (ResourceAttributesPropertyAttribute));
                    }
                    if (prop.IsEtagProperty)
                    {
                        AddAttribute(interfacePropDef, typeof (ResourceEtagPropertyAttribute));
                    }
                    if (prop.IsPrimaryKey)
                    {
                        AddAttribute(interfacePropDef, typeof (ResourceIdPropertyAttribute));
                    }
                    AddAttribute(interfacePropDef, typeof(ResourcePropertyAttribute)).Properties.Add(
                        new CustomAttributeNamedArgument("AccessMode",
                            new CustomAttributeArgument(Import(typeof(HttpMethod)),
                                prop.AccessMode)));

                    FieldDefinition backingField;
                    AddAutomaticProperty(pocoDef, prop.Name, propTypeRef, out backingField);
                    if (!prop.ExposedAsRepository)
                        AddPropertyFieldInitialization(backingField, prop.PropertyType, ctorIlActions);
                }

                var ctor = typeInfo.EmptyPocoCtor;
                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorReference));
                foreach (var ilAction in ctorIlActions)
                {
                    ilAction(ctorIlProcessor);
                }
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));
            }
        }

        private void AddPropertyFieldInitialization(FieldDefinition backingField, TypeSpec propertyType,
                                                    List<Action<ILProcessor>> ctorIlActions)
        {
            if (propertyType.Maybe().OfType<TransformedType>().Select(x => x.MappedAsValueObject).OrDefault())
            {
                var typeInfo = clientTypeInfoDict[propertyType];

                ctorIlActions.Add(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Newobj, typeInfo.EmptyPocoCtor);
                        il.Emit(OpCodes.Stfld, backingField);
                    });
            }
            else if (propertyType.IsCollection)
            {
                var genericInstanceFieldType = (GenericInstanceType)backingField.FieldType;
                var listReference = Import(typeof (List<>));
                var listCtor = listReference.Resolve().GetConstructors().First(x => x.Parameters.Count == 0);
                var listCtorInstance = Import(listCtor).MakeHostInstanceGeneric(genericInstanceFieldType.GenericArguments[0]);
                ctorIlActions.Add(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Newobj, listCtorInstance);
                        il.Emit(OpCodes.Stfld, backingField);
                    });
            }
            else if (propertyType.IsDictionary)
            {
                var genericInstanceFieldType = (GenericInstanceType)backingField.FieldType;
                var dictReference = Import(typeof (Dictionary<,>));
                var dictCtor = dictReference.Resolve().GetConstructors().First(x => x.Parameters.Count == 0);
                var dictCtorInstance =
                    this.Import(dictCtor).MakeHostInstanceGeneric(genericInstanceFieldType.GenericArguments.ToArray());
                ctorIlActions.Add(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Newobj, dictCtorInstance);
                        il.Emit(OpCodes.Stfld, backingField);
                    });
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
            var clientBaseTypeRef = Import(typeof (ClientBase<>));

            var clientTypeDefinition = new TypeDefinition(
                assemblyName, clientTypeName, TypeAttributes.Public);

            clientTypeDefinition.Interfaces.Add(clientInterface);

            var clientBaseTypeGenericInstance = clientBaseTypeRef.MakeGenericInstanceType(clientTypeDefinition);
            clientTypeDefinition.BaseType = clientBaseTypeGenericInstance;

            var clientBaseTypeCtor =
                this.Import(
                    clientBaseTypeRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 2)).MakeHostInstanceGeneric(clientTypeDefinition);

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
                                                            Import(typeof (IWebClient))));
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
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Call,  clientBaseTypeCtor));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            clientTypeDefinition.Methods.Add(ctor);
        }


        private void CreateProxies(
            ProxyBuilder proxyBuilder,
            Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated,
            Func<TypeCodeGenInfo, bool> typeIsGeneratedPredicate = null)
        {
            typeIsGeneratedPredicate = typeIsGeneratedPredicate ?? (x => true);
            var generatedTypeDict = new Dictionary<TypeCodeGenInfo, TypeDefinition>();

            foreach (var typeInfo in clientTypeInfoDict.Values.Where(typeIsGeneratedPredicate))
            {
                generatedTypeDict.GetOrCreate(typeInfo,
                                              () =>
                                              CreateProxy(proxyBuilder, onTypeGenerated, typeInfo, generatedTypeDict));
            }
        }

        private TypeDefinition CreateProxy(ProxyBuilder proxyBuilder,
                                           Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated,
                                           TypeCodeGenInfo typeInfo,
                                           Dictionary<TypeCodeGenInfo, TypeDefinition> generatedTypeDict)
        {
            var targetType = typeInfo.TransformedType;
            var name = targetType.Name;

            TypeDefinition baseTypeDef = null;
            var tt = typeInfo.TransformedType;
            var rt = typeInfo.TransformedType as ResourceType;
            if (rt != null && rt.UriBaseType != null && rt.UriBaseType != rt)
            {
                var baseTypeInfo = clientTypeInfoDict[tt.BaseType];
                baseTypeDef = generatedTypeDict.GetOrCreate(baseTypeInfo,
                                                            () =>
                                                            CreateProxy(proxyBuilder, onTypeGenerated, baseTypeInfo,
                                                                        generatedTypeDict));
            }
            var proxyType = proxyBuilder.CreateProxyType(name, typeInfo.InterfaceType.WrapAsEnumerable(), baseTypeDef);

            if (onTypeGenerated != null)
                onTypeGenerated(typeInfo, proxyType);

            return proxyType;
        }


        private TypeReference Import(Type type)
        {
            TypeReference typeReference;

            return typeReferenceCache.GetOrCreate(type, () =>
                {
                    if (PomonaClientEmbeddingEnabled && type.Assembly == typeof (IPomonaClient).Assembly)
                    {
                        // clientBaseTypeRef = this.module.GetType("Pomona.Client.ClientBase`1");
                        typeReference = module.GetType(type.FullName);
                    }
                    else
                        typeReference = module.Import(type);

                    if (typeReference == null)
                        throw new InvalidOperationException("Did not expect to get null when resolving type.");

                    return typeReference;
                });
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
            if (prop.ExposedAsRepository)
            {
                var propType = prop.PropertyType as EnumerableTypeSpec;
                if (propType == null)
                    throw new InvalidOperationException("Can only expose an enumerable type as repository.");
                var resourceInfo = clientTypeInfoDict[propType.ItemType];
                return resourceInfo.CustomRepositoryInterface;
            }
            else
                propTypeRef = GetTypeReference(prop.PropertyType);
            return propTypeRef;
        }


        private TypeReference GetProxyType(string proxyTypeName)
        {
            return Import(typeof (ClientBase).Assembly.GetTypes().First(x => x.Name == proxyTypeName));
        }


        private TypeReference GetTypeReference(TypeSpec type)
        {
            // TODO: Cache typeRef

            var sharedType = type as RuntimeTypeSpec;
            var transformedType = type as TransformedType;
            var enumType = type as EnumTypeSpec;
            TypeReference typeRef = null;

            if (type.GetCustomClientLibraryType() != null)
                typeRef = Import(type.GetCustomClientLibraryType());
            else if (enumType != null)
            {
                if (!enumClientTypeDict.TryGetValue(enumType, out typeRef))
                {
                    throw new InvalidOperationException(string.Format("Generated property has a reference to {0}, but has probably not been included in SourceTypes.", enumType.Type.FullName));
                }
            }
            else if (transformedType != null)
                typeRef = clientTypeInfoDict[transformedType].InterfaceType;
            else if (sharedType != null)
            {
                if (sharedType.Type.IsGenericType)
                    typeRef = Import(sharedType.Type.GetGenericTypeDefinition());
                else
                {
                    typeRef = Import(sharedType.Type);
                }

                if (sharedType.IsGenericType)
                {
                    if (sharedType.GenericArguments.Count() != typeRef.GenericParameters.Count)
                        throw new InvalidOperationException("Generic argument count not matching target type");

                    var typeRefInstance = new GenericInstanceType(typeRef);
                    foreach (var genericArgument in sharedType.GenericArguments)
                        typeRefInstance.GenericArguments.Add(GetTypeReference(genericArgument));

                    typeRef = typeRefInstance;
                }
            }

            if (typeRef == null)
                throw new InvalidOperationException("Unable to get TypeReference for TypeSpec");

            return typeRef;
        }

        #region Nested type: PostFormProxyBuilder

        private class PostFormProxyBuilder : WrappedPropertyProxyBuilder
        {
            private readonly ClientLibGenerator owner;


            public PostFormProxyBuilder(ClientLibGenerator owner, bool isPublic = true)
                : base(
                    owner.module,
                    owner.GetProxyType("PostResourceBase"),
                    owner.Import(typeof (PropertyWrapper<,>)).Resolve(),
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
                var mergedProperties =
                    propertyMapping.WrapAsEnumerable()
                                   .Concat(
                                       propertyMapping.ReflectedType.SubTypes.Select(
                                           x => x.Properties.First(y => y.Name == propertyMapping.Name)));

                if (
                    mergedProperties.Any(
                        x => x.AccessMode.HasFlag(HttpMethod.Post)))
                {
                    base.OnGeneratePropertyMethods(
                        targetProp, proxyProp, proxyBaseType, proxyTargetType, rootProxyTargetType);
                }
                else
                {
                    var invalidOperationStrCtor =
                        typeof (InvalidOperationException).GetConstructor(new[] { typeof (string) });
                    var invalidOperationStrCtorRef = owner.Import(invalidOperationStrCtor);

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
                    getMethod.Overrides.Add(owner.Import(targetProp.GetMethod));
                    if (targetProp.SetMethod != null)
                        setMethod.Overrides.Add(owner.Import(targetProp.SetMethod));
                }
            }
        }

        #endregion

        #region Nested type: TypeCodeGenInfo

        private class TypeCodeGenInfo
        {
            private readonly ClientLibGenerator parent;


            public TypeCodeGenInfo(ClientLibGenerator parent, TransformedType transformedType)
            {
                var resourceType = transformedType as ResourceType;
                if (resourceType != null && resourceType.IsUriBaseType)
                {
                    if ((resourceType.IsRootResource && resourceType.IsExposedAsRepository)
                        || (resourceType.ParentToChildProperty != null
                            && resourceType.ParentToChildProperty.ExposedAsRepository))
                    {
                        this.customRepositoryInterface = new TypeDefinition(parent.assemblyName, string.Format("I{0}Repository", transformedType.Name), TypeAttributes.Interface | TypeAttributes.Public |
                                                     TypeAttributes.Abstract);
                        parent.module.Types.Add(customRepositoryInterface);
                    }
                }

                if (parent == null)
                    throw new ArgumentNullException("parent");
                this.parent = parent;
                this.transformedType = transformedType;

                this.postReturnTypeReference = new System.Lazy<TypeReference>(() =>
                {
                    if (resourceType == null || resourceType.PostReturnType == null)
                        return InterfaceType;
                    return parent.clientTypeInfoDict[resourceType.PostReturnType].InterfaceType;
                });

                this.customRepositoryBaseType = new System.Lazy<Type>(() =>
                {
                    if (resourceType != null)
                    {
                        if (resourceType.IsUriBaseType)
                        {
                            if (resourceType.IsRootResource && resourceType.IsExposedAsRepository)
                            {
                                return typeof(ClientRepository<,>);
                            }
                            if (resourceType.ParentToChildProperty != null
                                && resourceType.ParentToChildProperty.ExposedAsRepository)
                            {
                                return typeof(ChildResourceRepository<,>);
                            }
                        }
                    }
                    return null;
                });

                this.customRepositoryBaseTypeDefinition = new System.Lazy<TypeDefinition>(() =>
                {
                    if (customRepositoryBaseType.Value == null)
                        return null;

                    var typeRef = parent.Import(customRepositoryBaseType.Value);
                    return typeRef as TypeDefinition ?? typeRef.Resolve();
                });

                this.customRepositoryBaseTypeReference = new System.Lazy<TypeReference>(() =>
                {
                    if (customRepositoryBaseType.Value == null)
                        return null;

                    return
                        parent.Import(customRepositoryBaseType.Value).MakeGenericInstanceType(
                            InterfaceType,
                            PostReturnTypeReference);
                });

                interfaceType = new TypeDefinition(
                    parent.assemblyName,
                    "I" + transformedType.Name,
                    TypeAttributes.Interface | TypeAttributes.Public |
                    TypeAttributes.Abstract);
            }


            public MethodDefinition EmptyPocoCtor { get; set; }

            private TypeDefinition interfaceType;
            public TypeDefinition InterfaceType { get { return interfaceType; } }

            public TypeDefinition LazyProxyType { get; set; }
            public TypeDefinition PocoType { get; set; }
            public TypeDefinition PostFormType { get; set; }
            public TypeDefinition PatchFormType { get; set; }

            public TransformedType TransformedType
            {
                get { return this.transformedType; }
            }

            public TypeDefinition UriBaseType { get; set; }

            public TypeDefinition BaseType { get; set; }

            private TypeDefinition customRepositoryInterface;
            private TransformedType transformedType;
            private System.Lazy<TypeReference> postReturnTypeReference;
            public TypeReference PostReturnTypeReference {get { return postReturnTypeReference.Value; }}
            private System.Lazy<Type> customRepositoryBaseType;
            private System.Lazy<TypeReference> customRepositoryBaseTypeReference;
            private System.Lazy<TypeDefinition> customRepositoryBaseTypeDefinition;
            public TypeDefinition CustomRepositoryInterface { get { return customRepositoryInterface; } }

            public TypeDefinition CustomRepositoryBaseTypeDefinition {get { return customRepositoryBaseTypeDefinition.Value; }}

            public GenericInstanceType CustomRepositoryBaseTypeReference
            {
                get
            {
                return (GenericInstanceType)customRepositoryBaseTypeReference.Value;
            }}
        }

        #endregion

        #region Nested type: UpdateProxyBuilder

        private class PatchFormProxyBuilder : WrappedPropertyProxyBuilder
        {
            private readonly ClientLibGenerator owner;


            public PatchFormProxyBuilder(ClientLibGenerator owner, bool isPublic = true)
                : base(
                    owner.module,
                    owner.GetProxyType("PostResourceBase"),
                    owner.Import(typeof (PropertyWrapper<,>)).Resolve(),
                    isPublic)
            {
                this.owner = owner;
                ProxyNameFormat = "{0}PatchForm";
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
                if ((propertyMapping.AccessMode & (HttpMethod.Patch | HttpMethod.Put | HttpMethod.Post)) == 0)
                {
                    var invalidOperationStrCtor =
                        typeof (InvalidOperationException).GetConstructor(new[] { typeof (string) });
                    var invalidOperationStrCtorRef = owner.Import(invalidOperationStrCtor);

                    // Do not disable GETTING of collection types in update proxy, we might want to change
                    // the collection itself.

                    var allowReadingOfProperty = propertyMapping.PropertyType is EnumerableTypeSpec ||
                                                 propertyMapping.PropertyType is DictionaryTypeSpec;

                    var methodsToRestrict = allowReadingOfProperty
                                                ? new[] { proxyProp.SetMethod }
                                                : new[] { proxyProp.SetMethod, proxyProp.GetMethod };

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