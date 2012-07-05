using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Pomona.Client;

namespace Pomona
{
    public class ClientLibGenerator
    {
        private ClassMappingFactory classMappingFactory;
        private Dictionary<IMappedType, TypeReference> toClientTypeDict;
        private ModuleDefinition module;


        public ClientLibGenerator()
        {
            classMappingFactory = new ClassMappingFactory();
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
                    {
                        typeRefInstance.GenericArguments.Add(GetTypeReference(genericArgument));
                    }

                    typeRef = typeRefInstance;
                }
            }

            var transformedType = type as TransformedType;
            if (transformedType != null)
            {
                typeRef = toClientTypeDict[transformedType];
            }

            if (typeRef == null)
                throw new InvalidOperationException("Unable to get TypeReference for IMappedType");

            return typeRef;
        }

        public void CreateClientDll(IEnumerable<TransformedType> typesToBeIncluded, Stream stream)
        {

            var types = typesToBeIncluded.ToList();

            var assembly = AssemblyDefinition.ReadAssembly(typeof(ResourceBase).Assembly.Location);
            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            module = assembly.MainModule;

            toClientTypeDict = new Dictionary<IMappedType, TypeReference>();

            foreach (var t in types)
            {
                //var typeDef = new TypeDefinition(
                //    "CritterClient", "I" + t.Name, TypeAttributes.Interface | TypeAttributes.Public);
                var typeDef = new TypeDefinition(
                    "CritterClient", t.Name, TypeAttributes.Public);
                toClientTypeDict[t] = typeDef;

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    module.Import(typeof(void)));

                ctor.Body.MaxStackSize = 8;
                ctor.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));

                typeDef.Methods.Add(ctor);

                module.Types.Add(typeDef);
            }

            var msObjectTypeRef = module.Import(typeof(object));

            foreach (var kvp in toClientTypeDict)
            {
                var type = (TransformedType)kvp.Key;
                var typeDef = (TypeDefinition)kvp.Value;
                var classMapping = type;

                if (type.BaseType != null && toClientTypeDict.ContainsKey(type.BaseType))
                {
                    typeDef.BaseType = toClientTypeDict[type.BaseType];
                }
                else
                {
                    typeDef.BaseType = msObjectTypeRef;
                }

                foreach (var prop in classMapping.Properties.Where(x => x.DeclaringType == classMapping))
                {
                    TypeReference propTypeRef = GetTypeReference(prop.PropertyType);

                    var propDef = new PropertyDefinition(prop.Name, PropertyAttributes.None, propTypeRef);

                    // For interface getters and setters
                    //var getMethod = new MethodDefinition(
                    //    "get_" + prop.Name, MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public, propTypeRef);

                    var propField = new FieldDefinition("_" + prop.Name.Substring(0, 1).ToLower() + prop.Name.Substring(1), FieldAttributes.Private, propTypeRef);

                    typeDef.Fields.Add(propField);

                    var getMethod = new MethodDefinition(
                        "get_" + prop.Name, MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, propTypeRef);


                    // Create get method

                    getMethod.Body.MaxStackSize = 1;
                    var getIlProcessor = getMethod.Body.GetILProcessor();
                    getIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    getIlProcessor.Append(Instruction.Create(OpCodes.Ldfld, propField));
                    getIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                    typeDef.Methods.Add(getMethod);

                    
                    // Create set property

                    var setMethod = new MethodDefinition("set_" + prop.Name,
                                                          MethodAttributes.NewSlot | MethodAttributes.Public | MethodAttributes.HideBySig |
                                                         MethodAttributes.Virtual | MethodAttributes.SpecialName,
                                                         module.Import(typeof(void)));

                    // Create set method body

                    setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propTypeRef));

                    setMethod.Body.MaxStackSize = 8;

                    var setIlProcessor = setMethod.Body.GetILProcessor();
                    setIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    setIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                    setIlProcessor.Append(Instruction.Create(OpCodes.Stfld, propField));
                    setIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                    typeDef.Methods.Add(setMethod);



                    propDef.GetMethod = getMethod;
                    propDef.SetMethod = setMethod;


                    typeDef.Properties.Add(propDef);
                }

            }


            // Copy types from running assembly



            var memstream = new MemoryStream();
            assembly.Write(memstream);


            var array = memstream.ToArray();

            stream.Write(array, 0, array.Length);

            //assembly.Write(stream);
        }

    }
}