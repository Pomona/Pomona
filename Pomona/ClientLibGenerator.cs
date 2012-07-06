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

using Pomona.Client;

namespace Pomona
{
    public class ClientLibGenerator
    {
        private ClassMappingFactory classMappingFactory;
        private ModuleDefinition module;
        private Dictionary<IMappedType, TypeReference> toClientTypeDict;


        public ClientLibGenerator(ClassMappingFactory classMappingFactory)
        {
            if (classMappingFactory == null)
                throw new ArgumentNullException("classMappingFactory");
            this.classMappingFactory = classMappingFactory;
        }


        public void CreateClientDll(Stream stream)
        {
            var types = classMappingFactory.TransformedTypes.ToList();

            var assembly = AssemblyDefinition.ReadAssembly(typeof(ResourceBase).Assembly.Location);
            //var assembly =
            //    AssemblyDefinition.CreateAssembly(
            //        new AssemblyNameDefinition("Critter", new Version(1, 0, 0, 134)), "Critter", ModuleKind.Dll);

            this.module = assembly.MainModule;

            this.toClientTypeDict = new Dictionary<IMappedType, TypeReference>();

            foreach (var t in types)
            {
                //var typeDef = new TypeDefinition(
                //    "CritterClient", "I" + t.Name, TypeAttributes.Interface | TypeAttributes.Public);
                var typeDef = new TypeDefinition(
                    "CritterClient", t.Name, TypeAttributes.Public);
                this.toClientTypeDict[t] = typeDef;

                // Empty public constructor
                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                    | MethodAttributes.Public,
                    this.module.Import(typeof(void)));

                ctor.Body.MaxStackSize = 8;
                ctor.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));

                typeDef.Methods.Add(ctor);

                this.module.Types.Add(typeDef);
            }

            var msObjectTypeRef = this.module.Import(typeof(object));

            foreach (var kvp in this.toClientTypeDict)
            {
                var type = (TransformedType)kvp.Key;
                var typeDef = (TypeDefinition)kvp.Value;
                var classMapping = type;

                if (type.BaseType != null && this.toClientTypeDict.ContainsKey(type.BaseType))
                    typeDef.BaseType = this.toClientTypeDict[type.BaseType];
                else
                    typeDef.BaseType = msObjectTypeRef;

                foreach (var prop in classMapping.Properties.Where(x => x.DeclaringType == classMapping))
                {
                    var propTypeRef = GetTypeReference(prop.PropertyType);

                    var propDef = new PropertyDefinition(prop.Name, PropertyAttributes.None, propTypeRef);

                    // For interface getters and setters
                    //var getMethod = new MethodDefinition(
                    //    "get_" + prop.Name, MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Public, propTypeRef);

                    var propField =
                        new FieldDefinition(
                            "_" + prop.Name.Substring(0, 1).ToLower() + prop.Name.Substring(1),
                            FieldAttributes.Private,
                            propTypeRef);

                    typeDef.Fields.Add(propField);

                    var getMethod = new MethodDefinition(
                        "get_" + prop.Name,
                        MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.HideBySig
                        | MethodAttributes.Virtual | MethodAttributes.Public,
                        propTypeRef);

                    // Create get method

                    getMethod.Body.MaxStackSize = 1;
                    var getIlProcessor = getMethod.Body.GetILProcessor();
                    getIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    getIlProcessor.Append(Instruction.Create(OpCodes.Ldfld, propField));
                    getIlProcessor.Append(Instruction.Create(OpCodes.Ret));

                    typeDef.Methods.Add(getMethod);

                    // Create set property

                    var setMethod = new MethodDefinition(
                        "set_" + prop.Name,
                        MethodAttributes.NewSlot | MethodAttributes.Public | MethodAttributes.HideBySig |
                        MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        this.module.Import(typeof(void)));

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


        private TypeReference GetTypeReference(IMappedType type)
        {
            // TODO: Cache typeRef

            TypeReference typeRef = null;

            var sharedType = type as SharedType;
            if (sharedType != null)
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

            var transformedType = type as TransformedType;
            if (transformedType != null)
                typeRef = this.toClientTypeDict[transformedType];

            if (typeRef == null)
                throw new InvalidOperationException("Unable to get TypeReference for IMappedType");

            return typeRef;
        }
    }
}