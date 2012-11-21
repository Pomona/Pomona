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

using NuGet;

using Pomona.Client;

namespace Pomona.CodeGen
{
    public class ClientNugetPackageBuilder
    {
        private TypeMapper typeMapper;


        public ClientNugetPackageBuilder(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }


        public string PackageFileName
        {
            get { return this.typeMapper.Filter.GetClientAssemblyName() + "." + GetVersionString() + ".nupkg"; }
        }


        public void BuildPackage(Stream stream)
        {
            var tempPath = Path.GetTempPath() + Guid.NewGuid().ToString("N");
            try
            {
                WritePackageContents(tempPath);
                CreateNugetPackage(tempPath, stream);
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }


        private static string GetVersionString()
        {
            return "1.0.0.0";
        }


        private void CreateNugetPackage(string tempPath, Stream stream)
        {
            var metadata = GetManifestMetadata();

            var packageBuilder = new PackageBuilder();

            // Have no idea what this means, copy paste from http://stackoverflow.com/questions/6808868/howto-create-a-nuget-package-using-nuget-core
            packageBuilder.PopulateFiles(tempPath, new[] { new ManifestFile() { Source = "**" } });

            packageBuilder.Populate(metadata);

            packageBuilder.DependencySets.Add(
                new PackageDependencySet(
                    null,
                    new[]
                    {
                        new PackageDependency(
                        "Mono.Cecil", new VersionSpec(new SemanticVersion("0.9.5.0"))),
                        new PackageDependency("Newtonsoft.Json", new VersionSpec(new SemanticVersion("4.5.11"))),
                    }));

            packageBuilder.Save(stream);
        }


        private ManifestMetadata GetManifestMetadata()
        {
            return new ManifestMetadata()
            {
                Authors = "nobody@example.com", // TODO: find a way to configure author in nuget file
                Version = GetVersionString(), // TODO: API versioning
                Id = this.typeMapper.Filter.GetClientAssemblyName(),
                Description = "TODO: Make it possible to set description for nuget file"
            };
        }


        private void WritePackageContents(string tempPath)
        {
            var clientLibGen = new ClientLibGenerator(this.typeMapper);

            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(Path.Combine(tempPath, "lib"));
            var dllDir = Path.Combine(tempPath, "lib", "net40");
            Directory.CreateDirectory(dllDir);
            var dllPath = Path.Combine(dllDir, this.typeMapper.Filter.GetClientAssemblyName() + ".dll");

            using (var stream = File.Create(dllPath))
            {
                clientLibGen.PomonaClientEmbeddingEnabled = false;
                clientLibGen.CreateClientDll(stream);
            }

            var pomonaClientAssemblySourcePath = typeof(ClientBase<>).Assembly.Location;

            var pomonaClientAssemblyDestPath = Path.Combine(dllDir, Path.GetFileName(pomonaClientAssemblySourcePath));

            File.Copy(pomonaClientAssemblySourcePath, pomonaClientAssemblyDestPath);
        }
    }
}