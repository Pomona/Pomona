#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
        [Category("FailsOnFileShare")]
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