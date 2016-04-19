#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Pomona.CodeGen;
using Pomona.Common.Internals;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.Example.SimpleExtraSite;
using Pomona.FluentMapping;

namespace Pomona.UnitTests.GenerateClientDllApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Modify property Protected of class Critter to not be protected in client dll.
            // This is to test setting a protected property will throw exception on server.

            WriteClientLibrary(@"../../../../lib/Critters.Client.dll", new TypeMapper(new ModifiedCritterPomonaConfiguration()));
            WriteClientLibrary(@"../../../../lib/Extra.Client.dll", new TypeMapper(new SimplePomonaConfiguration()));
            WriteClientLibrary(@"../../../../lib/IndependentCritters.dll", new TypeMapper(new IndependentClientDllConfiguration()));

            Console.WriteLine("Wrote client dlls.");
        }


        private static void TransformAssemblyHook(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var td = new TypeDefinition("Donkey", "Kong", TypeAttributes.Public);

            // Empty public constructor
            var baseCtor =
                module.Import(module.TypeSystem.Object.Resolve().GetConstructors().First(x => !x.IsStatic && x.Parameters.Count == 0));
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                | MethodAttributes.Public,
                module.TypeSystem.Void);
            ctor.Body.MaxStackSize = 8;
            var ctorIlProcessor = ctor.Body.GetILProcessor();
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, baseCtor));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            td.Methods.Add(ctor);
            td.BaseType = module.TypeSystem.Object;
            module.Types.Add(td);
        }


        private static void WriteClientLibrary(string dllName, TypeMapper typeMapper)
        {
            dllName = Path.GetFullPath(dllName);
            Console.WriteLine("Writing dll to {0}", dllName);
            var xmlDocName = Path.Combine(Path.GetDirectoryName(dllName), Path.GetFileNameWithoutExtension(dllName) + ".xml");

            // Avoid modifying existing xml doc file while Visual studio is reading it.
            if (File.Exists(xmlDocName))
                File.Delete(xmlDocName);
            using (var file = new FileStream(dllName, FileMode.OpenOrCreate))
            {
                if (!dllName.EndsWith(".dll"))
                    throw new ArgumentException("Filename should end with .dll");
                ClientLibGenerator.WriteClientLibrary(typeMapper, file, xmlDocStreamFactory : () => File.OpenWrite(xmlDocName),
                                                      assemblyTransformHook : TransformAssemblyHook);
            }
        }

        #region Nested type: IndependentClientDllConfiguration

        private class IndependentClientDllConfiguration : ModifiedCritterPomonaConfiguration
        {
            public override ITypeMappingFilter TypeMappingFilter
            {
                get { return new IndependentClientDllTypeMappingFilter(SourceTypes); }
            }

            #region Nested type: IndependentClientDllTypeMappingFilter

            private class IndependentClientDllTypeMappingFilter : CritterTypeMappingFilter
            {
                public IndependentClientDllTypeMappingFilter(IEnumerable<Type> sourceTypes)
                    : base(sourceTypes)
                {
                }


                public override ClientMetadata ClientMetadata
                {
                    get { return base.ClientMetadata.With("IndependentCritters"); }
                }


                public override bool GenerateIndependentClient()
                {
                    return true;
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: ModifiedCritterPomonaConfiguration

        private class ModifiedCritterPomonaConfiguration : CritterPomonaConfiguration
        {
            public ModifiedCritterPomonaConfiguration()
            {
                TypeMappingFilter = new ModifiedTypeMappingFilter(SourceTypes);
            }


            public override IEnumerable<object> FluentRuleObjects
            {
                get { return base.FluentRuleObjects.Append(new ModifiedFluentRules()); }
            }

            public override ITypeMappingFilter TypeMappingFilter { get; }
        }

        #endregion

        #region Nested type: ModifiedFluentRules

        internal class ModifiedFluentRules
        {
            public void Map(ITypeMappingConfigurator<UnpostableThingOnServer> map)
            {
                map.PostAllowed();
            }


            public void Map(ITypeMappingConfigurator<Critter> map)
            {
                map.Include(x => x.PublicAndReadOnlyThroughApi, o => o.ReadOnly());
            }
        }

        #endregion

        #region Nested type: ModifiedTypeMappingFilter

        private class ModifiedTypeMappingFilter : CritterTypeMappingFilter
        {
            public ModifiedTypeMappingFilter(IEnumerable<Type> sourceTypes)
                : base(sourceTypes)
            {
            }


            public override bool GetTypeIsAbstract(Type type)
            {
                if (type == typeof(AbstractOnServerAnimal))
                    return false;
                return base.GetTypeIsAbstract(type);
            }
        }

        #endregion
    }
}