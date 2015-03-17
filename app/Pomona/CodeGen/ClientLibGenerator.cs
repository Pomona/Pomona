#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
        private static readonly Assembly commonAssembly = typeof(IClientResource).Assembly;
        private readonly IEnumerable<Assembly> allowedReferencedAssemblies;
        private readonly TypeMapper typeMapper;
        private readonly Dictionary<Type, TypeReference> typeReferenceCache = new Dictionary<Type, TypeReference>();
        private TypeDefinition clientInterface;
        private Dictionary<TypeSpec, TypeCodeGenInfo> clientTypeInfoDict;
        private Dictionary<EnumTypeSpec, TypeReference> enumClientTypeDict;
        private ModuleDefinition module;
        private string @namespace;


        public ClientLibGenerator(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
            this.allowedReferencedAssemblies = new[]
            {
                /*mscorlib*/typeof(string).Assembly,
                typeof(IPomonaClient).Assembly,
                /*System.Core*/typeof(Func<>).Assembly,
                /*System*/ typeof(Uri).Assembly,
                typeof(HashSet<>).Assembly
            };
            PomonaClientEmbeddingEnabled = true;
        }


        public bool MakeProxyTypesPublic { get; set; }
        public bool PomonaClientEmbeddingEnabled { get; set; }

        private TypeReference StringTypeRef
        {
            get { return Import(typeof(string)); }
        }

        private TypeReference VoidTypeRef
        {
            get { return Import(typeof(void)); }
        }


        public void CreateClientDll(Stream stream)
        {
            var structuredTypes = this.typeMapper.SourceTypes.OfType<StructuredType>().ToList();
            this.@namespace = this.typeMapper.Filter.ClientMetadata.Namespace;
            var version = new Version(this.typeMapper.Filter.ApiVersion.PadTo(4));
            var assemblyName = this.typeMapper.Filter.ClientMetadata.AssemblyName;
            var assembly = CreateAssembly(assemblyName, version);

            this.module = assembly.MainModule;
            this.module.Name = assemblyName + ".dll";
            this.module.Mvid = Guid.NewGuid();

            this.clientTypeInfoDict = new Dictionary<TypeSpec, TypeCodeGenInfo>();
            this.enumClientTypeDict = new Dictionary<EnumTypeSpec, TypeReference>();

            BuildEnumTypes();
            BuildStringEnumTypes();

            BuildInterfacesAndPocoTypes(structuredTypes);

            var builder = new WrappedPropertyProxyBuilder(this.module,
                                                          GetProxyType("LazyProxyBase"),
                                                          Import(typeof(PropertyWrapper<,>)).Resolve(),
                                                          proxyNamespace : this.@namespace);
            CreateProxies(builder, (codeGenInfo, typeDefinition) =>
            {
                codeGenInfo.LazyProxyType = typeDefinition;
            });

            CreateProxies(new PatchFormProxyBuilder(this, MakeProxyTypesPublic), (info, def) =>
            {
                info.PatchFormType = def;
            }, typeIsGeneratedPredicate : x => x.StructuredType.PatchAllowed);

            CreateProxies(new PostFormProxyBuilder(this), (info, def) =>
            {
                info.PostFormType = def;
                def.IsAbstract = info.StructuredType.IsAbstract;
            }, typeIsGeneratedPredicate : x => (x.StructuredType.PostAllowed));

            CreateClientInterface(this.typeMapper.Filter.ClientMetadata.InterfaceName);
            CreateClientType(this.typeMapper.Filter.ClientMetadata.Name);
            //CreateClientInterface(clientName.InterfaceTypeName);
            //CreateClientType(clientName.ClientTypeName);

            foreach (var typeInfo in this.clientTypeInfoDict.Values)
                AddResourceInfoAttribute(typeInfo);

            foreach (var typeInfo in this.clientTypeInfoDict.Values.Where(x => x.CustomRepositoryInterface != null))
                CreateRepositoryInterfaceAndImplementation(typeInfo);

            //AddRepositoryPostExtensionMethods();

            // Copy types from running assembly

            if (PomonaClientEmbeddingEnabled)
            {
                foreach (var clientHelperType in this.module.Types
                                                     .Where(methodDefinition =>
                                                                !methodDefinition.Namespace.StartsWith(this.@namespace)
                                                                && !String.IsNullOrEmpty(methodDefinition.Namespace)))
                    clientHelperType.Namespace = this.@namespace + "." + clientHelperType.Namespace;
            }

            AddAssemblyAttributes();

            var memstream = new MemoryStream();
            assembly.Write(memstream);

            var array = memstream.ToArray();

            stream.Write(array, 0, array.Length);

            //assembly.Write(stream);
        }


        public static void WriteClientLibrary(TypeMapper typeMapper, Stream stream, bool embedPomonaClient = true)
        {
            var clientLibGenerator = new ClientLibGenerator(typeMapper)
            {
                PomonaClientEmbeddingEnabled = embedPomonaClient
            };
            clientLibGenerator.CreateClientDll(stream);
        }


        private void AddAssemblyAttributes()
        {
            var assemblyInformationalVersionAttribute = AddAttribute<AssemblyInformationalVersionAttribute>(this.module.Assembly);
            var informationalVersion = this.typeMapper.Filter.ClientMetadata.InformationalVersion;
            var customAttributeArgument = new CustomAttributeArgument(StringTypeRef, informationalVersion);
            assemblyInformationalVersionAttribute.ConstructorArguments.Add(customAttributeArgument);
        }


        private CustomAttribute AddAttribute<TAttribute>(ICustomAttributeProvider interfacePropDef)
            where TAttribute : Attribute
        {
            var attributeType = typeof(TAttribute);
            var attribute = Import(attributeType);
            var constructor = Import(attribute.Resolve().Methods.OrderBy(x => x.Parameters.Count).First(x => x.IsConstructor));
            var customAttribute = new CustomAttribute(constructor);
            interfacePropDef.CustomAttributes.Add(customAttribute);
            return customAttribute;
        }


        private void AddAttribute(ICustomAttributeProvider customAttributeProvider,
                                  CustomAttributeData customAttributeData)
        {
            var ctor = Import(customAttributeData.Constructor);
            var custAttr = new CustomAttribute(ctor);
            foreach (var ctorArg in customAttributeData.ConstructorArguments.EmptyIfNull())
            {
                custAttr.ConstructorArguments.Add(new CustomAttributeArgument(Import(ctorArg.ArgumentType),
                                                                              ctorArg.Value));
            }
            foreach (var ctorParam in customAttributeData.NamedArguments.EmptyIfNull())
            {
                var typedValue = ctorParam.TypedValue;
                custAttr.Properties.Add(new CustomAttributeNamedArgument(ctorParam.MemberInfo.Name,
                                                                         new CustomAttributeArgument(
                                                                             Import(typedValue.ArgumentType),
                                                                             typedValue.Value)));
            }
            customAttributeProvider.CustomAttributes.Add(custAttr);
        }


        private PropertyDefinition AddAutomaticProperty(TypeDefinition declaringType,
                                                        string name,
                                                        TypeReference propertyType)
        {
            FieldDefinition _;
            return AddAutomaticProperty(declaringType, name, propertyType, out _);
        }


        private PropertyDefinition AddAutomaticProperty(TypeDefinition declaringType,
                                                        string name,
                                                        TypeReference propertyType,
                                                        out FieldDefinition propField)
        {
            var propertyDefinition = AddProperty(declaringType, name, propertyType);

            propField = new FieldDefinition("_" + name.LowercaseFirstLetter(),
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


        private PropertyDefinition AddInterfaceProperty(TypeDefinition interfaceDef,
                                                        string propName,
                                                        TypeReference propTypeRef,
                                                        bool readOnly = false)
        {
            var interfacePropDef = new PropertyDefinition(propName, PropertyAttributes.None, propTypeRef);
            var interfaceGetMethod = new MethodDefinition("get_" + propName,
                                                          MethodAttributes.Abstract
                                                          | MethodAttributes.SpecialName
                                                          | MethodAttributes.HideBySig
                                                          | MethodAttributes.NewSlot
                                                          | MethodAttributes.Virtual
                                                          | MethodAttributes.Public,
                                                          propTypeRef);
            interfacePropDef.GetMethod = interfaceGetMethod;
            interfaceDef.Methods.Add(interfaceGetMethod);

            if (!readOnly)
            {
                var interfaceSetMethod = new MethodDefinition("set_" + propName,
                                                              MethodAttributes.Abstract
                                                              | MethodAttributes.SpecialName
                                                              | MethodAttributes.HideBySig
                                                              | MethodAttributes.NewSlot
                                                              | MethodAttributes.Virtual
                                                              | MethodAttributes.Public,
                                                              this.module.TypeSystem.Void);

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


        /// <summary>
        /// Create property with public getter and setter, with no method defined.
        /// </summary>
        private PropertyDefinition AddProperty(TypeDefinition declaringType, string name, TypeReference propertyType)
        {
            var proxyPropDef = new PropertyDefinition(name, PropertyAttributes.None, propertyType);
            var proxyPropGetter = new MethodDefinition("get_" + name,
                                                       MethodAttributes.NewSlot
                                                       | MethodAttributes.SpecialName
                                                       | MethodAttributes.HideBySig
                                                       | MethodAttributes.Virtual
                                                       | MethodAttributes.Public,
                                                       propertyType);
            proxyPropDef.GetMethod = proxyPropGetter;

            var proxyPropSetter = new MethodDefinition("set_" + name,
                                                       MethodAttributes.NewSlot
                                                       | MethodAttributes.SpecialName
                                                       | MethodAttributes.HideBySig
                                                       | MethodAttributes.Virtual
                                                       | MethodAttributes.Public,
                                                       VoidTypeRef);
            proxyPropSetter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propertyType));
            proxyPropDef.SetMethod = proxyPropSetter;

            declaringType.Methods.Add(proxyPropGetter);
            declaringType.Methods.Add(proxyPropSetter);
            declaringType.Properties.Add(proxyPropDef);
            return proxyPropDef;
        }


        private void AddPropertyFieldInitialization(FieldDefinition backingField,
                                                    TypeSpec propertyType,
                                                    List<Action<ILProcessor>> ctorIlActions)
        {
            if (propertyType.MaybeAs<StructuredType>().Select(x => x.MappedAsValueObject).OrDefault())
            {
                var typeInfo = this.clientTypeInfoDict[propertyType];

                ctorIlActions.Add(il =>
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Newobj, typeInfo.EmptyPocoCtor);
                    il.Emit(OpCodes.Stfld, backingField);
                });
            }
            else if (propertyType.IsCollection && !propertyType.IsArray)
            {
                var genericInstanceFieldType = (GenericInstanceType)backingField.FieldType;
                var implementationType = propertyType.Type.IsGenericInstanceOf(typeof(ISet<>))
                    ? typeof(HashSet<>)
                    : typeof(List<>);
                var listReference = Import(implementationType);
                var listCtor = listReference.Resolve().GetConstructors().First(x => x.Parameters.Count == 0);
                var listCtorInstance =
                    Import(listCtor).MakeHostInstanceGeneric(genericInstanceFieldType.GenericArguments[0]);
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
                var dictReference = Import(typeof(Dictionary<,>));
                var dictCtor = dictReference.Resolve().GetConstructors().First(x => x.Parameters.Count == 0);
                var dictCtorInstance =
                    Import(dictCtor).MakeHostInstanceGeneric(genericInstanceFieldType.GenericArguments.ToArray());
                ctorIlActions.Add(il =>
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Newobj, dictCtorInstance);
                    il.Emit(OpCodes.Stfld, backingField);
                });
            }
        }


        private void AddRepositoryFormPostMethod(MethodAttributes methodAttributes,
                                                 bool isImplementation,
                                                 ResourceType subType,
                                                 TypeDefinition repoTypeDef,
                                                 TypeDefinition baseTypeGenericDef,
                                                 TypeReference[] baseTypeGenericArgs)
        {
            var postReturnTypeRef = this.clientTypeInfoDict[subType.PostReturnType].InterfaceType;
            var method = new MethodDefinition("Post",
                                              methodAttributes,
                                              postReturnTypeRef);
            method.Parameters.Add(new ParameterDefinition("form", 0, this.clientTypeInfoDict[subType].PostFormType));
            repoTypeDef.Methods.Add(method);

            if (isImplementation)
            {
                var basePostMethodRef =
                    Import(
                        Import(typeof(ClientRepository<,,>)).Resolve().GetMethods()
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


        private void AddRepositoryPropertiesToClientType(TypeDefinition clientTypeDefinition)
        {
            foreach (var resourceTypeInfo in GetAllUriBaseTypesExposedAsRepositories())
            {
                var transformedType = (ResourceType)resourceTypeInfo.StructuredType;
                var repoPropName = transformedType.PluralName;

                var repoPropType = resourceTypeInfo.CustomRepositoryInterface;

                if (clientTypeDefinition.IsInterface)
                    AddInterfaceProperty(clientTypeDefinition, repoPropName, repoPropType, true);
                else
                {
                    var repoProp = AddAutomaticProperty(clientTypeDefinition, repoPropName, repoPropType);
                    repoProp.SetMethod.IsPublic = false;
                }
            }
        }


        private void AddResourceInfoAttribute(TypeCodeGenInfo typeInfo)
        {
            var interfaceDef = typeInfo.InterfaceType;
            var type = typeInfo.StructuredType;
            var attr = Import(typeof(ResourceInfoAttribute));
            var methodDefinition =
                Import(attr.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var custAttr =
                new CustomAttribute(methodDefinition);
            var stringTypeReference = this.module.TypeSystem.String;

            var namedArgs = custAttr.Properties;

            var uriRelativePath = type
                .Maybe()
                .OfType<ResourceType>()
                .Select(x => x.UrlRelativePath)
                .OrDefault();

            if (uriRelativePath != null)
            {
                namedArgs.Add(
                    new CustomAttributeNamedArgument("UrlRelativePath",
                                                     new CustomAttributeArgument(stringTypeReference,
                                                                                 uriRelativePath)));
            }

            var typeTypeReference = Import(typeof(Type));
            namedArgs.Add(new CustomAttributeNamedArgument("PocoType",
                                                           new CustomAttributeArgument(typeTypeReference,
                                                                                       typeInfo.PocoType)));
            namedArgs.Add(new CustomAttributeNamedArgument("InterfaceType",
                                                           new CustomAttributeArgument(typeTypeReference,
                                                                                       typeInfo.InterfaceType)));
            namedArgs.Add(new CustomAttributeNamedArgument("LazyProxyType",
                                                           new CustomAttributeArgument(typeTypeReference,
                                                                                       typeInfo.LazyProxyType)));
            namedArgs.Add(new CustomAttributeNamedArgument("PostFormType",
                                                           new CustomAttributeArgument(typeTypeReference,
                                                                                       typeInfo.PostFormType)));
            namedArgs.Add(new CustomAttributeNamedArgument("PatchFormType",
                                                           new CustomAttributeArgument(typeTypeReference,
                                                                                       typeInfo.PatchFormType)));

            namedArgs.Add(new CustomAttributeNamedArgument("JsonTypeName",
                                                           new CustomAttributeArgument(stringTypeReference,
                                                                                       type.Name)));

            namedArgs.Add(new CustomAttributeNamedArgument("UriBaseType",
                                                           new CustomAttributeArgument(typeTypeReference,
                                                                                       typeInfo.UriBaseType)));

            var resourceType = typeInfo.StructuredType as ResourceType;
            if (resourceType != null && resourceType.ParentResourceType != null)
            {
                var parentResourceTypeInfo = this.clientTypeInfoDict[resourceType.ParentResourceType];
                namedArgs.Add(new CustomAttributeNamedArgument("ParentResourceType",
                                                               new CustomAttributeArgument(typeTypeReference,
                                                                                           parentResourceTypeInfo
                                                                                               .InterfaceType)));
            }

            if (typeInfo.BaseType != null)
            {
                namedArgs.Add(new CustomAttributeNamedArgument("BaseType",
                                                               new CustomAttributeArgument(typeTypeReference,
                                                                                           typeInfo.BaseType)));
            }

            namedArgs.Add(new CustomAttributeNamedArgument("IsValueObject",
                                                           new CustomAttributeArgument(
                                                               this.module.TypeSystem.Boolean,
                                                               typeInfo.StructuredType
                                                                       .MappedAsValueObject)));

            interfaceDef.CustomAttributes.Add(custAttr);
            //var attrConstructor = attr.Resolve().GetConstructors();
        }


        private void BuildEnumTypes()
        {
            foreach (var enumType in this.typeMapper.EnumTypes.Where(x => !this.typeMapper.Filter.ClientEnumIsGeneratedAsStringEnum(x)))
            {
                var typeDef = new TypeDefinition(this.@namespace,
                                                 enumType.Name,
                                                 TypeAttributes.AutoClass
                                                 | TypeAttributes.AnsiClass
                                                 | TypeAttributes.Sealed
                                                 | TypeAttributes.Public,
                                                 Import(typeof(Enum)));

                var fieldDef = new FieldDefinition("value__",
                                                   FieldAttributes.FamANDAssem | FieldAttributes.Family
                                                   | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
                                                   Import(this.module.TypeSystem.Int32));

                typeDef.Fields.Add(fieldDef);

                foreach (var kvp in enumType.EnumValues)
                {
                    var name = kvp.Key;
                    var value = kvp.Value;

                    var memberFieldDef = new FieldDefinition(name,
                                                             FieldAttributes.FamANDAssem
                                                             | FieldAttributes.Family
                                                             | FieldAttributes.Static
                                                             | FieldAttributes.Literal
                                                             | FieldAttributes.HasDefault,
                                                             typeDef)
                    {
                        Constant = (int)value
                    };

                    typeDef.Fields.Add(memberFieldDef);
                }

                this.enumClientTypeDict[enumType] = typeDef;

                this.module.Types.Add(typeDef);
            }
        }


        private void BuildInterfacesAndPocoTypes(IEnumerable<StructuredType> transformedTypes)
        {
            var resourceBaseRef = Import(typeof(ResourceBase));
            var resourceInterfaceRef = Import(typeof(IClientResource));
            var resourceBaseCtor = Import(
                resourceBaseRef.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 0));

            foreach (var transformedType in transformedTypes)
            {
                var typeInfo = new TypeCodeGenInfo(this, transformedType);
                this.clientTypeInfoDict[transformedType] = typeInfo;

                typeInfo.InterfaceType.Namespace = this.@namespace;

                var pocoDef = new TypeDefinition(
                    this.@namespace,
                    transformedType.Name + "Resource",
                    TypeAttributes.Public);

                typeInfo.PocoType = pocoDef;

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    this.module.TypeSystem.Void);

                typeInfo.EmptyPocoCtor = ctor;
                pocoDef.Methods.Add(ctor);

                this.module.Types.Add(typeInfo.InterfaceType);
                this.module.Types.Add(pocoDef);
            }

            foreach (var kvp in this.clientTypeInfoDict)
            {
                var type = (StructuredType)kvp.Key;
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
                    typeInfo.BaseType = baseTypeInfo.InterfaceType;
                }
                else
                {
                    interfaceDef.Interfaces.Add(resourceInterfaceRef);
                    pocoDef.BaseType = resourceBaseRef;
                    baseCtorReference = resourceBaseCtor;
                    typeInfo.BaseType = null;
                }

                typeInfo.UriBaseType = type
                    .Maybe()
                    .OfType<ResourceType>()
                    .Select(x => x.UriBaseType)
                    .Select(x => this.clientTypeInfoDict[x].InterfaceType)
                    .OrDefault();

                var ctorIlActions = new List<Action<ILProcessor>>();

                foreach (var property in classMapping.Properties.Where(x => x.DeclaringType == classMapping))
                {
                    var propTypeRef = GetPropertyTypeReference(property);

                    // For interface getters and setters
                    var interfacePropDef = AddInterfaceProperty(interfaceDef, property.Name, propTypeRef);

                    if (property.IsAttributesProperty)
                        AddAttribute<ResourceAttributesPropertyAttribute>(interfacePropDef);
                    if (property.IsEtagProperty)
                        AddAttribute<ResourceEtagPropertyAttribute>(interfacePropDef);
                    if (property.IsPrimaryKey)
                        AddAttribute<ResourceIdPropertyAttribute>(interfacePropDef);

                    var accessModeValue = new CustomAttributeArgument(Import(typeof(HttpMethod)), property.AccessMode);
                    var accessModeProperty = new CustomAttributeNamedArgument("AccessMode", accessModeValue);
                    AddAttribute<ResourcePropertyAttribute>(interfacePropDef).Properties.Add(accessModeProperty);

                    var attributes = this.typeMapper.Filter.GetClientLibraryAttributes(property.PropertyInfo);
                    foreach (var attr in attributes)
                        AddAttribute(interfacePropDef, attr);

                    FieldDefinition backingField;
                    AddAutomaticProperty(pocoDef, property.Name, propTypeRef, out backingField);
                    if (!property.MaybeAs<ResourceProperty>().Select(x => x.ExposedAsRepository).OrDefault())
                        AddPropertyFieldInitialization(backingField, property.PropertyType, ctorIlActions);
                }

                var ctor = typeInfo.EmptyPocoCtor;
                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorReference));
                foreach (var ilAction in ctorIlActions)
                    ilAction(ctorIlProcessor);
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));
            }
        }


        private void BuildStringEnumTypes()
        {
            foreach (var enumType in this.typeMapper.EnumTypes.Where(x => this.typeMapper.Filter.ClientEnumIsGeneratedAsStringEnum(x)))
            {
                var tdf = new TypeDefinitionCloner(this.module);
                var typeDef = tdf.Clone(LoadAssemblyHavingType<StringEnumTemplate>().MainModule.GetType(typeof(StringEnumTemplate).FullName));
                typeDef.Namespace = this.@namespace;
                typeDef.Name = enumType.Name;
                typeDef.Attributes = TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed
                                     | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
                //var ctor = typeDef.GetConstructors().First(x => x.Parameters.Count == 1);

                var memberTemplate = typeDef.Fields.First(x => x.Name == "MemberTemplate");
                var memberFieldAttr = memberTemplate.Attributes;
                var memberFieldType = memberTemplate.FieldType;
                typeDef.Fields.Remove(memberTemplate);
                var cctor = typeDef.Methods.First(x => x.Name == ".cctor");
                var ctor = typeDef.Methods.First(x => x.Name == ".ctor" && x.Parameters.Count == 1);
                var defaultField = typeDef.Fields.First(x => x.Name == "defaultValue");
                cctor.Body.Instructions.Clear();
                var ilGen = cctor.Body.GetILProcessor();

                foreach (var kvp in enumType.EnumValues)
                {
                    var name = kvp.Key;
                    var value = kvp.Value;
                    var field = new FieldDefinition(name, memberFieldAttr, memberFieldType);
                    typeDef.Fields.Add(field);

                    ilGen.Emit(OpCodes.Ldstr, name);
                    ilGen.Emit(OpCodes.Newobj, ctor);
                    if (value == 0)
                    {
                        ilGen.Emit(OpCodes.Dup);
                        ilGen.Emit(OpCodes.Stsfld, defaultField);
                    }
                    ilGen.Emit(OpCodes.Stsfld, field);
                }
                ilGen.Emit(OpCodes.Ret);

                this.enumClientTypeDict[enumType] = typeDef;
            }
        }


        private AssemblyDefinition CreateAssembly(string name, Version version)
        {
            AssemblyDefinition assembly;
            var assemblyResolver = GetAssemblyResolver();
            var assemblyName = new AssemblyNameDefinition(name, version);

            if (PomonaClientEmbeddingEnabled)
            {
                // Use Pomona.Common lib as starting point!
                assembly = LoadPomonaCommonAssembly();
                assembly.CustomAttributes.Clear();
            }
            else
            {
                var moduleParameters = new ModuleParameters
                {
                    Kind = ModuleKind.Dll,
                    AssemblyResolver = assemblyResolver
                };
                assembly = AssemblyDefinition.CreateAssembly(assemblyName,
                                                             name,
                                                             moduleParameters);
            }

            assembly.Name = assemblyName;
            return assembly;
        }


        private void CreateClientConstructor(
            MethodReference clientBaseTypeCtor,
            TypeDefinition clientTypeDefinition)
        {
            var ctor = new MethodDefinition(".ctor",
                                            MethodAttributes.HideBySig
                                            | MethodAttributes.SpecialName
                                            | MethodAttributes.RTSpecialName
                                            | MethodAttributes.Public,
                                            VoidTypeRef);

            clientBaseTypeCtor
                .Parameters
                .Select(x => new ParameterDefinition(x.Name, x.Attributes, x.ParameterType))
                .AddTo(ctor.Parameters);

            ctor.Body.MaxStackSize = 8;
            var ctorIlProcessor = ctor.Body.GetILProcessor();
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            foreach (var param in ctor.Parameters)
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg, param));

            ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, clientBaseTypeCtor));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            clientTypeDefinition.Methods.Add(ctor);
        }


        private void CreateClientInterface(string interfaceName)
        {
            this.clientInterface = new TypeDefinition(this.@namespace,
                                                      interfaceName,
                                                      TypeAttributes.Interface
                                                      | TypeAttributes.Public
                                                      | TypeAttributes.Abstract);

            this.clientInterface.Interfaces.Add(Import(typeof(IPomonaClient)));

            AddRepositoryPropertiesToClientType(this.clientInterface);

            this.module.Types.Add(this.clientInterface);
        }


        private void CreateClientType(string clientTypeName)
        {
            var clientBaseTypeRef = Import(typeof(ClientBase<>));

            var clientTypeDefinition = new TypeDefinition(this.@namespace,
                                                          clientTypeName,
                                                          TypeAttributes.Public);

            clientTypeDefinition.Interfaces.Add(this.clientInterface);

            var clientBaseTypeGenericInstance = clientBaseTypeRef.MakeGenericInstanceType(clientTypeDefinition);
            clientTypeDefinition.BaseType = clientBaseTypeGenericInstance;

            clientBaseTypeRef
                .Resolve()
                .GetConstructors()
                .Where(x => !x.IsStatic)
                .Select(x => Import(x).MakeHostInstanceGeneric(clientTypeDefinition))
                .ForEach(x => CreateClientConstructor(x, clientTypeDefinition));

            AddRepositoryPropertiesToClientType(clientTypeDefinition);

            this.module.Types.Add(clientTypeDefinition);
        }


        private void CreateProxies(ProxyBuilder proxyBuilder,
                                   Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated,
                                   Func<TypeCodeGenInfo, bool> typeIsGeneratedPredicate = null)
        {
            typeIsGeneratedPredicate = typeIsGeneratedPredicate ?? (x => true);
            var generatedTypeDict = new Dictionary<TypeCodeGenInfo, TypeDefinition>();

            foreach (var typeInfo in this.clientTypeInfoDict.Values.Where(typeIsGeneratedPredicate))
            {
                generatedTypeDict.GetOrCreate(typeInfo, () => CreateProxy(proxyBuilder,
                                                                          onTypeGenerated,
                                                                          typeInfo,
                                                                          generatedTypeDict));
            }
        }


        private TypeDefinition CreateProxy(ProxyBuilder proxyBuilder,
                                           Action<TypeCodeGenInfo, TypeDefinition> onTypeGenerated,
                                           TypeCodeGenInfo typeInfo,
                                           Dictionary<TypeCodeGenInfo, TypeDefinition> generatedTypeDict)
        {
            var targetType = typeInfo.StructuredType;
            var name = targetType.Name;

            TypeDefinition baseTypeDef = null;
            var tt = typeInfo.StructuredType;
            var rt = typeInfo.StructuredType as ResourceType;
            if (rt != null && rt.UriBaseType != null && rt.UriBaseType != rt)
            {
                var baseTypeInfo = this.clientTypeInfoDict[tt.BaseType];
                baseTypeDef = generatedTypeDict.GetOrCreate(baseTypeInfo,
                                                            () =>
                                                                CreateProxy(proxyBuilder,
                                                                            onTypeGenerated,
                                                                            baseTypeInfo,
                                                                            generatedTypeDict));
            }
            var proxyType = proxyBuilder.CreateProxyType(name, typeInfo.InterfaceType.WrapAsEnumerable(), baseTypeDef);

            if (onTypeGenerated != null)
                onTypeGenerated(typeInfo, proxyType);

            return proxyType;
        }


        private void CreateRepositoryInterfaceAndImplementation(TypeCodeGenInfo resourceTypeInfo)
        {
            var queryableRepoType =
                Import(typeof(IQueryableRepository<>))
                    .MakeGenericInstanceType(resourceTypeInfo.InterfaceType);

            var interfacesToImplement = new List<TypeReference> { queryableRepoType };

            var tt = resourceTypeInfo.StructuredType as ResourceType;

            if (tt.PrimaryId != null)
            {
                interfacesToImplement.Add(
                    Import(typeof(IGettableRepository<,>)).MakeGenericInstanceType(resourceTypeInfo.InterfaceType,
                                                                                   resourceTypeInfo
                                                                                       .PrimaryIdTypeReference));
            }

            if (tt.PatchAllowed || tt.MergedTypes.Any(x => x.PatchAllowed))
            {
                interfacesToImplement.Add(
                    Import(typeof(IPatchableRepository<>))
                        .MakeGenericInstanceType(resourceTypeInfo.InterfaceType));
            }

            if (tt.PostAllowed ||
                tt.MergedTypes.Any(x => x.PostAllowed))
            {
                interfacesToImplement.Add(
                    Import(typeof(IPostableRepository<,>))
                        .MakeGenericInstanceType(resourceTypeInfo.InterfaceType,
                                                 this.clientTypeInfoDict[tt.PostReturnType]
                                                     .InterfaceType));
            }

            if (tt.DeleteAllowed || tt.MergedTypes.Any(x => x.DeleteAllowed))
            {
                interfacesToImplement.Add(
                    Import(typeof(IDeletableRepository<>))
                        .MakeGenericInstanceType(resourceTypeInfo.InterfaceType));
                if (tt.PrimaryId != null)
                {
                    interfacesToImplement.Add(
                        Import(typeof(IDeletableByIdRepository<>)).MakeGenericInstanceType(
                            resourceTypeInfo.PrimaryIdTypeReference));
                }
            }

            var repoInterface = CreateRepositoryType(resourceTypeInfo.CustomRepositoryInterface,
                                                     resourceTypeInfo,
                                                     MethodAttributes.Abstract |
                                                     MethodAttributes.HideBySig |
                                                     MethodAttributes.NewSlot |
                                                     MethodAttributes.Virtual |
                                                     MethodAttributes.Public,
                                                     "I{0}Repository",
                                                     TypeAttributes.Interface | TypeAttributes.Public |
                                                     TypeAttributes.Abstract,
                                                     false,
                                                     interfacesToImplement);

            var repoImplementation = CreateRepositoryType(new TypeDefinition(null, null, 0),
                                                          resourceTypeInfo,
                                                          MethodAttributes.NewSlot |
                                                          MethodAttributes.HideBySig
                                                          | MethodAttributes.Virtual |
                                                          MethodAttributes.Public,
                                                          "{0}Repository",
                                                          TypeAttributes.Public,
                                                          true,
                                                          repoInterface.WrapAsEnumerable());
        }


        private TypeDefinition CreateRepositoryType(TypeDefinition repoTypeDef,
                                                    TypeCodeGenInfo rti,
                                                    MethodAttributes methodAttributes,
                                                    string repoTypeNameFormat,
                                                    TypeAttributes typeAttributes,
                                                    bool isImplementation,
                                                    IEnumerable<TypeReference> interfacesToImplement = null)
        {
            var tt = (ResourceType)rti.StructuredType;

            repoTypeDef.Namespace = this.@namespace;
            repoTypeDef.Name = String.Format(repoTypeNameFormat, rti.StructuredType.Name);
            repoTypeDef.Attributes = typeAttributes;

            if (isImplementation)
                repoTypeDef.BaseType = rti.CustomRepositoryBaseTypeReference;

            repoTypeDef.Interfaces.AddRange(interfacesToImplement ?? Enumerable.Empty<TypeReference>());
            var baseTypeGenericDef = rti.CustomRepositoryBaseTypeDefinition;
            var baseTypeGenericArgs = rti.CustomRepositoryBaseTypeReference.GenericArguments.ToArray();

            foreach (var subType in tt.MergedTypes.Append(tt).Where(subType => subType.PostAllowed))
            {
                AddRepositoryFormPostMethod(methodAttributes,
                                            isImplementation,
                                            subType,
                                            repoTypeDef,
                                            baseTypeGenericDef,
                                            baseTypeGenericArgs);
            }

            // Constructor
            if (isImplementation)
            {
                var baseCtor = baseTypeGenericDef.GetConstructors().First();
                var baseCtorRef =
                    Import(baseCtor).MakeHostInstanceGeneric(baseTypeGenericArgs);

                var ctor = new MethodDefinition(".ctor",
                                                MethodAttributes.HideBySig
                                                | MethodAttributes.SpecialName
                                                | MethodAttributes.RTSpecialName
                                                | MethodAttributes.Public,
                                                VoidTypeRef);

                baseCtor.Parameters.Select(x => new ParameterDefinition(x.Name, x.Attributes, Import(x.ParameterType)))
                        .AddTo(ctor.Parameters);

                ctor.Body.MaxStackSize = 8;
                var ctorIlProcessor = ctor.Body.GetILProcessor();
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                foreach (var ctorParam in ctor.Parameters)
                    ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg, ctorParam));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtorRef));
                ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));
                repoTypeDef.Methods.Add(ctor);
            }

            if (!this.module.Types.Contains(repoTypeDef))
                this.module.Types.Add(repoTypeDef);

            return repoTypeDef;
        }


        private IEnumerable<TypeCodeGenInfo> GetAllUriBaseTypesExposedAsRepositories()
        {
            return this.clientTypeInfoDict.Values.Where(x => x.UriBaseType == x.InterfaceType
                                                             && x.StructuredType.Maybe().OfType<ResourceType>().Select(
                                                                 y => !y.IsChildResource && y.IsExposedAsRepository)
                                                                 .OrDefault());
        }


        private DefaultAssemblyResolver GetAssemblyResolver()
        {
            var assemblyResolver = new DefaultAssemblyResolver();

            // Fix for having path to bin directory when running ASP.NET app.
            var extraSearchDir = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).AbsolutePath);
            assemblyResolver.AddSearchDirectory(extraSearchDir);
            return assemblyResolver;
        }


        private StructuredProperty GetPropertyMapping(PropertyDefinition propertyDefinition,
                                                      TypeReference reflectedInterface = null)
        {
            reflectedInterface = reflectedInterface ?? propertyDefinition.DeclaringType;

            return this.clientTypeInfoDict
                       .Values
                       .First(x => x.InterfaceType == reflectedInterface)
                       .StructuredType.Properties.Cast<StructuredProperty>()
                       .First(x => x.Name == propertyDefinition.Name);
        }


        private TypeReference GetPropertyTypeReference(StructuredProperty prop)
        {
            if (prop.MaybeAs<ResourceProperty>().Select(x => x.ExposedAsRepository).OrDefault())
            {
                var propType = prop.PropertyType as EnumerableTypeSpec;
                if (propType == null)
                    throw new InvalidOperationException("Can only expose an enumerable type as repository.");
                var resourceInfo = this.clientTypeInfoDict[propType.ItemType];
                return resourceInfo.CustomRepositoryInterface;
            }

            TypeReference propTypeRef = GetTypeReference(prop.PropertyType);
            return propTypeRef;
        }


        private TypeReference GetProxyType(string proxyTypeName)
        {
            return Import(typeof(ClientBase).Assembly.GetTypes().First(x => x.Name == proxyTypeName));
        }


        private TypeReference GetTypeReference(TypeSpec type)
        {
            // TODO: Cache typeRef

            var sharedType = type as RuntimeTypeSpec;
            var transformedType = type as StructuredType;
            var enumType = type as EnumTypeSpec;
            TypeReference typeRef = null;

            if (type.GetCustomClientLibraryType() != null)
                typeRef = Import(type.GetCustomClientLibraryType());
            else if (enumType != null)
            {
                if (!this.enumClientTypeDict.TryGetValue(enumType, out typeRef))
                {
                    throw new InvalidOperationException(
                        String.Format(
                            "Generated property has a reference to {0}, but has probably not been included in SourceTypes.",
                            enumType.Type.FullName));
                }
            }
            else if (transformedType != null)
                typeRef = this.clientTypeInfoDict[transformedType].InterfaceType;
            else if (sharedType != null)
            {
                typeRef = Import(sharedType.Type.IsGenericType
                    ? sharedType.Type.GetGenericTypeDefinition()
                    : sharedType.Type);

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


        private TypeReference Import(TypeReference typeReference)
        {
            if (typeReference is TypeDefinition && typeReference.Module == this.module)
                return typeReference;
            return this.module.Import(typeReference);
        }


        private MethodReference Import(MethodReference methodReference)
        {
            if (methodReference is MethodDefinition && methodReference.Module == this.module)
                return methodReference;

            var importedMethodReference = this.module.Import(methodReference);

            // NOTE: Workaround to preserve the parameter names since Cecil's MetadataImporter.ImportMethod strips them away. @asbjornu
            //       See <https://github.com/jbevain/cecil/pull/184>.
            importedMethodReference
                .Parameters
                .Zip(methodReference.Parameters, (importedParameter, sourceParameter) =>
                {
                    importedParameter.Attributes = sourceParameter.Attributes;
                    return importedParameter.Name = sourceParameter.Name;
                })
                .ToArray();

            return importedMethodReference;
        }


        private MethodReference Import(MethodBase methodBase)
        {
            if (PomonaClientEmbeddingEnabled && methodBase.DeclaringType.Assembly == commonAssembly)
            {
                return
                    this.module.GetType(methodBase.DeclaringType.FullName)
                        .Methods.First(x => x.Name == methodBase.Name &&
                                            ParametersAreEqual(methodBase, x));
            }
            return this.module.Import(methodBase);
        }


        private TypeReference Import(Type type)
        {
            return this.typeReferenceCache.GetOrCreate(type, () =>
            {
                TypeReference typeReference;

                if (PomonaClientEmbeddingEnabled
                    && type.Assembly == typeof(IPomonaClient).Assembly)
                {
                    // clientBaseTypeRef = this.module.GetType("Pomona.Client.ClientBase`1");
                    typeReference = this.module.GetType(type.FullName);
                }
                else
                {
                    if (
                        !this.allowedReferencedAssemblies.Contains(
                            type.Assembly))
                    {
                        throw new InvalidOperationException(
                            "Generated dll contains a reference to "
                            + type.AssemblyQualifiedName
                            + ", which is not allowed for references.");
                    }
                    typeReference = this.module.Import(type);
                }

                if (typeReference == null)
                {
                    throw new InvalidOperationException(
                        "Did not expect to get null when resolving type.");
                }

                return typeReference;
            });
        }


        private AssemblyDefinition LoadAssemblyHavingType<T>()
        {
            var readerParameters = new ReaderParameters { AssemblyResolver = GetAssemblyResolver() };
            return AssemblyDefinition.ReadAssembly(typeof(T).Assembly.Location, readerParameters);
        }


        private AssemblyDefinition LoadPomonaCommonAssembly()
        {
            return LoadAssemblyHavingType<ResourceBase>();
        }


        private static bool ParametersAreEqual(MethodBase methodBase, MethodDefinition methodDefinition)
        {
            return methodDefinition.Parameters.Select(y => y.ParameterType.FullName)
                                   .SequenceEqual(methodBase.GetParameters().Select(y => y.ParameterType.FullName));
        }

        #region Nested type: PatchFormProxyBuilder

        private class PatchFormProxyBuilder : WrappedPropertyProxyBuilder
        {
            private readonly ClientLibGenerator owner;


            public PatchFormProxyBuilder(ClientLibGenerator owner, bool isPublic = true)
                : base(owner.module,
                       owner.GetProxyType("PostResourceBase"),
                       owner.Import(typeof(PropertyWrapper<,>)).Resolve(),
                       isPublic,
                       proxyNamespace : owner.@namespace)
            {
                this.owner = owner;
                ProxyNameFormat = "{0}PatchForm";
            }


            protected override void OnGeneratePropertyMethods(PropertyDefinition targetProp,
                                                              PropertyDefinition proxyProp,
                                                              TypeReference proxyBaseType,
                                                              TypeReference proxyTargetType,
                                                              TypeReference rootProxyTargetType)
            {
                var propertyMapping = this.owner.GetPropertyMapping(targetProp);
                base.OnGeneratePropertyMethods(targetProp,
                                               proxyProp,
                                               proxyBaseType,
                                               proxyTargetType,
                                               rootProxyTargetType);
                if ((propertyMapping.AccessMode & (HttpMethod.Patch | HttpMethod.Put | HttpMethod.Post)) != 0)
                    return;

                var invalidOperationStrCtor =
                    typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                var invalidOperationStrCtorRef = this.owner.Import(invalidOperationStrCtor);

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
                    ilproc.Append(Instruction.Create(OpCodes.Ldstr,
                                                     "Illegal to update remote property " + propertyMapping.Name));
                    ilproc.Append(Instruction.Create(OpCodes.Newobj, invalidOperationStrCtorRef));
                    ilproc.Append(Instruction.Create(OpCodes.Throw));
                }
            }
        }

        #endregion

        #region Nested type: PostFormProxyBuilder

        private class PostFormProxyBuilder : WrappedPropertyProxyBuilder
        {
            private readonly ClientLibGenerator owner;


            public PostFormProxyBuilder(ClientLibGenerator owner, bool isPublic = true)
                : base(owner.module,
                       owner.GetProxyType("PostResourceBase"),
                       owner.Import(typeof(PropertyWrapper<,>)).Resolve(),
                       isPublic,
                       proxyNamespace : owner.@namespace)
            {
                this.owner = owner;
                ProxyNameFormat = "{0}Form";
            }


            protected override void OnGeneratePropertyMethods(PropertyDefinition targetProp,
                                                              PropertyDefinition proxyProp,
                                                              TypeReference proxyBaseType,
                                                              TypeReference proxyTargetType,
                                                              TypeReference rootProxyTargetType)
            {
                var propertyMapping = this.owner.GetPropertyMapping(targetProp, rootProxyTargetType);
                var mergedProperties = propertyMapping
                    .WrapAsEnumerable()
                    .Concat(propertyMapping.ReflectedType.SubTypes.Select(
                        x => x.Properties.First(y => y.Name == propertyMapping.Name)));

                if (mergedProperties.Any(x => x.AccessMode.HasFlag(HttpMethod.Post)))
                {
                    base.OnGeneratePropertyMethods(targetProp,
                                                   proxyProp,
                                                   proxyBaseType,
                                                   proxyTargetType,
                                                   rootProxyTargetType);
                }
                else
                {
                    var invalidOperationStrCtor =
                        typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                    var invalidOperationStrCtorRef = this.owner.Import(invalidOperationStrCtor);

                    var getMethod = proxyProp.GetMethod;
                    var setMethod = proxyProp.SetMethod;

                    foreach (var ilproc in new[] { getMethod, setMethod }
                        .Select(method => method.Body.GetILProcessor()))
                    {
                        ilproc.Append(Instruction.Create(OpCodes.Ldstr,
                                                         propertyMapping.Name + " can't be set during initialization."));
                        ilproc.Append(Instruction.Create(OpCodes.Newobj, invalidOperationStrCtorRef));
                        ilproc.Append(Instruction.Create(OpCodes.Throw));
                    }

                    var explicitPropNamePrefix = targetProp.DeclaringType + ".";
                    proxyProp.Name = explicitPropNamePrefix + targetProp.Name;
                    getMethod.Name = explicitPropNamePrefix + getMethod.Name;
                    setMethod.Name = explicitPropNamePrefix + setMethod.Name;
                    const MethodAttributes exlicitMethodAttrs = MethodAttributes.Private
                                                                | MethodAttributes.Final
                                                                | MethodAttributes.HideBySig
                                                                | MethodAttributes.SpecialName
                                                                | MethodAttributes.NewSlot
                                                                | MethodAttributes.Virtual;
                    getMethod.Attributes = exlicitMethodAttrs;
                    setMethod.Attributes = exlicitMethodAttrs;
                    getMethod.Overrides.Add(this.owner.Import(targetProp.GetMethod));

                    if (targetProp.SetMethod != null)
                        setMethod.Overrides.Add(this.owner.Import(targetProp.SetMethod));
                }
            }
        }

        #endregion

        #region Nested type: TypeCodeGenInfo

        private class TypeCodeGenInfo
        {
            private readonly Lazy<Type> customRepositoryBaseType;
            private readonly Lazy<TypeDefinition> customRepositoryBaseTypeDefinition;
            private readonly Lazy<TypeReference> customRepositoryBaseTypeReference;
            private readonly TypeDefinition customRepositoryInterface;
            private readonly TypeDefinition interfaceType;
            private readonly ClientLibGenerator parent;
            private readonly Lazy<TypeReference> postReturnTypeReference;
            private readonly StructuredType structuredType;


            public TypeCodeGenInfo(ClientLibGenerator parent, StructuredType structuredType)
            {
                var resourceType = structuredType as ResourceType;
                if (resourceType != null && resourceType.IsUriBaseType)
                {
                    if ((!resourceType.IsChildResource && resourceType.IsExposedAsRepository)
                        || (resourceType.ParentToChildProperty != null
                            && resourceType.ParentToChildProperty.ExposedAsRepository))
                    {
                        this.customRepositoryInterface = new TypeDefinition(parent.@namespace,
                                                                            String.Format("I{0}Repository",
                                                                                          structuredType.Name),
                                                                            TypeAttributes.Interface
                                                                            | TypeAttributes.Public
                                                                            | TypeAttributes.Abstract);
                        parent.module.Types.Add(this.customRepositoryInterface);
                    }
                }

                if (parent == null)
                    throw new ArgumentNullException("parent");

                this.parent = parent;
                this.structuredType = structuredType;

                this.postReturnTypeReference = new Lazy<TypeReference>(() =>
                {
                    if (resourceType == null || resourceType.PostReturnType == null)
                        return InterfaceType;
                    return parent.clientTypeInfoDict[resourceType.PostReturnType].InterfaceType;
                });

                this.customRepositoryBaseType = new Lazy<Type>(() =>
                {
                    if (resourceType != null)
                    {
                        if (resourceType.IsUriBaseType)
                        {
                            if (!resourceType.IsChildResource && resourceType.IsExposedAsRepository)
                                return typeof(ClientRepository<,,>);
                            if (resourceType.ParentToChildProperty != null
                                && resourceType.ParentToChildProperty.ExposedAsRepository)
                                return typeof(ChildResourceRepository<,,>);
                        }
                    }
                    return null;
                });

                this.customRepositoryBaseTypeDefinition = new Lazy<TypeDefinition>(() =>
                {
                    if (this.customRepositoryBaseType.Value == null)
                        return null;

                    var typeRef = parent.Import(this.customRepositoryBaseType.Value);
                    return typeRef as TypeDefinition ?? typeRef.Resolve();
                });

                this.customRepositoryBaseTypeReference = new Lazy<TypeReference>(() =>
                {
                    if (this.customRepositoryBaseType.Value == null)
                        return null;

                    return parent
                        .Import(this.customRepositoryBaseType.Value)
                        .MakeGenericInstanceType(InterfaceType,
                                                 PostReturnTypeReference,
                                                 PrimaryIdTypeReference);
                });

                // Add I in front of name if resource is class type, do not for interfaces to avoid IIWhatever name.
                var name = "I" + structuredType.Name;

                this.interfaceType = new TypeDefinition(parent.@namespace,
                                                        name,
                                                        TypeAttributes.Interface
                                                        | TypeAttributes.Public
                                                        | TypeAttributes.Abstract);
            }


            public TypeDefinition BaseType { get; set; }

            public TypeDefinition CustomRepositoryBaseTypeDefinition
            {
                get { return this.customRepositoryBaseTypeDefinition.Value; }
            }

            public GenericInstanceType CustomRepositoryBaseTypeReference
            {
                get { return (GenericInstanceType)this.customRepositoryBaseTypeReference.Value; }
            }

            public TypeDefinition CustomRepositoryInterface
            {
                get { return this.customRepositoryInterface; }
            }

            public MethodDefinition EmptyPocoCtor { get; set; }

            public TypeDefinition InterfaceType
            {
                get { return this.interfaceType; }
            }

            public TypeDefinition LazyProxyType { get; set; }
            public TypeDefinition PatchFormType { get; set; }
            public TypeDefinition PocoType { get; set; }
            public TypeDefinition PostFormType { get; set; }

            public TypeReference PostReturnTypeReference
            {
                get { return this.postReturnTypeReference.Value; }
            }

            public TypeReference PrimaryIdTypeReference
            {
                get
                {
                    return
                        this.parent.Import(this.structuredType.PrimaryId != null
                            ? this.structuredType.PrimaryId.PropertyType
                            : typeof(object));
                }
            }

            public StructuredType StructuredType
            {
                get { return this.structuredType; }
            }

            public TypeDefinition UriBaseType { get; set; }
        }

        #endregion
    }
}