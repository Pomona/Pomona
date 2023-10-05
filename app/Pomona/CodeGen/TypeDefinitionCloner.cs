#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using NuGet;

using Pomona.Common;

namespace Pomona.CodeGen
{
    public class TypeDefinitionCloner
    {
        // Copies a self-contained class or several classes, with templating possibilities

        private readonly ModuleDefinition destinationModule;

        private readonly Dictionary<FieldReference, FieldReference> fieldMap =
            new Dictionary<FieldReference, FieldReference>();

        private readonly Dictionary<MethodReference, MethodReference> methodMap =
            new Dictionary<MethodReference, MethodReference>();

        private readonly Dictionary<PropertyReference, PropertyReference> propertyMap =
            new Dictionary<PropertyReference, PropertyReference>();

        private readonly Dictionary<TypeReference, TypeReference> typeMap =
            new Dictionary<TypeReference, TypeReference>();


        public TypeDefinitionCloner(ModuleDefinition destinationModule)
        {
            this.destinationModule = destinationModule;
        }


        public TypeDefinition Clone(TypeDefinition sourceType)
        {
            if (sourceType.HasGenericParameters)
                throw new NotSupportedException("Does not yet support cloning types having generic parameters");
            if (sourceType.HasNestedTypes)
                throw new NotSupportedException("Does not support cloning types having nested types.");

            var destType = new TypeDefinition(
                sourceType.Namespace,
                sourceType.Name,
                sourceType.Attributes);

            this.destinationModule.Types.Add(destType);

            this.typeMap[sourceType] = destType;

            if (sourceType.BaseType != null)
                destType.BaseType = Import(sourceType.BaseType);

            foreach (var sourceInteface in sourceType.Interfaces)
                destType.Interfaces.Add(Import(sourceInteface));

            foreach (var sourceField in sourceType.Fields)
            {
                var destField = new FieldDefinition(sourceField.Name,
                                                    sourceField.Attributes,
                                                    Import(sourceField.FieldType));
                destType.Fields.Add(destField);
                this.fieldMap[sourceField] = destField;

                CopyAttributes(sourceField, destField);
                if (sourceField.HasConstant)
                    throw new NotImplementedException("Can't copy constant field");
            }
            var methodParamMap = new Dictionary<ParameterDefinition, ParameterDefinition>();

            foreach (var sourceMethod in sourceType.Methods)
                CopyMethodDeclaration(sourceMethod, destType, methodParamMap);

            foreach (var sourceMethod in sourceType.Methods)
                CopyMethodBody(sourceMethod, methodParamMap);
            foreach (var sourceProp in sourceType.Properties)
                CopyProperty(sourceProp, destType);

            return destType;
        }


        private CustomAttribute Clone(CustomAttribute sourceAttribute)
        {
            return new CustomAttribute(Import(sourceAttribute.Constructor), sourceAttribute.GetBlob());
        }


        private void CopyAttributes(ICustomAttributeProvider source, ICustomAttributeProvider destination)
        {
            destination.CustomAttributes.AddRange(source.CustomAttributes.Select(Clone));
        }


        private void CopyExceptionHandlers(ExceptionHandler sourceHandler, Dictionary<Instruction, Instruction> instMap, MethodBody destBody)
        {
            var destHandler = new ExceptionHandler(sourceHandler.HandlerType)
            {
                CatchType = sourceHandler.CatchType != null ? Import(sourceHandler.CatchType) : null,
                FilterStart = MapInstructionSafe(instMap, sourceHandler.FilterStart),
                HandlerStart = MapInstructionSafe(instMap, sourceHandler.HandlerStart),
                HandlerEnd = MapInstructionSafe(instMap, sourceHandler.HandlerEnd),
                TryStart = MapInstructionSafe(instMap, sourceHandler.TryStart),
                TryEnd = MapInstructionSafe(instMap, sourceHandler.TryEnd)
            };
            destBody.ExceptionHandlers.Add(destHandler);
        }


