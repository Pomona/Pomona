#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

using Critters.Client;
using Critters.Client.Pomona.Common.Proxies;

using NUnit.Framework;

namespace Pomona.IndependentClientTests
{
    [TestFixture]
    public class IndependentClientMetadataTests
    {
        [Test]
        public void AssemblyNameIsCorrect()
        {
            var assembly = typeof(ICritterClient).Assembly;
            var assemblyName = assembly.GetName();
            Assert.That(assemblyName.Name, Is.EqualTo("IndependentCritters"));
        }


        [Test]
        public void NamespaceOfLazyProxyClassesIsCorrect()
        {
            VerifyNamespace<LazyProxyBase>();
        }


        [Test]
        public void NamespaceOfPostAndPatchFormClassesIsCorrect()
        {
            VerifyNamespace<PostResourceBase>();
        }


        private static void VerifyNamespace<T>()
        {
            var assembly = typeof(ICritterClient).Assembly;
            var proxyType = typeof(T);
            var postFormTypes = assembly
                .GetExportedTypes()
                .Where(t => proxyType.IsAssignableFrom(t)
                    // Exclude types that are included from Pomona for assembly independency
                            && !t.Namespace.Contains("Pomona"))
                .ToArray();

            Assert.That(postFormTypes, Has.Length.GreaterThan(0), "No implementations of {0} found.", proxyType);

            var postFormTypesInWrongNamespace = postFormTypes
                .Where(t => t.Namespace != "Critters.Client")
                .ToArray();

            Assert.That(postFormTypesInWrongNamespace,
                        Has.Length.EqualTo(0),
                        "Invalid namespace: {0}",
                        postFormTypesInWrongNamespace.FirstOrDefault());
        }
    }
}