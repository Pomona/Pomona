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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class ClientGeneratedTypeTests : ClientTestsBase
    {
        private static void PeVerify(string dllPath)
        {
            var programFilesX86Path = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ??
                                      Environment.GetEnvironmentVariable("ProgramFiles");
            Assert.NotNull(programFilesX86Path);

            var peverifyPath = Path.Combine(programFilesX86Path,
                @"Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\PEVerify.exe");
            if (!File.Exists(peverifyPath))
                Assert.Inconclusive("Unable to run peverify test, need to have Microsoft sdk installed.");

            var peVerifyArguments = string.Format("\"{0}\" /md /il", dllPath.Replace("\"", "\\\""));

            var proc = new Process
            {
                StartInfo =
                    new ProcessStartInfo(peverifyPath, peVerifyArguments)
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = false,
                        UseShellExecute = false
                    }
            };
            proc.Start();
            Console.Write(proc.StandardOutput.ReadToEnd());
            proc.WaitForExit();
            Assert.That(proc.ExitCode, Is.EqualTo(0), "PEVerify returned error code " + proc.ExitCode);
        }


        [Test]
        public void AllPropertyTypesOfClientTypesAreAllowed()
        {
            var clientAssembly = typeof(ICritter).Assembly;
            var allPropTypes =
                clientAssembly.GetExportedTypes().SelectMany(
                    x => x.GetProperties().Select(y => y.PropertyType)).Distinct();

            var allTypesOk = true;
            foreach (var type in allPropTypes)
            {
                if (!IsAllowedType(type))
                {
                    allTypesOk = false;
                    var typeLocal = type;
                    var propsWithType = clientAssembly
                        .GetExportedTypes()
                        .SelectMany(x => x.GetProperties())
                        .Where(x => x.PropertyType == typeLocal).ToList();
                    foreach (var propertyInfo in propsWithType)
                    {
                        Console.WriteLine(
                            "Property {0} of {1} has type {2} of assembly {3}, which should not be referenced by client!",
                            propertyInfo.Name,
                            propertyInfo.DeclaringType.FullName,
                            propertyInfo.PropertyType.FullName,
                            propertyInfo.PropertyType.Assembly.FullName);
                    }
                }
            }

            Assert.IsTrue(allTypesOk, "There was properties in CritterClient with references to disallowed assemblies.");
        }


        [Test]
        public void AssemblyVersionSetToApiVersionFromTypeMappingFilter()
        {
            Assert.That(typeof(Client).Assembly.GetName().Version.ToString(3), Is.EqualTo("0.1.0"));
        }


        [Test]
        public void ClientLibraryIsCorrectlyGenerated()
        {
            var foundError = false;
            var errors = new StringBuilder();
            foreach (
                var prop in
                    Client.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                        x =>
                            x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ClientRepository<,>)))
            {
                var value = prop.GetValue(Client, null);
                if (value == null)
                {
                    foundError = true;
                    errors.AppendFormat("Property {0} of generated client lib is null\r\n", prop.Name);
                }
                if (prop.GetSetMethod(true).IsPublic)
                {
                    foundError = true;
                    errors.AppendFormat("Property {0} of generated client lib has a public setter.\r\n", prop.Name);
                }
            }

            if (foundError)
                Assert.Fail("Found the following errors on generated client lib: {0}\r\n", errors);
        }


        [Test]
        public void GeneratedPocoTypeInitializesDictionaryPropertyInConstructor()
        {
            var dictContainer = new DictionaryContainerResource();
            Assert.That(dictContainer.Map, Is.Not.Null);
        }


        [Test]
        public void GeneratedPocoTypeInitializesListPropertyInConstructor()
        {
            var critter = new CritterResource();
            Assert.That(critter.Weapons, Is.Not.Null);
        }


        [Test]
        public void GeneratedPocoTypeInitializesValueObjectPropertyInConstructor()
        {
            var critter = new CritterResource();
            Assert.That(critter.CrazyValue, Is.Not.Null);
        }


        [Test]
        public void GeneratedPropertyHasResourcePropertyAttributeWithAccessDeclared()
        {
            var prop = typeof(ICritter).GetProperty("Name");
            Assert.That(prop, Is.Not.Null);
            var attr = prop.GetCustomAttributes(true).OfType<ResourcePropertyAttribute>().First();
            Assert.That(attr.AccessMode, Is.EqualTo(HttpMethod.Post | HttpMethod.Put | HttpMethod.Get));
        }


        [Test]
        public void MiddleBaseClassExcludedFromMapping_WillBeExcludedInGeneratedClient()
        {
            Assert.That(typeof(IInheritsFromHiddenBase).GetInterfaces(), Has.Member(typeof(IEntityBase)));
            Assert.That(typeof(Client).Assembly.GetTypes().Count(x => x.Name == "IHiddenBaseInMiddle"), Is.EqualTo(0));
            Assert.That(typeof(IInheritsFromHiddenBase).GetProperty("ExposedFromDerivedResource"), Is.Not.Null);
        }


        [Test]
        public void PeVerify_ClientWithEmbeddedPomonaCommon_HasExitCode0()
        {
            var origDllPath = typeof(ICritter).Assembly.Location;
            var dllDir = Path.GetDirectoryName(origDllPath);
            var clientWithEmbeddedStuffName = Path.Combine(dllDir, "..\\..\\..\\..\\lib\\IndependentCritters.dll");
            var newDllPath = Path.Combine(dllDir, "TempCopiedIndependentCrittersDll.tmp");
            File.Copy(clientWithEmbeddedStuffName, newDllPath, true);
            PeVerify(newDllPath);
        }


        [Test]
        public void PeVerify_HasExitCode0()
        {
            PeVerify(typeof(ICritter).Assembly.Location);
        }


        [Test(Description = "This test has been added since more errors are discovered when dll has been renamed.")]
        public void PeVerify_RenamedToAnotherDllName_StillHasExitCode0()
        {
            var origDllPath = typeof(ICritter).Assembly.Location;
            Console.WriteLine(Path.GetDirectoryName(origDllPath));
            var newDllPath = Path.Combine(Path.GetDirectoryName(origDllPath), "TempCopiedClientLib.tmp");
            File.Copy(origDllPath, newDllPath, true);
            PeVerify(newDllPath);
            //Assert.Inconclusive();
        }


        [Test]
        public void PropertyGeneratedFromInheritedVirtualProperty_IsNotDuplicatedOnInheritedInterface()
        {
            Assert.That(typeof(IAbstractAnimal).GetProperty("TheVirtualProperty"), Is.Not.Null);
            Assert.That(typeof(IBear).GetProperty("TheVirtualProperty"), Is.EqualTo(null));
            Assert.That(typeof(IAbstractAnimal).GetProperty("TheAbstractProperty"), Is.Not.Null);
            Assert.That(typeof(IBear).GetProperty("TheAbstractProperty"), Is.EqualTo(null));
        }


        [Test]
        public void PropertyOfPostFormOfAbstractType_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            Assert.That(typeof(AbstractAnimalForm).GetProperty("PublicAndReadOnlyThroughApi"), Is.Null);
        }


        [Test]
        public void PropertyOfPostForm_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            Assert.That(typeof(CritterForm).GetProperty("PublicAndReadOnlyThroughApi"), Is.Null);
        }


        [Test]
        public void ResourceInfoAttributeOfGeneratedTypeHasCorrectEtagPropertySet()
        {
            var resInfo = typeof(IEtaggedEntity).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(resInfo.EtagProperty, Is.EqualTo(typeof(IEtaggedEntity).GetProperty("ETag")));
        }


        [Test]
        public void ResourceInfoAttributeOfGeneratedTypeHasCorrectIdPropertySet()
        {
            var resInfo = typeof(ICritter).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(resInfo.EtagProperty, Is.EqualTo(typeof(IEtaggedEntity).GetProperty("Id")));
        }


        [Test]
        public void ResourceInfoAttributeOfGeneratedTypeHasParentResourceTypeSet()
        {
            var resInfo = typeof(IPlanet).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(resInfo.ParentResourceType, Is.EqualTo(typeof(IPlanetarySystem)));
        }


        [Test]
        public void ResourceInheritedFromResourceWithPostDeniedDoesNotHavePostResourceFormGenerated()
        {
            var typeInfo =
                typeof(IInheritedUnpostableThing).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(typeInfo.PostFormType, Is.Null);
            Assert.That(
                typeof(IInheritedUnpostableThing).Assembly.GetType("Critters.Client.InheritedUnpostableThingForm"),
                Is.Null);
        }


        [Test]
        public void ResourceWithPatchDeniedDoesNotHavePatchResourceFormGenerated()
        {
            var typeInfo = typeof(IUnpatchableThing).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(typeInfo.PatchFormType, Is.Null);
            Assert.That(typeof(IUnpatchableThing).Assembly.GetType("Critters.Client.UnpatchableThingPatchForm"),
                Is.Null);
            Assert.That(typeof(IUnpatchableThing).Assembly.GetType("Critters.Client.CritterPatchForm"), Is.Not.Null);
        }


        [Test]
        public void ResourceWithPostDeniedDoesNotHavePostResourceFormGenerated()
        {
            var typeInfo = typeof(IUnpostableThing).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(typeInfo.PostFormType, Is.Null);
            Assert.That(typeof(IUnpostableThing).Assembly.GetType("Critters.Client.UnpostableThingForm"), Is.Null);
            Assert.That(typeof(IUnpostableThing).Assembly.GetType("Critters.Client.CritterForm"), Is.Not.Null);
        }


        [Test]
        public void ThingIndependentFromBase_DoesNotInheritEntityBase()
        {
            Assert.That(!typeof(IEntityBase).IsAssignableFrom(typeof(IThingIndependentFromBase)));
        }


        [Test]
        public void ThingIndependentFromBase_IncludesPropertyFromEntityBase()
        {
            Assert.That(typeof(IThingIndependentFromBase).GetProperty("Id"), Is.Not.Null);
        }
    }
}