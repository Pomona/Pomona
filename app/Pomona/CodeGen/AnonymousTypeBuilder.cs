#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Pomona.Common;

using CustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Pomona.CodeGen
{
    /// <summary>
    /// A class that builds classes that looks like C# anonymous types runtime.
    /// This is a huge mess, but implemented to make .Select(x => new { ... })
    /// queries dynamically.
    /// 
    /// Write only code. Seriously, don't read, your eyes will burn.
    /// </summary>
    public class AnonymousTypeBuilder
    {
        private static int anonTypeNumber = 100;

        private static readonly ConcurrentDictionary<string, Type> anonTypeCache =
            new ConcurrentDictionary<string, Type>();

        private readonly IList<Property> properties;
        private TypeDefinition definition;
        private GenericInstanceType ilFieldDeclaringType;
        private ModuleDefinition module;


        public AnonymousTypeBuilder(IEnumerable<string> propNames)
        {
            this.properties = propNames.Select((x, i) => new Property { Index = i, Name = x }).ToList();
        }


        private int PropCount
        {
            get { return this.properties.Count; }
        }


        public TypeDefinition BuildAnonymousType()
        {
            var assemblyName = "dyn" + Guid.NewGuid().ToString();
            var dynamicAssembly =
                AssemblyDefinition.CreateAssembly(
                    new AssemblyNameDefinition(assemblyName, new Version(1, 0)), assemblyName, ModuleKind.Dll);

            this.module = dynamicAssembly.MainModule;
            this.module.Architecture = TargetArchitecture.I386;
            this.module.Attributes = ModuleAttributes.ILOnly;

            AddAssemblyAttributes();

            this.definition = new TypeDefinition(
                "",
                $"<>f__AnonymousType{AllocateUniqueAnonymousClassNumber()}`{PropCount}",
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, this.module.TypeSystem.Object);

            AddGenericParams();
            this.module.Types.Add(this.definition);

            foreach (var prop in this.properties)
                AddProperty(prop);

            AddConstructor();
            AddGetHashCode();
            AddEquals();
            AddToString();

            AddDebuggerDisplayAttribute();
            AddCompilerGeneratedAttribute();
            AddDebuggerBrowseableAttributesToFields();

            return this.definition;
        }


        public static object CreateAnonymousObject<T>(IEnumerable<T> source,
                                                      Func<T, string> nameSelector,
                                                      Func<T, object> valueSelector,
                                                      Func<T, Type> typeSelector)
        {
            var sourceList = source as IList<T> ?? source.ToList();
            var anonType = GetAnonymousType(sourceList.Select(nameSelector));
            var typeArguments = sourceList.Select(typeSelector).ToArray();
            var anonTypeInstance = anonType.MakeGenericType(typeArguments);
            var constructorInfo = anonTypeInstance.GetConstructor(typeArguments);

            if (constructorInfo == null)
                throw new InvalidOperationException("Did not find expected constructor on anonymous type.");

            return constructorInfo.Invoke(sourceList.Select(valueSelector).ToArray());
        }


        public static Expression CreateNewExpression(IEnumerable<KeyValuePair<string, Expression>> map,
                                                     out Type anonTypeInstance)
        {
            var kvpList = map.ToList();

            var propNames = kvpList.Select(x => x.Key);

            var anonType = GetAnonymousType(propNames);

            var typeArguments = kvpList.Select(x => x.Value.Type).ToArray();
            anonTypeInstance = anonType.MakeGenericType(typeArguments);
            var constructorInfo = anonTypeInstance.GetConstructor(typeArguments);

            if (constructorInfo == null)
                throw new InvalidOperationException("Did not find expected constructor on anonymous type.");

            var anonTypeLocal = anonTypeInstance;
            return Expression.New(
                constructorInfo, kvpList.Select(x => x.Value), kvpList.Select(x => anonTypeLocal.GetProperty(x.Key)));
        }


        public MethodReference MakeHostInstanceGeneric(MethodReference self, params TypeReference[] arguments)
        {
            /*  var returnType = self.ReturnType;
              if (returnType is GenericInstanceType)
                  returnType = ReplaceGenericArguments((GenericInstanceType)self.ReturnType,  self.DeclaringType, arguments);
              */
            var reference = new MethodReference(
                self.Name, self.ReturnType, self.DeclaringType.MakeGenericInstanceType(arguments))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return this.module.Import(reference, this.ilFieldDeclaringType);
        }


        public static void ScanAssemblyForExistingAnonymousTypes(Assembly assembly)
        {
            var anonTypes = assembly.GetTypes().Where(x => x.IsAnonymous()).ToList();

            foreach (var anonType in anonTypes)
            {
                var typeKey = GetAnonTypeKey(anonType.GetConstructors().Single().GetParameters().Select(x => x.Name));
                anonTypeCache[typeKey] = anonType;
            }
        }


        private void AddAssemblyAttributes()
        {
            AddRuntimeCompabilityAttributeToAssembly();
            AddCompilationRelaxationsAttributeToAssembly();
        }


        private void AddCompilationRelaxationsAttributeToAssembly()
        {
            var attrType = this.module.Import(typeof(CompilationRelaxationsAttribute));
            var methodDefinition = this.module.Import(attrType.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 1));
            var attr =
                new CustomAttribute(methodDefinition);

            attr.ConstructorArguments.Add(new CustomAttributeArgument(this.module.TypeSystem.Int32, 8));
            this.module.Assembly.CustomAttributes.Add(attr);
        }


        private void AddCompilerGeneratedAttribute()
        {
            var attrType = this.module.Import(typeof(CompilerGeneratedAttribute));
            var methodDefinition = this.module.Import(attrType.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var attr =
                new CustomAttribute(methodDefinition);
            this.definition.CustomAttributes.Add(attr);
        }


        private void AddConstructor()
        {
            var baseCtor = this.module.Import(this.module.TypeSystem.Object.Resolve().GetConstructors().First());
            const MethodAttributes methodAttributes =
                MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var ctor = new MethodDefinition(
                ".ctor", methodAttributes, this.module.TypeSystem.Void);

            foreach (var prop in this.properties)
            {
                prop.CtorParameter = new ParameterDefinition(prop.Name, ParameterAttributes.None, prop.GenericParameter);
                ctor.Parameters.Add(prop.CtorParameter);
            }

            var ctorBody = ctor.Body;
            ctorBody.MaxStackSize = 8;
            var ilproc = ctorBody.GetILProcessor();
            ilproc.Emit(OpCodes.Ldarg_0);
            ilproc.Emit(OpCodes.Call, baseCtor);

            foreach (var prop in this.properties)
            {
                ilproc.Emit(OpCodes.Ldarg_0);
                ilproc.Emit(OpCodes.Ldarg, prop.CtorParameter);
                ilproc.Emit(OpCodes.Stfld, prop.FieldIlReference);
            }

            ilproc.Emit(OpCodes.Ret);

            this.definition.Methods.Add(ctor);
        }


        private void AddDebuggerBrowseableAttributesToFields()
        {
            var attrType = this.module.Import(typeof(DebuggerBrowsableAttribute));
            var methodDefinition = this.module.Import(attrType.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 1));
            foreach (var prop in this.properties)
            {
                var attr =
                    new CustomAttribute(methodDefinition);
                attr.ConstructorArguments.Add(
                    new CustomAttributeArgument(this.module.TypeSystem.String, DebuggerBrowsableState.Never));
                prop.Field.CustomAttributes.Add(attr);
            }
        }


        private void AddDebuggerDisplayAttribute()
        {
            // \{ Foo = {Foo}, Bar = {Bar} }
            var attrValue = $"\\{{ {string.Join(", ", this.properties.Select(x => string.Format("{0} = {{{0}}}", x.Name)))} }}";
            var attrType = this.module.Import(typeof(DebuggerDisplayAttribute));
            var methodDefinition = this.module.Import(attrType.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 1));
            var attr =
                new CustomAttribute(methodDefinition);
            attr.ConstructorArguments.Add(new CustomAttributeArgument(this.module.TypeSystem.String, attrValue));
            this.definition.CustomAttributes.Add(attr);
        }


        private void AddEquals()
        {
            var method = new MethodDefinition(
                "Equals",
                MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual
                | MethodAttributes.HideBySig, this.module.TypeSystem.Boolean);
            method.Body.MaxStackSize = 5;
            method.Body.InitLocals = true;

            var otherArg = new ParameterDefinition("value", ParameterAttributes.None, this.module.TypeSystem.Object);
            method.Parameters.Add(otherArg);
            var otherVar = new VariableDefinition(this.ilFieldDeclaringType);
            method.Body.Variables.Add(otherVar);
            var isEqualVar = new VariableDefinition(this.module.TypeSystem.Int32);
            method.Body.Variables.Add(isEqualVar);

            var il = method.Body.GetILProcessor();

            var setToFalseInstruction = Instruction.Create(OpCodes.Ldc_I4_0);
            //IL_0000: ldarg.1
            il.Emit(OpCodes.Ldarg, otherArg);
            //IL_0001: isinst class '<>f__AnonymousType0`2'<!'<Lo>j__TPar', !'<La>j__TPar'>
            il.Emit(OpCodes.Isinst, this.ilFieldDeclaringType);
            //IL_0006: stloc.0
            il.Emit(OpCodes.Stloc, otherVar);
            //IL_0007: ldloc.0
            il.Emit(OpCodes.Ldloc, otherVar);
            //IL_0008: brfalse.s IL_003a
            il.Emit(OpCodes.Brfalse, setToFalseInstruction);

            var storeResultInstruction = Instruction.Create(OpCodes.Stloc, isEqualVar);

            for (var i = 0; i < this.properties.Count; i++)
            {
                var prop = this.properties[i];

                //IL_000a: call class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<Lo>j__TPar'>::get_Default()
                il.Emit(OpCodes.Call, prop.GetDefaultEqualityComparerMethod);
                //IL_000f: ldarg.0
                il.Emit(OpCodes.Ldarg_0);
                //IL_0010: ldfld !0 class '<>f__AnonymousType0`2'<!'<Lo>j__TPar', !'<La>j__TPar'>::'<Lo>i__Field'
                il.Emit(OpCodes.Ldfld, prop.FieldIlReference);
                //IL_0015: ldloc.0
                il.Emit(OpCodes.Ldloc, otherVar);
                //IL_0016: ldfld !0 class '<>f__AnonymousType0`2'<!'<Lo>j__TPar', !'<La>j__TPar'>::'<Lo>i__Field'
                il.Emit(OpCodes.Ldfld, prop.FieldIlReference);
                //IL_001b: callvirt instance bool class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<Lo>j__TPar'>::Equals(!0, !0)
                il.Emit(OpCodes.Callvirt, prop.EqualityComparerEquals);
                //IL_0020: brfalse.s IL_003a
                if (i == this.properties.Count - 1)
                    il.Emit(OpCodes.Br, storeResultInstruction);
                else
                    il.Emit(OpCodes.Brfalse, setToFalseInstruction);
            }

            //IL_003a: ldc.i4.0
            il.Append(setToFalseInstruction);

            //IL_003b: stloc.1
            il.Append(storeResultInstruction);

            //IL_003c: br.s IL_003e

            //IL_003e: ldloc.1
            il.Emit(OpCodes.Ldloc, isEqualVar);

            //IL_003f: ret
            il.Emit(OpCodes.Ret);
            this.definition.Methods.Add(method);
        }


        private void AddGenericParams()
        {
            this.ilFieldDeclaringType = new GenericInstanceType(this.definition);

            foreach (var prop in this.properties)
            {
                prop.GenericParameter = new GenericParameter($"<{prop.Name}>j__TPar", this.definition);
                this.definition.GenericParameters.Add(prop.GenericParameter);
                this.ilFieldDeclaringType.GenericArguments.Add(prop.GenericParameter);
            }
        }


        private void AddGetHashCode()
        {
            var method = new MethodDefinition(
                "GetHashCode",
                MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual
                | MethodAttributes.HideBySig, this.module.TypeSystem.Int32);
            method.Body.MaxStackSize = 3;
            method.Body.InitLocals = true;
            var var0 = new VariableDefinition(this.module.TypeSystem.Int32);
            method.Body.Variables.Add(var0);
            var var1 = new VariableDefinition(this.module.TypeSystem.Int32);
            method.Body.Variables.Add(var1);

            var il = method.Body.GetILProcessor();

            // Different initial seed for different combinations of hashcodes
            int seed;
            using (var cipher = new SHA1Managed())
            {
                var uniqueTypeString = string.Join("|", this.properties.Select(x => x.Name));
                var hash = cipher.ComputeHash(Encoding.UTF8.GetBytes(uniqueTypeString));
                seed = BitConverter.ToInt32(hash, 0);
            }

            il.Emit(OpCodes.Ldc_I4, seed);
            il.Emit(OpCodes.Stloc, var0);

            foreach (var prop in this.properties)
            {
                //IL_0006: ldc.i4 -1521134295
                il.Emit(OpCodes.Ldc_I4, -1521134295);
                //IL_000b: ldloc.0
                il.Emit(OpCodes.Ldloc, var0);
                //IL_000c: mul
                il.Emit(OpCodes.Mul);
                //IL_000d: call class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<Lo>j__TPar'>::get_Default()

                il.Emit(OpCodes.Call, prop.GetDefaultEqualityComparerMethod);
                //IL_0012: ldarg.0
                il.Emit(OpCodes.Ldarg_0);
                //IL_0013: ldfld !0 class '<>f__AnonymousType0`2'<!'<Lo>j__TPar', !'<La>j__TPar'>::'<Lo>i__Field'
                il.Emit(OpCodes.Ldfld, prop.FieldIlReference);
                //IL_0018: callvirt instance int32 class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<Lo>j__TPar'>::GetHashCode(!0)
                il.Emit(OpCodes.Callvirt, prop.EqualityComparerGetHashCode);
                //IL_001d: add
                il.Emit(OpCodes.Add);
                //IL_001e: stloc.0
                il.Emit(OpCodes.Stloc, var0);
            }

            il.Emit(OpCodes.Ldloc, var0);
            il.Emit(OpCodes.Stloc, var1);
            il.Emit(OpCodes.Ldloc, var1);
            il.Emit(OpCodes.Ret);

            this.definition.Methods.Add(method);
        }


        private void AddProperty(Property prop)
        {
            prop.Field = new FieldDefinition(
                $"<{prop.Name}>i__Field",
                FieldAttributes.InitOnly | FieldAttributes.Private,
                prop.GenericParameter);
            this.definition.Fields.Add(prop.Field);

            // Getter
            prop.Definition = new PropertyDefinition(prop.Name, PropertyAttributes.None, prop.GenericParameter);
            this.definition.Properties.Add(prop.Definition);

            prop.GetMethod = new MethodDefinition(
                $"get_{prop.Name}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                prop.GenericParameter);

            prop.FieldIlReference = new FieldReference(
                prop.Field.Name, prop.GenericParameter, this.ilFieldDeclaringType);
            var getter = prop.GetMethod;
            var getterBody = getter.Body;
            getterBody.MaxStackSize = 1;
            getterBody.InitLocals = true;
            getterBody.Variables.Add(new VariableDefinition(prop.GenericParameter));
            var ilProc = getterBody.GetILProcessor();
            ilProc.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProc.Append(Instruction.Create(OpCodes.Ldfld, prop.FieldIlReference));
            ilProc.Append(Instruction.Create(OpCodes.Stloc_0));
            ilProc.Append(Instruction.Create(OpCodes.Ldloc_0));
            ilProc.Append(Instruction.Create(OpCodes.Ret));
            prop.GetMethod = getter;
            this.definition.Methods.Add(getter);

            prop.Definition.GetMethod = getter;

            var equalityComparerOfPropTypeDef = this.module.Import(typeof(EqualityComparer<>)).Resolve();
            prop.GetDefaultEqualityComparerMethod = MakeHostInstanceGeneric(
                equalityComparerOfPropTypeDef.
                    Properties.Where(x => x.Name == "Default" && x.GetMethod.IsStatic).Select(x => x.GetMethod).
                    First(),
                prop.GenericParameter);
            prop.EqualityComparerGetHashCode =
                MakeHostInstanceGeneric(
                    equalityComparerOfPropTypeDef.Methods.First(x => x.Name == "GetHashCode"), prop.GenericParameter);
            prop.EqualityComparerEquals =
                MakeHostInstanceGeneric(
                    equalityComparerOfPropTypeDef.Methods.First(x => x.Name == "Equals"), prop.GenericParameter);

            // Constructor */
        }


        private void AddRuntimeCompabilityAttributeToAssembly()
        {
            var attrType = this.module.Import(typeof(RuntimeCompatibilityAttribute));
            var methodDefinition = this.module.Import(attrType.Resolve().Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
            var attr =
                new CustomAttribute(methodDefinition);

            attr.Properties.Add(
                new CustomAttributeNamedArgument(
                    "WrapNonExceptionThrows", new CustomAttributeArgument(this.module.TypeSystem.Boolean, true)));
            this.module.Assembly.CustomAttributes.Add(attr);
        }


        private void AddToString()
        {
            var method = new MethodDefinition(
                "ToString",
                MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual
                | MethodAttributes.HideBySig, this.module.TypeSystem.String);
            method.Body.MaxStackSize = 2;
            method.Body.InitLocals = true;

            var stringBuilderTypeRef = this.module.Import(typeof(StringBuilder));
            var stringBuilderTypeDef = stringBuilderTypeRef.Resolve();
            var stringBuilderCtor = this.module.Import(
                stringBuilderTypeDef.GetConstructors().First(x => x.Parameters.Count == 0 && !x.IsStatic));
            var appendStringMethod = this.module.Import(
                stringBuilderTypeDef.Methods.First(
                    x =>
                        x.Name == "Append" && x.Parameters.Count == 1
                        && x.Parameters[0].ParameterType.FullName == this.module.TypeSystem.String.FullName));
            var appendObjectMethod = this.module.Import(
                stringBuilderTypeDef.Methods.First(
                    x =>
                        x.Name == "Append" && x.Parameters.Count == 1
                        && x.Parameters[0].ParameterType.FullName == this.module.TypeSystem.Object.FullName));

            var stringBuilderToStringMethod = this.module.Import(
                stringBuilderTypeDef.Methods.First(
                    x => !x.IsStatic && x.Name == "ToString" && x.Parameters.Count == 0));

            var stringBuilderVar = new VariableDefinition(stringBuilderTypeRef);
            method.Body.Variables.Add(stringBuilderVar);
            var tmpStringVar = new VariableDefinition(this.module.TypeSystem.String);
            method.Body.Variables.Add(tmpStringVar);

            var il = method.Body.GetILProcessor();
            //IL_0000: newobj instance void [mscorlib]System.Text.StringBuilder::.ctor()
            il.Emit(OpCodes.Newobj, stringBuilderCtor);
            //IL_0005: stloc.0
            il.Emit(OpCodes.Stloc, stringBuilderVar);
            //IL_0006: ldloc.0
            il.Emit(OpCodes.Ldloc, stringBuilderVar);
            //IL_0007: ldstr "{ Target = "
            il.Emit(OpCodes.Ldstr, $"{{ {this.properties[0].Name} = ");
            //IL_000c: callvirt instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
            il.Emit(OpCodes.Callvirt, appendStringMethod);
            //IL_0011: pop
            il.Emit(OpCodes.Pop);
            //IL_0012: ldloc.0
            il.Emit(OpCodes.Ldloc, stringBuilderVar);
            //IL_0013: ldarg.0
            il.Emit(OpCodes.Ldarg_0);
            //IL_0014: ldfld !0 class '<>f__AnonymousType1`2'<!'<Target>j__TPar', !'<Elements>j__TPar'>::'<Target>i__Field'
            il.Emit(OpCodes.Ldfld, this.properties[0].FieldIlReference);
            //IL_0019: box !'<Target>j__TPar'
            il.Emit(OpCodes.Box, this.properties[0].GenericParameter);
            //IL_001e: callvirt instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
            il.Emit(OpCodes.Callvirt, appendObjectMethod);
            //IL_0023: pop
            il.Emit(OpCodes.Pop);

            foreach (var prop in this.properties.Skip(1))
            {
                //IL_0024: ldloc.0
                il.Emit(OpCodes.Ldloc, stringBuilderVar);
                //IL_0025: ldstr ", Elements = "
                il.Emit(OpCodes.Ldstr, $", {prop.Name} = ");
                //IL_002a: callvirt instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
                il.Emit(OpCodes.Callvirt, appendStringMethod);
                //IL_002f: pop
                il.Emit(OpCodes.Pop);
                //IL_0030: ldloc.0
                il.Emit(OpCodes.Ldloc, stringBuilderVar);
                //IL_0031: ldarg.0
                il.Emit(OpCodes.Ldarg_0);
                //IL_0032: ldfld !1 class '<>f__AnonymousType1`2'<!'<Target>j__TPar', !'<Elements>j__TPar'>::'<Elements>i__Field'
                il.Emit(OpCodes.Ldfld, prop.FieldIlReference);
                //IL_0037: box !'<Elements>j__TPar'
                il.Emit(OpCodes.Box, prop.GenericParameter);
                //IL_003c: callvirt instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
                il.Emit(OpCodes.Callvirt, appendObjectMethod);
                //IL_0041: pop
                il.Emit(OpCodes.Pop);
            }

            //IL_0042: ldloc.0
            il.Emit(OpCodes.Ldloc, stringBuilderVar);
            //IL_0043: ldstr " }"
            il.Emit(OpCodes.Ldstr, " }");
            //IL_0048: callvirt instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
            il.Emit(OpCodes.Callvirt, appendStringMethod);
            //IL_004d: pop
            il.Emit(OpCodes.Pop);
            //IL_004e: ldloc.0
            il.Emit(OpCodes.Ldloc, stringBuilderVar);
            //IL_004f: callvirt instance string [mscorlib]System.Object::ToString()
            il.Emit(OpCodes.Callvirt, stringBuilderToStringMethod);
            //IL_0054: stloc.1
            il.Emit(OpCodes.Stloc, tmpStringVar);
            //IL_0055: br.s IL_0057
            // nah
            //IL_0057: ldloc.1
            il.Emit(OpCodes.Ldloc, tmpStringVar);
            //IL_0058: ret
            il.Emit(OpCodes.Ret);

            this.definition.Methods.Add(method);
        }


        private static int AllocateUniqueAnonymousClassNumber()
        {
            return Interlocked.Increment(ref anonTypeNumber);
        }


        private static string GetAnonTypeKey(IEnumerable<string> propNames)
        {
            return string.Join(",", propNames);
        }


        private static Type GetAnonymousType(IEnumerable<string> propNames)
        {
            var anonType = anonTypeCache.GetOrCreate(GetAnonTypeKey(propNames), () =>
            {
                var atb = new AnonymousTypeBuilder(propNames);
                var typedef = atb.BuildAnonymousType();
                var memStream = new MemoryStream();
                typedef.Module.Assembly.Write(memStream);
                var loadedAsm = AppDomain.CurrentDomain.Load(memStream.ToArray());
                return loadedAsm.GetTypes().First(x => x.Name == typedef.Name);
            });
            return anonType;
        }


        private TypeReference Import<T>()
        {
            return this.module.Import(typeof(T));
        }

        #region Nested type: Property

        private class Property
        {
            public Property()
            {
                // Set to invalid value by default
                Index = int.MinValue;
            }


            public ParameterDefinition CtorParameter { get; set; }
            public PropertyDefinition Definition { get; set; }
            public MethodReference EqualityComparerEquals { get; set; }
            public MethodReference EqualityComparerGetHashCode { get; set; }
            public FieldDefinition Field { get; set; }
            public FieldReference FieldIlReference { get; set; }
            public GenericParameter GenericParameter { get; set; }
            public MethodReference GetDefaultEqualityComparerMethod { get; set; }
            public MethodDefinition GetMethod { get; set; }
            public int Index { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}