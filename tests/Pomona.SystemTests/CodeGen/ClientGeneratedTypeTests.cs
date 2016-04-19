#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Critters.Client;

using Mono.Cecil;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;
using Pomona.UnitTests;

using HttpMethod = Pomona.Common.HttpMethod;

namespace Pomona.SystemTests.CodeGen
{
    [TestFixture]
    public class ClientGeneratedTypeTests
    {
        private static readonly HashSet<Assembly> allowedAssemblies =
            new HashSet<Assembly>(new[]
            {
                typeof(object),
                typeof(ICritter),
                typeof(PomonaClient),
                typeof(IQueryProvider),
                typeof(Uri),
                typeof(HttpClient)
            }.Select(x => x.Assembly));

        private static Assembly ClientAssembly
        {
            get { return typeof(CritterClient).Assembly; }
        }


        [Test]
        public void AbstractClassOnServerIsAbstractOnClient()
        {
            Assert.That(typeof(AbstractAnimalForm).IsAbstract);
        }


        [Test]
        public void AllInterfacesArePrefixedByLetterI()
        {
            foreach (var t in ClientAssembly.GetTypes().Where(x => x.IsInterface))
            {
                try
                {
                    Assert.That(t.Name.Length, Is.GreaterThan(1));
                    Assert.That(t.Name[0], Is.EqualTo('I'));
                    Assert.That(char.IsUpper(t.Name[1]), Is.True);
                }
                catch (AssertionException)
                {
                    Console.WriteLine("Failed while testing type " + t.FullName);
                    throw;
                }
            }
        }


        [Test]
        public void AllPropertyTypesOfClientTypesAreAllowed()
        {
            var allPropTypes =
                ClientAssembly.GetExportedTypes().SelectMany(
                    x => x.GetProperties().Select(y => y.PropertyType)).Distinct();

            var allTypesOk = true;
            foreach (var type in allPropTypes)
            {
                if (!IsAllowedType(type))
                {
                    allTypesOk = false;
                    var typeLocal = type;
                    var propsWithType = ClientAssembly
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
            Assert.That(ClientAssembly.GetName().Version.ToString(3), Is.EqualTo("0.1.0"));
        }


        [Test]
        public void ClientLibraryIsCorrectlyGenerated()
        {
            var foundError = false;
            var errors = new StringBuilder();
            var client = new CritterClient("http://test", new HttpWebClient(new HttpClient()));
            foreach (
                var prop in
                    typeof(CritterClient).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                        x =>
                            x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ClientRepository<,,>)))
            {
                var value = prop.GetValue(client, null);
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
        public void Constructor_TakingBaseUri_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => new CritterClient("http://whatever"));
        }


        [Test]
        public void ConstructorOfInheritedClientDoesNotThrowException()
        {
            Assert.DoesNotThrow(() => new InheritedClient("http://test/", new HttpWebClient(new NoopHttpMessageHandler())));
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
            Assert.That(attr.AccessMode, Is.EqualTo(HttpMethod.Post | HttpMethod.Put | HttpMethod.Get | HttpMethod.Patch));
        }


        [Test]
        public void MiddleBaseClassExcludedFromMapping_WillBeExcludedInGeneratedClient()
        {
            Assert.That(typeof(IInheritsFromHiddenBase).GetInterfaces(), Has.Member(typeof(IEntityBase)));
            Assert.That(typeof(CritterClient).Assembly.GetTypes().Count(x => x.Name == "IHiddenBaseInMiddle"), Is.EqualTo(0));
            Assert.That(typeof(IInheritsFromHiddenBase).GetProperty("ExposedFromDerivedResource"), Is.Not.Null);
        }


        [Test]
        public void NameOfGeneratedTypeFromInterfaceDoesNotGetDoubleIPrefix()
        {
            Assert.That(typeof(ICritter).Assembly.GetTypes().Any(x => x.Name == "IIExposedInterface"), Is.False);
            Assert.That(typeof(ICritter).Assembly.GetTypes().Any(x => x.Name == "IExposedInterface"), Is.True);
        }