        private void CopyMethodBody(MethodDefinition sourceMethod,
                                    Dictionary<ParameterDefinition, ParameterDefinition> methodParamMap)
        {
            var destMethod = (MethodDefinition)this.methodMap[sourceMethod];
            var varMap = new Dictionary<VariableDefinition, VariableDefinition>();
            var postActions = new List<Action>();
            var nopInstruction = Instruction.Create(OpCodes.Nop);
            var instMap = new Dictionary<Instruction, Instruction>();

            if (sourceMethod.HasBody)
            {
                var destBody = destMethod.Body;
                var sourceBody = sourceMethod.Body;

                foreach (var sourceVar in sourceBody.Variables)
                {
                    var destVar = new VariableDefinition(sourceVar.Name,
                                                         Import(sourceVar.VariableType));
                    destBody.Variables.Add(destVar);
                    varMap[sourceVar] = destVar;
                }

                destBody.InitLocals = sourceBody.InitLocals;
                destBody.MaxStackSize = sourceBody.MaxStackSize;
                var destIl = destBody.GetILProcessor();
                foreach (var si in sourceBody.Instructions)
                {
                    Instruction di;
                    var operand = si.Operand;
                    if (operand == null)
                        di = destIl.Create(si.OpCode);
                    else if (operand is MethodReference)
                        di = destIl.Create(si.OpCode, (MethodReference)Import((MethodReference)operand));
                    else if (operand is FieldReference)
                        di = destIl.Create(si.OpCode, (FieldReference)Import((FieldReference)operand));
                    else if (operand is ParameterDefinition)
                        di = destIl.Create(si.OpCode, methodParamMap[(ParameterDefinition)operand]);
                    else if (operand is VariableDefinition)
                        di = destIl.Create(si.OpCode, varMap[(VariableDefinition)operand]);
                    else if (operand is Instruction)
                    {
                        var siInstOperand = (Instruction)operand;
                        di = destIl.Create(si.OpCode, nopInstruction);
                        postActions.Add(() => di.Operand = instMap[siInstOperand]);
                    }
                    else if (operand is Instruction[])
                    {
                        var siInstOperand = (Instruction[])operand;
                        di = destIl.Create(si.OpCode, new[] { nopInstruction });
                        postActions.Add(() => di.Operand = siInstOperand.Select(y => instMap[y]).ToArray());
                    }
                    else if (operand is byte)
                        di = destIl.Create(si.OpCode, (byte)operand);
                    else if (operand is double)
                        di = destIl.Create(si.OpCode, (double)operand);
                    else if (operand is float)
                        di = destIl.Create(si.OpCode, (float)operand);
                    else if (operand is int)
                        di = destIl.Create(si.OpCode, (int)operand);
                    else if (operand is long)
                        di = destIl.Create(si.OpCode, (long)operand);
                    else if (operand is sbyte)
                        di = destIl.Create(si.OpCode, (sbyte)operand);
                    else if (operand is string)
                        di = destIl.Create(si.OpCode, (string)operand);
                    else if (operand is TypeReference)
                        di = destIl.Create(si.OpCode, Import((TypeReference)operand));
                    else
                        throw new NotImplementedException("Does not support copying opcode " + si.OpCode);
                    instMap[si] = di;
                    destIl.Append(di);
                }

                foreach (var pa in postActions)
                    pa();

                foreach (var sourceHandler in sourceBody.ExceptionHandlers)
                    CopyExceptionHandlers(sourceHandler, instMap, destBody);
            }

            CopyAttributes(sourceMethod, destMethod);
        }


        private void CopyMethodDeclaration(MethodDefinition sourceMethod,
                                           TypeDefinition destType,
                                           Dictionary<ParameterDefinition, ParameterDefinition> methodParamMap)
        {
            var destMethod = new MethodDefinition(
                sourceMethod.Name,
                sourceMethod.Attributes,
                Import(sourceMethod.ReturnType));
            destType.Methods.Add(destMethod);
            this.methodMap[sourceMethod] = destMethod;

            foreach (var sourceParam in sourceMethod.Parameters)
            {
                var destParam = new ParameterDefinition(sourceParam.Name,
                                                        sourceParam.Attributes,
                                                        Import(sourceParam.ParameterType));
                destMethod.Parameters.Add(destParam);
                CopyAttributes(sourceParam, destParam);
                methodParamMap[sourceParam] = destParam;
            }
        }


        private void CopyProperty(PropertyDefinition sourceProp, TypeDefinition destType)
        {
            var destProp = new PropertyDefinition(sourceProp.Name,
                                                  sourceProp.Attributes,
                                                  Import(sourceProp.PropertyType));
            destType.Properties.Add(destProp);
            this.propertyMap[sourceProp] = destProp;

            if (sourceProp.GetMethod != null)
                destProp.GetMethod = (MethodDefinition)Import(sourceProp.GetMethod);
            if (sourceProp.SetMethod != null)
                destProp.SetMethod = (MethodDefinition)Import(sourceProp.SetMethod);

            CopyAttributes(sourceProp, destProp);
        }


        private FieldReference Import(FieldReference fieldReference)
        {
            return this.fieldMap.GetOrCreate(fieldReference, () => this.destinationModule.Import(fieldReference));
        }


        private MethodReference Import(MethodReference methodReference)
        {
            return this.methodMap.GetOrCreate(methodReference,
                                              () =>
                                              {
                                                  GenericInstanceMethod sourceInstance =
                                                      methodReference as GenericInstanceMethod;
                                                  if (sourceInstance != null)
                                                  {
                                                      var destInstance =
                                                          new GenericInstanceMethod(Import(sourceInstance.ElementMethod));
                                                      destInstance.GenericArguments.AddRange(
                                                          sourceInstance.GenericArguments.Select(Import));
                                                      return destInstance;
                                                  }
                                                  var imported = this.destinationModule.Import(methodReference);
                                                  if (imported.DeclaringType != null)
                                                      imported.DeclaringType = Import(methodReference.DeclaringType);
                                                  return imported;
                                              });
        }


        private TypeReference Import(TypeReference typeReference)
        {
            return this.typeMap.GetOrCreate(typeReference,
                                            () =>
                                            {
                                                if (typeReference.IsByReference)
                                                    return Import(typeReference.GetElementType()).MakeByReferenceType();

                                                var sourceInstance = typeReference as GenericInstanceType;
                                                if (sourceInstance != null)
                                                {
                                                    var destInstance =
                                                        new GenericInstanceType(Import(sourceInstance.ElementType));
                                                    destInstance.GenericArguments.AddRange(
                                                        sourceInstance.GenericArguments.Select(Import));
                                                    return destInstance;
                                                }
                                                return this.destinationModule.Import(typeReference);
                                            });
        }


        private static Instruction MapInstructionSafe(Dictionary<Instruction, Instruction> instMap, Instruction sourceFilterStart)
        {
            return sourceFilterStart != null ? instMap[sourceFilterStart] : null;
        }
    }
}

