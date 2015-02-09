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