        [Test]
        public void NoClassesArePrefixedWithTheLetterI()
        {
            foreach (var t in typeof(CritterClient).Assembly.GetTypes().Where(x => !x.IsInterface))
            {
                try
                {
                    Assert.That(t.Name.Length, Is.GreaterThan(1));
                    Assert.That(char.IsUpper(t.Name[0]), Is.True);
                    Assert.That(char.IsLower(t.Name[1]), Is.True);
                }
                catch (AssertionException)
                {
                    Console.WriteLine("Failed while testing type " + t.FullName);
                    throw;
                }
            }
        }


        [Test]
        public void ObsoletePropertyIsCopiedFromServerProperty()
        {
            Assert.That(
                typeof(ICritter).GetProperty("ObsoletedProperty").GetCustomAttributes(true).OfType<ObsoleteAttribute>(),
                Is.Not.Empty);
        }


        [Category("WindowsRequired")]
        [Test]
        public void PeVerify_ClientWithEmbeddedPomonaCommon_HasExitCode0()
        {
            var origDllPath = ClientAssembly.CodeBaseAbsolutePath();
            var dllDir = Path.GetDirectoryName(origDllPath);
            var clientWithEmbeddedStuffName = Path.Combine(dllDir, "../../../../lib/IndependentCritters.dll");
            var newDllPath = Path.Combine(dllDir, "TempCopiedIndependentCrittersDll.tmp");
            File.Copy(clientWithEmbeddedStuffName, newDllPath, true);
            PeVerify(newDllPath);
        }


        [Category("WindowsRequired")]
        [Test]
        public void PeVerify_HasExitCode0()
        {
            PeVerify(typeof(ICritter).Assembly.Location);
        }


        [Category("WindowsRequired")]
        [Test(Description = "This test has been added since more errors are discovered when dll has been renamed.")]
        public void PeVerify_RenamedToAnotherDllName_StillHasExitCode0()
        {
            var origDllPath = ClientAssembly.CodeBaseAbsolutePath();
            Console.WriteLine(Path.GetDirectoryName(origDllPath));
            var newDllPath = Path.Combine(Path.GetDirectoryName(origDllPath), "TempCopiedClientLib.tmp");
            File.Copy(origDllPath, newDllPath, true);
            PeVerify(newDllPath);
            //Assert.Inconclusive();
        }


        [Ignore("This test is only applicable when DISABLE_PROXY_GENERATION is set.")]
        [Test]
        public void PomonaCommonHaveZeroReferencesToEmitNamespace()
        {
            var assembly = AssemblyDefinition.ReadAssembly(ClientAssembly.CodeBaseAbsolutePath());
            var trefs =
                assembly.MainModule.GetTypeReferences().Where(x => x.Namespace == "System.Reflection.Emit").ToList();
            Assert.That(trefs, Is.Empty);
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
        public void PropertyOfPostForm_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            Assert.That(typeof(CritterForm).GetProperty("PublicAndReadOnlyThroughApi"), Is.Null);
        }


        [Test]
        public void PropertyOfPostFormOfAbstractType_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            Assert.That(typeof(AbstractAnimalForm).GetProperty("PublicAndReadOnlyThroughApi"), Is.Null);
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


        [Test]
        public void TypeWasAddedInTransformAssemblyHooko()
        {
            Assert.That(ClientAssembly.GetTypes().Any(x => x.FullName == "Donkey.Kong"), Is.True);
        }


        protected bool IsAllowedType(Type t)
        {
            return FlattenGenericTypeHierarchy(t).All(x => IsAllowedClientReferencedAssembly(x.Assembly));
        }


        private IEnumerable<Type> FlattenGenericTypeHierarchy(Type t)
        {
            if (t.IsGenericType)
            {
                yield return t.GetGenericTypeDefinition();
                foreach (var genarg in t.GetGenericArguments())
                {
                    foreach (var gent in FlattenGenericTypeHierarchy(genarg))
                        yield return gent;
                }
            }
            else
                yield return t;
        }


        private bool IsAllowedClientReferencedAssembly(Assembly assembly)
        {
            return allowedAssemblies.Contains(assembly);
        }


        private static void PeVerify(string dllPath)
        {
            PeVerifyHelper.Verify(dllPath);
        }


        private class InheritedClient : CritterClient
        {
            public InheritedClient(string baseUri, IWebClient webClient)
                : base(baseUri, webClient)
            {
            }
        }

        private class NoopHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}