#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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