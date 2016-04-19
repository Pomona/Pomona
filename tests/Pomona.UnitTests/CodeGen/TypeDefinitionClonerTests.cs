#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;

using Mono.Cecil;

using NUnit.Framework;

using Pomona.CodeGen;

namespace Pomona.UnitTests.CodeGen
{
    [TestFixture]
    public class TypeDefinitionClonerTests
    {
        [Test]
        [Category("WindowsRequired")]
        public void Cloned_Type_Passes_PeVerify()
        {
            var fn = "Copycat.dll";
            try
            {
                if (File.Exists(fn))
                    File.Delete(fn);

                var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(IndependentClass).Assembly.Location);

                var destAssembly =
                    AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Copycat", new Version(1, 1, 1, 1)),
                                                      "Copycat",
                                                      ModuleKind.Dll);
                var destinationModule = destAssembly.MainModule;

                var tdc = new TypeDefinitionCloner(destinationModule);
                tdc.Clone(sourceAssembly.MainModule.GetType("Pomona.UnitTests.CodeGen.IndependentClass"));
                //tdc.Clone(sourceAssembly.MainModule.GetType("Pomona.UnitTests.CodeGen.StringEnumExample"));
                destAssembly.Write(fn);

                PeVerifyHelper.Verify(fn);
            }
            finally
            {
                if (File.Exists(fn))
                    File.Delete(fn);
            }
        }
    }
}