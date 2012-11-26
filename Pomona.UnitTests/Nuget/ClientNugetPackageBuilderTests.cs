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
using System.IO;

using NUnit.Framework;

using Pomona.CodeGen;
using Pomona.UnitTests.PomonaSession;

namespace Pomona.UnitTests.Nuget
{
    [TestFixture]
    public class ClientNugetPackageBuilderTests : SessionTestsBase
    {
        [Test]
        public void BuildPackage_DoesNotThrowAnyExceptions()
        {
            var packageBuilder = new ClientNugetPackageBuilder(TypeMapper);
            byte[] fileData;
            using (var fileStream = new MemoryStream())
            {
                packageBuilder.BuildPackage(fileStream);
                fileData = fileStream.ToArray();
            }
            Console.WriteLine("Package size is " + fileData.Length / 1024.0 + " KiB");
        }


        [Ignore("For running manually, to inspect nupkg (will put resulting nupkg in current dir)")]
        [Test]
        public void BuildPackage_ToFile()
        {
            var packageBuilder = new ClientNugetPackageBuilder(TypeMapper);
            using (var fileStream = File.Create(packageBuilder.PackageFileName))
            {
                packageBuilder.BuildPackage(fileStream);
            }
        }
    }
}